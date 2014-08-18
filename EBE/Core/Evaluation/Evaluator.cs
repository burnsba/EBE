using System;
using System.Collections.Generic;

namespace EBE.Core.Evaluation
{
    /// <summary>
    /// Evaluates over possible expression variable values.
    /// </summary>
    public class Evaluator
    {
        private Expression _expression;
        private Dictionary<string, int> _variables;
        private List<string> _variableKeys;
        private int _maxBits = 1;

        private int _maxVariableValue = 1;

        private string _rawInput;
        private string _parsedInput;
        private int _numVariables;
        private int _numSlots;
        private Guid _evalId;

        private bool _doneIterating = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EBE.Core.Evaluation.Evaluator"/> class.
        /// </summary>
        private Evaluator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EBE.Core.Evaluation.Evaluator"/> class.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <param name="numSlots">Number of slots.</param>
        /// <param name="maxBits">Max number of bits.</param>
        public Evaluator(string expression, int numSlots, int maxBits)
        {
            _expression = new Expression(maxBits);

            _expression.Parse(expression);

            PostConstructor(_expression, numSlots, maxBits);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EBE.Core.Evaluation.Evaluator"/> class.
        /// </summary>
        /// <param name="expression">Expression.</param>
        /// <param name="numSlots">Number of slots.</param>
        /// <param name="maxBits">Max number of bits.</param>
        public Evaluator(Expression expression, int numSlots, int maxBits)
        {
            _expression = expression;

            PostConstructor(_expression, numSlots, maxBits);
        }

        /// <summary>
        /// Helper to set initial values.
        /// </summary>
        /// <param name="expression">Expression to use.</param>
        /// <param name="numSlots">Number of slots.</param>
        /// <param name="maxBits">Max number of bits.</param>
        private void PostConstructor(Expression expression, int numSlots, int maxBits)
        {
            _variables = _expression.Variables;
            _variableKeys = _expression.VariableKeys;

            foreach(var key in _variableKeys)
            {
                _variables[key] = _expression.Variables[key];
            }

            _numVariables = _variables.Keys.Count;
            _numSlots = numSlots;

            // sets _maxVariableValue too
            MaxBits = maxBits;
        }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public Expression Expression
        {
            get
            {
                return _expression;
            }

            private set
            {
                _expression = value;
            }
        }

        /// <summary>
        /// Gets or sets the max number of bits.
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

                _maxVariableValue = (1 << (_maxBits)) - 1;
            }
        }

        /// <summary>
        /// Gets the raw input.
        /// </summary>
        public string RawInput
        {
            get
            {
                return _rawInput;
            }

            private set
            {
                _rawInput = value;
            }
        }

        /// <summary>
        /// Gets the parsed input.
        /// </summary>
        public string ParsedInput
        {
            get
            {
                return _parsedInput;
            }

            private set
            {
                _parsedInput = value;
            }
        }

        /// <summary>
        /// Gets the number variables.
        /// </summary>
        public int NumVariables
        {
            get
            {
                return _numVariables;
            }

            private set
            {
                _numVariables = value;
            }
        }

        /// <summary>
        /// Gets the number of slots.
        /// </summary>
        public int NumSlots
        {
            get
            {
                return _numSlots;
            }

            private set
            {
                _numSlots = value;
            }
        }

        /// <summary>
        /// Gets the unique identifier for evaluation string.
        /// </summary>
        public Guid EvalId
        {
            get
            {
                return _evalId;
            }

            private set
            {
                _evalId = value;
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        public bool MoveNext()
        {
            if(_doneIterating)
            {
                return false;
            }

            foreach(var key in _variableKeys)
            {
                _variables[key] = _variables[key] + 1;

                if(_variables[key] > _maxVariableValue)
                {
                    _variables[key] = 0;
                }
                else
                {
                    return true;
                }
            }

            // couldn't increment, all values are zero so reset to maxvalue
            foreach(var key in _variableKeys)
            {
                _variables[key] = _maxVariableValue;
            }

            _doneIterating = true;

            return false;
        }

        /// <summary>
        /// Evaluates the current iteration.
        /// </summary>
        public int? Eval()
        {
            return EvalHelper(_expression.Root);
        }

        /// <summary>
        /// Recursive helper.
        /// </summary>
        /// <returns>Expression value.</returns>
        /// <param name="node">Node to parse.</param>
        private int? EvalHelper(ExpressionNode node)
        {
            if(node.NodeType == NodeType.Leaf)
            {
                return _variables[node.Value];
            }

            int left;
            int right;

            int? leftn = EvalHelper(node.Left);

            if(leftn.HasValue)
            {
                left = leftn.Value;
            }
            else
            {
                return null;
            }

            int? rightn = EvalHelper(node.Right);

            if(rightn.HasValue)
            {
                right = rightn.Value;
            }
            else
            {
                return null;
            }
          
            return node.Operator.Eval(left, right);
        }
    }
}

