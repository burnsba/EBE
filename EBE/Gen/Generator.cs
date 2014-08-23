using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Unix;
using System.Runtime.Serialization;
using System.Xml;
using EBE.Data;
using EBE.Core;
using EBE.Core.ExpressionIterators;
using EBE.Core.Utilities;
using EBE.Core.Evaluation;
using System.Xml.Linq;

namespace EBE
{
    /// <summary>
    /// Generator.
    /// </summary>
    /// <remarks>
    /// some benchmarks
    /// Paren
    ///
    /// 15 - 372693519 - 9 min 29 sec
    /// 14 - 71039373 - 1 min 47 sec
    /// 13 - 13648869 - 0 min 20 sec
    /// 12 - 2646723 - 0 min 3 sec
    /// 11 - 518859 - 0 min 0 sec
    ///
    /// Var
    ///
    /// 13 - 27644437 - 0 min 5 sec
    /// 14 - 190899322 - 0 min 34 sec
    /// 15 - 1382958545 - 4 min 16 sec
    ///
    /// All
    ///
    /// 3 - 1500 - 0 min 0 sec
    /// 4 - 165000 - 0 min 14 sec
    /// 5 - 23400000 - 11 min 48 sec
    /// </remarks>
    public class Generator
    {
        #region Fields

        private int _numVariables = 1;

        private ParenState _paren = null;

        private VarState _var = null;

        private OpState _op = null;

        private int _iterationCount = 0;

        private bool _pauseAfterEach = false;

        private int _maxBits = 1;

        private TimeSpan _saveFrequency = new TimeSpan(0, 10, 0);

        private DateTime _lastSaveTime;

        private bool _receivedInterrupt = false;

        private bool _resumeFromSave = false;
        private bool _pastFirstResume = false;

        private StreamWriter _outputStream;

        private string _saveFilePath = String.Empty;

        private EBEContext _context;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether an interrupt was received, signalling the
        /// need to save context and quit.
        /// </summary>
        /// <value><c>true</c> if received interrupt; otherwise, <c>false</c>.</value>
        public bool ReceivedInterrupt
        {
            get
            {
                return _receivedInterrupt;
            }
            set
            {
                _receivedInterrupt = value;
            }
        }

        /// <summary>
        /// Gets or sets the max number of bits to use when evaluating expressions.
        /// </summary>
        public int MaxBits
        {
            get
            {
                return _maxBits;
            }
            set
            {
                _maxBits = value;
            }
        }

        /// <summary>
        /// Gets or sets the stream to write output to.
        /// </summary>
        /// <value>The output stream.</value>
        public StreamWriter OutputStream
        {
            get
            {
                return _outputStream;
            }
            set
            {
                _outputStream = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the context was loading from a save file.
        /// </summary>
        /// <value>True if loaded from file, false otherwise.</value>
        public bool ResumeFromSave
        {
            get
            {
                return _resumeFromSave;
            }
            set
            {
                _resumeFromSave = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether consle should be read after each iteration.
        /// </summary>
        public bool PauseAfterEach
        {
            get
            {
                return _pauseAfterEach;
            }
            set
            {
                _pauseAfterEach = value;
            }
        }

        public int IterationCount
        {
            get
            {
                return _iterationCount;
            }
        }

        public int DbNew
        {
            get;
            set;
        }

        public int DbSkip
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        private Generator()
        {
            _context = new EBEContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EBE.Generator"/> class.
        /// </summary>
        /// <param name="num">Number of variables.</param>
        /// <param name="outputStream">Where to send output.</param>
        public Generator(int num, StreamWriter outputStream)
        {
            _numVariables = num;
            _outputStream = outputStream;
            _context = new EBEContext();
        }

        #endregion

        #region Methods

        public void DoWork()
        {
            List<string> varNames;
            List<string> opValues;
            string output;
            string line;
            var doWork = true;

            if (_pastFirstResume || _paren == null)
            {
                _paren = new ParenState(_numVariables);
            }

            do
            {
                if (_pastFirstResume || _var == null)
                {
                    _var = new VarState(_numVariables);
                }

                do
                {
                    if (_pastFirstResume || _op == null)
                    {
                        _op = new OpState(_numVariables, _maxBits);
                    }

                    do
                    {
                        _pastFirstResume = true;
                        output = String.Empty;
                        output = _paren.ToString();
                        varNames = _var.VarNames;

                        foreach (var s in varNames)
                        {
                            output = EBE.Core.Utilities.Extensions.ReplaceFirst(output, Placeholder.Var, s);
                        }

                        opValues = _op.OpValues;

                        foreach (var s in opValues)
                        {
                            var sbracket = String.Format(
                                               "{0}{1}{0}",
                                               Placeholder.OpBracket,
                                               s);
                            output = EBE.Core.Utilities.Extensions.ReplaceFirst(output, Placeholder.Op, sbracket);
                        }

                        var evalOutput = new List<string>();
                        Evaluator evaluator = new Evaluator(output, _numVariables, _maxBits);

                        do
                        {
                            int? val = evaluator.Eval();
                            string s = String.Empty;

                            if (!val.HasValue)
                            {
                                s += "n";
                            }
                            else
                            {
                                s += val.Value.ToString();
                            }

                            evalOutput.Add(s);
                        }
                        while (evaluator.MoveNext() && doWork);

                        line = String.Format("{0}.{1}.{2}.{3} {4}",
                                             _numVariables,
                                             _paren.IterationCount,
                                             _var.IterationCount,
                                             _op.IterationCount,
                                             output);

                        _outputStream.WriteLine(line);

                        _outputStream.Flush();

                        Gen g = new Gen();

                        g.Id = Guid.NewGuid();
                        g.ParenId = _paren.IterationCount;
                        g.VariableId = _var.IterationCount;
                        g.OperatorId = _op.IterationCount;
                        g.Expression = output;

                        Encyclopedia e = new Encyclopedia();

                        e.CleanedInput = output;
                        e.EvalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
                        e.Gen = g;
                        e.GenId = g.Id;
                        e.Id = Guid.NewGuid();
                        e.MaxBits = MaxBits;
                        e.ParsedInput = evaluator.Expression.Root.ToString();
                        e.RawEval = String.Join(",", evalOutput);
                        e.RawInput = output;
                        e.Slots = _numVariables;
                        e.Variables = evaluator.Expression.VariableKeys.Count();

                        if (_context.Encyclopedia.Find(
                                    "ParsedInput", e.ParsedInput,
                                    "MaxBits", _maxBits).Count == 0)
                        {
                            // foreign key first
                            _context.Gen.Add(g);
                            _context.Encyclopedia.Add(e);
                            DbNew++;
                        }
                        else
                        {
                            DbSkip++;
                            Console.WriteLine("Skipping " + e.ParsedInput);
                        }

                        _iterationCount++;

                        if (_lastSaveTime < DateTime.Now - _saveFrequency)
                        {
                            _lastSaveTime = DateTime.Now;
                            Save();
                        }

                        if (ReceivedInterrupt)
                        {
                            // output to console or output stream?
                            Console.WriteLine("Stopping generation and attempting to save the current state.");
                            Save();
                            doWork = false;
                        }

                        if (_pauseAfterEach)
                        {
                            Console.ReadLine();
                        }
                    }
                    while (_op.MoveNext() && doWork);
                }
                while (_var.MoveNext() && doWork);
            }
            while (_paren.MoveNext() && doWork);
        }

        public void Save()
        {
            DateTime now = DateTime.Now;
            _saveFilePath = String.Format("t{0}.context",
                                          now.ToString("yyyyMMddHHmmss"));

            var xe = ToXElement();
            var reader = xe.CreateReader();
            reader.MoveToContent();

            File.WriteAllText(_saveFilePath, reader.ReadOuterXml());
        }

        public XElement ToXElement()
        {
            XElement root = new XElement("Generator");

            root.Add(new XElement("NumVariables", _numVariables));
            root.Add(_paren.ToXElement());
            root.Add(_var.ToXElement());
            root.Add(_op.ToXElement());
            root.Add(new XElement("IterationCount", _iterationCount));
            root.Add(new XElement("PauseAfterEach", _pauseAfterEach));
            root.Add(new XElement("MaxBits", _maxBits));

            return root;
        }

        public static Generator FromXElement(XElement xe)
        {
            Generator g = new Generator();

            XElement xel = xe.Elements("NumVariables").FirstOrDefault();
            g._numVariables = int.Parse(xel.Value);

            xel = xe.Elements("IterationCount").FirstOrDefault();
            g._iterationCount = int.Parse(xel.Value);

            xel = xe.Elements("PauseAfterEach").FirstOrDefault();
            g._pauseAfterEach = bool.Parse(xel.Value);

            xel = xe.Elements("MaxBits").FirstOrDefault();
            g._maxBits = int.Parse(xel.Value);

            xel = xe.Elements("ParenState").FirstOrDefault();
            g._paren = ParenState.FromXElement(xel);

            xel = xe.Elements("VarState").FirstOrDefault();
            g._var = VarState.FromXElement(xel);

            xel = xe.Elements("OpState").FirstOrDefault();
            g._op = OpState.FromXElement(xel);

            return g;
        }

        #endregion
    }
}

