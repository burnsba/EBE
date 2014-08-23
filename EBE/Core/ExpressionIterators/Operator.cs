using System;
using EBE.Core.Utilities;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections;
using System.Linq;
using System.Xml.Linq;

namespace EBE.Core.ExpressionIterators
{
    /// <summary>
    /// Operator for bit expressions.
    /// </summary>
    public class Operator : OperatorBase
    {
        // compiler time constants. Gives an upper limit on the number
        // of possible iterations.
        public const int MaxInternalInput = 1;
        public const int MaxInternalOutput = 0;

        private int _internalInputCount;

        private int _internalOutputCount;

        private int _numberCombinations;

        private int _degreesOfFreedom;

        private List<int[]> _internalOutputEvalMap;

        private List<int> _internalOutputId;

        private int[] _outputEvalMap;

        private int _traditionalOperatorIndex = 1;

        private TraditionalOperator _toperator;

        bool _toStringIsDirty = true;
        string _toStringValue = String.Empty;

        bool _toStringInternalOutputIdIsDirty = true;
        string _toStringInternalOutputIdValue = String.Empty;

        protected Operator(XElement xe)
            : base(xe.Elements((XName)"OperatorBase").FirstOrDefault())
        {
            XElement xel = xe.Elements("InternalInputCount").FirstOrDefault();
            _internalInputCount = int.Parse(xel.Value);

            xel = xe.Elements("InternalOutputCount").FirstOrDefault();
            _internalOutputCount = int.Parse(xel.Value);

            xel = xe.Elements("NumberCombinations").FirstOrDefault();
            _numberCombinations = int.Parse(xel.Value);

            xel = xe.Elements("DegreesOfFreedom").FirstOrDefault();
            _degreesOfFreedom = int.Parse(xel.Value);

            xel = xe.Elements("TraditionalOperatorIndex").FirstOrDefault();
            _traditionalOperatorIndex = int.Parse(xel.Value);

            _internalOutputId = new List<int>();

            xel = xe.Elements("InternalOutputId").FirstOrDefault();
            foreach(var x in xel.Descendants())
            {
                _internalOutputId.Add(int.Parse(x.Value));
            }
        }

        public static Operator FromXElement(XElement xe)
        {
            return new Operator(xe);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EBE.Core.ExpressionIterators.Operator"/> class.
        /// </summary>
        /// <param name="internalInputCount">Internal input count.</param>
        /// <param name="internalOutputCount">Internal output count.</param>
        /// <param name="id">Id.</param>
        /// <param name="internalOutputId">Internal output identifier.</param>
        /// <param name="maxBits">Max number of bits in operator</param>
        public Operator(int internalInputCount, int internalOutputCount, int id, List<int> internalOutputId, int maxBits)
        : base(OperatorType.Bit, maxBits)
        {
            if (internalOutputId == null && _internalOutputCount != 0)
            {
                throw new ArgumentException("InternalOutputCount is set, but no id is given.");
            }

            if (internalOutputId != null && internalOutputId.Count != internalOutputCount)
            {
                throw new ArgumentException("Number of ids does not match number of internalOutputCount.");
            }

            _internalInputCount = internalInputCount;
            _internalOutputCount = internalOutputCount;

            Id = id;

            _internalOutputEvalMap = new List<int[]>();
            _internalOutputId = new List<int>();

            int right = 1 << (1 + _internalOutputCount);
            int pow = 1 << (_internalInputCount + 2);

            if (right == 2)
            {
                _numberCombinations = 1 << pow;
            }
            else
            {
                _numberCombinations = Core.Math.IntegerPow(right, pow);
            }

            _degreesOfFreedom = 2 + 1 + _internalInputCount + _internalOutputCount;

            if (_outputEvalMap != null && _outputEvalMap.Length == _numberCombinations)
            {
                ResetArray(Id, ref _outputEvalMap);
            }
            else
            {
                _outputEvalMap = SetArray(Id);
            }

            for (int i = 0; i < _internalOutputCount; i++)
            {
                _internalOutputId.Add(internalOutputId[i]);
                _internalOutputEvalMap.Add(SetArray(internalOutputId[i]));
            }

            _toStringIsDirty = true;
        }

        /// <summary>
        /// Once bit operator is done iterating, moves to regular operators.
        /// </summary>
        private TraditionalOperator TOperator
        {
            get
            {
                if (_toperator == null)
                {
                    _toperator = new TraditionalOperator(MaxBits);
                }

                return _toperator;
            }
        }

        public override void Reset()
        {
            List<int> outputIds = new List<int>();

            for (int i = 0; i < _internalOutputCount; i++)
            {
                outputIds.Add(0);
            }

            Reset(0, outputIds);
            _toStringIsDirty = true;

            // can't call base.Reset on abstract class
            DoneIterating = false;
            IterationCount = 1;
        }

        private void Reset(int id, List<int> internalOutputId)
        {
            Id = id;
            int right = 1 << (1 + _internalOutputCount);
            int pow = 1 << (_internalInputCount + 2);

            if (right == 2)
            {
                _numberCombinations = 1 << pow;
            }
            else
            {
                _numberCombinations = Core.Math.IntegerPow(right, pow);
            }

            _degreesOfFreedom = 2 + 1 + _internalInputCount + _internalOutputCount;

            if (_outputEvalMap != null && _outputEvalMap.Length == _numberCombinations)
            {
                ResetArray(Id, ref _outputEvalMap);
            }
            else
            {
                _outputEvalMap = SetArray(Id);
            }

            for (int i = 0; i < _internalOutputCount; i++)
            {
                _internalOutputId[i] = internalOutputId[i];
                _internalOutputEvalMap[i] = SetArray(internalOutputId[i]);
            }
        }

        /// <summary>
        /// Gets first instance of bit operator.
        /// </summary>
        public static Operator First(int maxBits)
        {
            return new Operator(0, 0, 0, null, maxBits);
        }

        public Operator FamilyFirst()
        {
            return new Operator(_internalInputCount, _internalOutputCount, 0, _internalOutputId, MaxBits);
        }

        public Operator FamilyLast()
        {
            return new Operator(_internalInputCount, _internalOutputCount, Id - 1, _internalOutputId, MaxBits);
        }

        public int InternalInputCount
        {
            get
            {
                return _internalInputCount;
            }
        }

        public int InternalOutputCount
        {
            get
            {
                return _internalOutputCount;
            }
        }

        public int NumberCombinations
        {
            get
            {
                return _numberCombinations;
            }
        }

        public int DegreesOfFreedom
        {
            get
            {
                return _degreesOfFreedom;
            }
        }

        public override bool MoveNext()
        {
            if (DoneIterating && _traditionalOperatorIndex >= TraditionalOperator.OperatorsLength)
            {
                return false;
            }
            else if (!DoneIterating)
            {
                if (_internalOutputId != null)
                {
                    bool incremented = false;
                    int i;

                    for (i = 0; i < _internalOutputCount; i++)
                    {
                        if (_internalOutputId[i] < _numberCombinations - 1)
                        {
                            _internalOutputId[i] = _internalOutputId[i] + 1;
                            incremented = true;
                            IterationCount++;
                            break;
                        }
                        else
                        {
                            _internalOutputId[i] = 0;
                        }
                    }

                    if (incremented)
                    {
                        _toStringInternalOutputIdIsDirty = true;
                        Reset(Id, _internalOutputId);
                        return true;
                    }
                }

                if (Id < _numberCombinations - 1)
                {
                    _toStringIsDirty = true;
                    _toStringInternalOutputIdIsDirty = true;

                    IterationCount++;
                    Reset(Id + 1, _internalOutputId);

                    return true;
                }

                if (_internalInputCount == _internalOutputCount)
                {
                    if (_internalInputCount + 1 <= MaxInternalInput)
                    {
                        _toStringIsDirty = true;
                        _toStringInternalOutputIdIsDirty = true;
                        _internalInputCount++;

                        IterationCount++;

                        List<int> outputIds = new List<int>();

                        for (int i = 0; i < _internalOutputCount; i++)
                        {
                            outputIds.Add(0);
                        }

                        Reset(0, outputIds);
                        return true;
                    }
                }

                if (_internalOutputCount + 1 <= MaxInternalOutput)
                {
                    _internalOutputCount++;

                    IterationCount++;

                    List<int> outputIds = new List<int>();

                    for (int i = 0; i < _internalOutputCount; i++)
                    {
                        _toStringIsDirty = true;
                        _toStringInternalOutputIdIsDirty = true;

                        outputIds.Add(0);

                        _internalOutputId.Add(0);
                        _internalOutputEvalMap.Add(new int[1]);
                    }

                    Reset(0, outputIds);
                    return true;
                }

                DoneIterating = true;
                return true;
            }
            else
            {
                _traditionalOperatorIndex++;

                if (TOperator.MoveNext())
                {
                    return true;
                }

                return false;
            }
        }

        public override string ToString()
        {
            string output = String.Empty;

            if (!DoneIterating)
            {
                if (_toStringIsDirty == true)
                {
                    _toStringValue = String.Format("{0}.{1}.{2}", _internalInputCount, _internalOutputCount, Id);
                    _toStringIsDirty = false;
                }

                if (_toStringInternalOutputIdIsDirty == true)
                {
                    if (_internalOutputId.Count > 0)
                    {
                        _toStringInternalOutputIdValue = "-" + String.Join("-", _internalOutputId);
                    }

                    _toStringInternalOutputIdIsDirty = false;
                }

                output = _toStringValue + _toStringInternalOutputIdValue;
            }
            else
            {
                output = TOperator.ToString();
            }

            return output;
        }

        private int[] SetArray(int id)
        {
            int[] arr = new int[_numberCombinations];
            int count = 0;

            for (int i = 1; i < _numberCombinations; i <<= 1, count++)
            {
                arr[count] = id & 0x1;
                id >>= 1;
            }

            return arr;
        }

        private void ResetArray(int id, ref int[] arr)
        {
            int count = 0;

            for (int i = 1; i < _numberCombinations; i <<= 1, count++)
            {
                arr[count] = id & 0x1;
                id >>= 1;
            }
        }

        private int BitEval(int a, int b)
        {
            int index = 0;

            index = (a & 0x01) << 1;
            index += (b & 0x01);

            return _outputEvalMap[index];
        }

        /// <summary>
        /// Evaluates two values based on the current operator.
        /// </summary>
        /// <param name="a">First parameter.</param>
        /// <param name="b">Second parameter.</param>
        public override int? Eval(int a, int b)
        {
            int solution = 0;

            if (!DoneIterating)
            {
                int hi_a = Bit.GetHighestBit((uint)a);
                int hi_b = Bit.GetHighestBit((uint)b);
                int hi = hi_a > hi_b ? hi_a : hi_b;
                int i;
                int t;

                for (i = 0; i <= hi; i++)
                {
                    t = BitEval(a & 0x1, b & 0x1);
                    solution |= t << i;
                    a >>= 1;
                    b >>= 1;
                }
            }
            else
            {
                int? sn = TOperator.Eval(a, b);

                if (sn.HasValue)
                {
                    solution = sn.Value;
                }
                else
                {
                    return null;
                }
            }

            return solution & MaxBitValue;
        }

        public new XElement ToXElement()
        {
            XElement root = new XElement("Operator");

            root.Add(new XElement("InternalInputCount", _internalInputCount));
            root.Add(new XElement("InternalOutputCount", _internalOutputCount));
            root.Add(new XElement("NumberCombinations", _numberCombinations));
            root.Add(new XElement("DegreesOfFreedom", _degreesOfFreedom));
            root.Add(new XElement("TraditionalOperatorIndex", _traditionalOperatorIndex));

            XElement internalOutputId = new XElement("InternalOutputId");

            foreach(var i in _internalOutputId)
            {
                internalOutputId.Add(new XElement("int", i));
            }

            root.Add(internalOutputId);

            root.Add(base.ToXElement());

            return root;
        }
    }
}

