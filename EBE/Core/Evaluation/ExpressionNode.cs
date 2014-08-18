using System;
using EBE.Core.ExpressionIterators;

namespace EBE.Core.Evaluation
{
    /// <summary>
    /// Node in expression tree.
    /// </summary>
    public class ExpressionNode
    {
        private OperatorBase _operator;
        private ExpressionNode _left;
        private ExpressionNode _right;
        private string _value;

        private NodeType _nodeType = NodeType.Unknown;

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        public OperatorBase Operator
        {
            get
            {
                return _operator;
            }
            set
            {
                _operator = value;
                _nodeType = NodeType.Node;
            }
        }

        /// <summary>
        /// Gets or sets the node value.
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _nodeType = NodeType.Leaf;
            }
        }

        /// <summary>
        /// Gets or sets the left node.
        /// </summary>
        public ExpressionNode Left
        {
            get
            {
                return _left;
            }
            set
            {
                _left = value;
                _nodeType = (_left != null || _right != null) ? NodeType.Node : NodeType.Leaf;
            }
        }

        /// <summary>
        /// Gets or sets the right node.
        /// </summary>
        public ExpressionNode Right
        {
            get
            {
                return _right;
            }
            set
            {
                _right = value;
                _nodeType = (_left != null || _right != null) ? NodeType.Node : NodeType.Leaf;
            }
        }

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        internal NodeType NodeType
        {
            get
            {
                return _nodeType;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="EBE.Core.Evaluation.ExpressionNode"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="EBE.Core.Evaluation.ExpressionNode"/>.</returns>
        public override string ToString()
        {
            if (NodeType == NodeType.Leaf)
            {
                return Value;
            }

            string s = String.Empty;

            if (_left != null)
            {
                if (_left.NodeType == NodeType.Leaf)
                {
                    s += _left.Value;
                }
                else
                {
                    s += _left.ToString();
                }
            }
            else
            {
                s += "{null}";
            }

            s += " " + _operator.ToString() + " ";

            if (_right != null)
            {
                if (_right.NodeType == NodeType.Leaf)
                {
                    s += _right.Value;
                }
                else
                {
                    s += _right.ToString();
                }
            }
            else
            {
                s += "{null}";
            }

            return s;
        }
    }
}

