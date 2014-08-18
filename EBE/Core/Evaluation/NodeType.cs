using System;

namespace EBE.Core.Evaluation
{
    /// <summary>
    /// Identifies the type of node on an expression tree.
    /// </summary>
    internal enum NodeType
    {
        Unknown,

        /// <summary>
        /// Node with no children.
        /// </summary>
        Leaf,

        /// <summary>
        /// Node with at least one child.
        /// </summary>
        Node
    }
}

