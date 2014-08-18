using System;
using EBE.Core.Utilities;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections;
using System.Linq;

namespace EBE.Core.ExpressionIterators
{
	/// <summary>
	/// Used to evaluate "regular" operators like +,-,*, etc.
	/// </summary>
    [DataContract]
	public class TraditionalOperator : OperatorBase
    {
		/// <summary>
		/// Compile time constant. Length of operators collection.
		/// </summary>
		internal const int OperatorsLength = 10;

		/// <summary>
		/// List of operators used to iterate over.
		/// </summary>
		private static List<string> _operators = new List<string>()
		{
			"+",
			"-",
			"*",
			"/",
			"%",
			"<",
			">",
			"&",
			"|",
			"^"
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="EBE.Core.ExpressionIterators.Operator"/> class.
		/// </summary>
		public TraditionalOperator() : base(OperatorType.Traditional)
        {
            
        }
        
		/// <inheritdoc/>
        public override bool MoveNext()
        {
            if (DoneIterating)
            {
                return false;
            }

			IterationCount++;

			if (IterationCount >= OperatorsLength)
			{
				DoneIterating = true;
				return false;
			}

			return true;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="EBE.Core.ExpressionIterators.TraditionalOperator"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="EBE.Core.ExpressionIterators.TraditionalOperator"/>.</returns>
        public override string ToString()
        {
			return _operators [IterationCount - 1];
        }

        /// <summary>
        /// Reset this instance.
        /// </summary>
        public override void Reset()
        {
            // can't call base.Reset on abstract class
            DoneIterating = false;
            IterationCount = 1;
        }

		/// <summary>
		/// Evaluates a operator b, based on the current operator. If the result of the operation
		/// is undefined (divide by zero) or an overflow occurs, null is returned.
		/// </summary>
		/// <param name="a">The first parameter.</param>
		/// <param name="b">The second parameter.</param>
        public override int? Eval(int a, int b)
        {
			if (DoneIterating)
			{
				return null;
			}

            int solution = 0;

			string op = _operators [IterationCount - 1];

			switch (op)
			{
				case "+":
                    try
                    {
    					solution = checked(a + b);
                    }
                    catch (OverflowException)
                    {
                        return null;
                    }
					break;
				case "-":
                    try
                    {
                        solution = checked(a - b);
                    }
                    catch (OverflowException)
                    {
                        return null;
                    }
					break;
				case "*":
                    try
                    {
                        solution = checked(a * b);
                    }
                    catch (OverflowException)
                    {
                        return null;
                    }
					break;
				case "/":
					if (b == 0)
					{
						return null;
					}
					solution = a / b;
					break;
				case "%":
					if (b == 0)
					{
						return null;
					}
					solution = a % b;
					break;
				case "<":
					solution = a << b;
					break;
				case ">":
					solution = a >> b;
					break;
				case "&":
					solution = a & b;
					break;
				case "|":
					solution = a | b;
					break;
				case "^":
					solution = a ^ b;
					break;
				default:
					return null;
			}

			return solution & MaxBitValue;
        }
    }
}

