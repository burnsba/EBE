using System;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.Generic;
using EBE.Core.ExpressionIterators;
using System.Xml.Linq;

namespace EBE.Core.ExpressionIterators
{
    /// <summary>
    /// Common base class for operators.
    /// </summary>
    public abstract class OperatorBase : IteratorBase
    {
        protected internal enum OperatorType
        {
            Unknown,

            Bit,

            Traditional
        }

        private OperatorType _operatorType = OperatorType.Unknown;

        private int _maxBits;

        private int _maxBitValue;
        private bool _maxBitValueSet = false;

        protected OperatorBase(XElement xe)
            : base(xe)
        {
            XElement xel = xe.Elements("MaxBits").FirstOrDefault();
            _maxBits = int.Parse(xel.Value);

            xel = xe.Elements("OperatorType").FirstOrDefault();
            _operatorType = (OperatorType)Enum.Parse(typeof(OperatorType), xel.Value);
        }

        protected OperatorBase(OperatorType type, int maxBits)
        {
            _operatorType = type;

            _maxBits = maxBits;
        }

        /// <summary>
        /// Describes the highest bit position for operations.
        /// </summary>
        /// <value>The max bits.</value>
        public int MaxBits
        {
            get
            {
                return _maxBits;
            }

            protected set
            {
                _maxBits = value;
            }
        }

        public int MaxBitValue
        {
            get
            {
                if (_maxBitValueSet == false)
                {
                    _maxBitValue = (1 << _maxBits) - 1;
                }

                return _maxBitValue;
            }
        }

        public abstract int? Eval(int a, int b);

        public override abstract bool MoveNext();

        public override abstract void Reset();

        /// <summary>
        /// Parse the specified expression to create an operator.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <param name="maxBits">Max bits.</param>
        public static OperatorBase Parse(string expression, int maxBits)
        {
            expression = expression.Replace(":", "");

            if (String.IsNullOrEmpty(expression))
            {
                throw new FormatException("Can not create operator from empty string.");
            }

            if (expression.Count() == 1)
            {
                char c = expression[0];

                switch (c)
                {
                    case '+':
                        return new TraditionalOperator(maxBits)
                        {
                            MaxBits = maxBits
                        };

                    case '-':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 2, MaxBits = maxBits
                        };

                    case '*':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 3, MaxBits = maxBits
                        };

                    case '/':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 4, MaxBits = maxBits
                        };

                    case '%':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 5, MaxBits = maxBits
                        };

                    case '<':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 6, MaxBits = maxBits
                        };

                    case '>':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 7, MaxBits = maxBits
                        };

                    case '&':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 8, MaxBits = maxBits
                        };

                    case '|':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 9, MaxBits = maxBits
                        };

                    case '^':
                    return new TraditionalOperator(maxBits)
                        {
                            IterationCount = 10, MaxBits = maxBits
                        };

                    default:
                        throw new FormatException("Unrecognized operator: " + expression);
                }
            }

            var ids = expression.Split('.');

            if (ids.Count() < 3 || ids.Count() > 4)
            {
                throw new FormatException("Bit expression operator is malformed: " + expression);
            }

            int internalInputCount = int.Parse(ids[0]);
            int internalOutputCount = int.Parse(ids[1]);
            int id = int.Parse(ids[2]);

            List<int> internalOutputId = null;

            if (ids.Length > 3)
            {
                internalOutputId = ids[3].Split('-').ToList().Select(x => int.Parse(x)).ToList();
            }

            Operator op = new Operator(internalInputCount, internalOutputCount, id, internalOutputId, maxBits);
            op.MaxBits = maxBits;
            return op;
        }

        public new XElement ToXElement()
        {
            XElement root = new XElement("OperatorBase");

            root.Add(new XElement("OperatorType", _operatorType));
            root.Add(new XElement("MaxBits", _maxBits));

            root.Add(base.ToXElement());

            return root;
        }
    }
}

