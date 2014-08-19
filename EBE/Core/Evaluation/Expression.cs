using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using EBE.Core.ExpressionIterators;
using System.Text.RegularExpressions;

namespace EBE.Core.Evaluation
{
    /// <summary>
    /// Expression containing operators and values.
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/questions/17568067/how-to-parse-a-boolean-expression-and-load-it-into-a-class/17572545#17572545
    /// </remarks>
    public class Expression
    {
        private int _maxBits;

        private Dictionary<string, int> _variables;
        private List<string> _variableKeys;

        private ExpressionNode _root;

        private static Regex rgx = new Regex("[0-9]", RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="EBE.Core.Evaluation.Expression"/> class.
        /// </summary>
        /// <param name="maxBits">Max number of bits.</param>
        public Expression(int maxBits)
        {
            _variables = new Dictionary<string, int>();
            _variableKeys = new List<string>();
            _maxBits = maxBits;
        }

        /// <summary>
        /// Gets the root node of the expression.
        /// </summary>
        public ExpressionNode Root
        {
            get
            {
                return _root;
            }

            private set
            {
                _root = value;
            }
        }

        /// <summary>
        /// Map for variables to values.
        /// </summary>
        public Dictionary<string, int> Variables
        {
            get
            {
                return _variables;
            }
        }

        /// <summary>
        /// Gets the variable keys.
        /// </summary>
        /// <value>The variable keys.</value>
        public List<string> VariableKeys
        {
            get
            {
                return _variableKeys;
            }
        }

        /// <summary>
        /// Parses string into expression.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        public void Parse(string expression)
        {
            List<Token> tokens = new List<Token>();
            StringReader reader = new StringReader(expression);

            //Tokenize the expression
            Token t = null;

            do
            {
                t = new Token(reader);
                tokens.Add(t);
            }
            while (t.type != Token.TokenType.EXPR_END);

            //Use a minimal version of the Shunting Yard algorithm to transform the token list to polish notation
            List<Token> polishNotation = TransformToPolishNotation(tokens);

            var enumerator = polishNotation.GetEnumerator();
            enumerator.MoveNext();
            Root = Make(ref enumerator);
        }

        /// <summary>
        /// Parser helper.
        /// </summary>
        /// <param name="polishNotationTokensEnumerator">Polish notation tokens enumerator.</param>
        private ExpressionNode Make(ref List<Token>.Enumerator polishNotationTokensEnumerator)
        {
            if (polishNotationTokensEnumerator.Current.type == Token.TokenType.LITERAL)
            {
                ExpressionNode lit = new ExpressionNode();
                lit.Value = polishNotationTokensEnumerator.Current.value;
                int intValue = 0;

                if (rgx.IsMatch(lit.Value))
                {
                    intValue = int.Parse(lit.Value);
                }

                if (!_variables.ContainsKey(lit.Value))
                {
                    _variables.Add(lit.Value, intValue);
                    _variableKeys.Add(lit.Value);
                }

                polishNotationTokensEnumerator.MoveNext();
                return lit;
            }
            else if (polishNotationTokensEnumerator.Current.type == Token.TokenType.BINARY_OP ||
                     polishNotationTokensEnumerator.Current.type == Token.TokenType.TBINARY_OP)
            {
                ExpressionNode node = new ExpressionNode();
                node.Operator = OperatorBase.Parse(polishNotationTokensEnumerator.Current.value, _maxBits);
                polishNotationTokensEnumerator.MoveNext();
                node.Left = Make(ref polishNotationTokensEnumerator);
                node.Right = Make(ref polishNotationTokensEnumerator);
                return node;
            }

            return null;
        }

        /// <summary>
        /// Helper functino to transform to polish notation.
        /// </summary>
        /// <returns>List of tokens in polish notation.</returns>
        /// <param name="infixTokenList">Infix token list.</param>
        private static List<Token> TransformToPolishNotation(List<Token> infixTokenList)
        {
            Queue<Token> outputQueue = new Queue<Token>();
            Stack<Token> stack = new Stack<Token>();
            int index = 0;

            while (infixTokenList.Count > index)
            {
                Token t = infixTokenList[index];

                switch (t.type)
                {
                    case Token.TokenType.LITERAL:
                        outputQueue.Enqueue(t);
                        break;

                    case Token.TokenType.TBINARY_OP:
                    case Token.TokenType.BINARY_OP:
                    case Token.TokenType.UNARY_OP:
                    case Token.TokenType.OPEN_PAREN:
                        stack.Push(t);
                        break;

                    case Token.TokenType.CLOSE_PAREN:
                        while (stack.Peek().type != Token.TokenType.OPEN_PAREN)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }

                        stack.Pop();

                        if (stack.Count > 0 && stack.Peek().type == Token.TokenType.UNARY_OP)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }

                        break;

                    default:
                        break;
                }

                ++index;
            }

            while (stack.Count > 0)
            {
                outputQueue.Enqueue(stack.Pop());
            }

            return outputQueue.Reverse().ToList();
        }
    }
}

