using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EBE
{
	/// <summary>
	/// Operator state.
	/// </summary>
    [DataContract]
	public class OpState : IEnumerator
	{
		#region Fields

		private const int MaxNumberOfOperators = 10;

		/// <summary>
		/// Number of variables to build expressions for.
		/// </summary>
        [DataMember(Name="NumVariables", Order = 1)]
        private readonly int _numVariables;

		/// <summary>
		/// Number of times object has been enumerated.
		/// </summary>
		/// <remarks>
		/// First entry starts at 1.
		/// </remarks>
        [DataMember(Name="IterationCount", Order = 2)]
		private int _iterationCount = 1;

		/// <summary>
		/// Flag to indicate if the object can be iterated.
		/// </summary>
        [DataMember(Name="DoneIterating", Order = 3)]
		private bool _doneIterating = false;

		/// <summary>
		/// Variable state.
		/// </summary>
        [DataMember(Name="State", Order = 4)]
		private List<int> _state = null;

		/// <summary>
		/// List of operators used.
		/// </summary>
        /// <remarks>
        /// Should be const, but can't have const arrays. Can't make it readonly 
        /// due to deserialization.
        /// </remarks>
        private string[] _operators;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of variables to build expressions for.
		/// </summary>
		public int VariablesCount
		{
			get
			{
				return _numVariables;
			}
		}

		/// <summary>
		/// Gets the maximum number of times the object will be iterated. If the number
		/// of variables is larger than 19, an overflow exception occurs.
		/// </summary>
		public UInt64 VariablesMaxCount
		{
			get
			{
				switch (_numVariables - 1)
				{
					case (0): return 0;
					case (1): return 10;
					case (2): return 100;
					case (3): return 1000;
					case (4): return 10000;
					case (5): return 100000;
					case (6): return 1000000;
					case (7): return 10000000;
					case (8): return 100000000;
					case (9): return 1000000000;
					case (10): return 10000000000;
					case (11): return 100000000000;
					case (12): return 1000000000000;
					case (13): return 10000000000000;
					case (14): return 100000000000000;
					case (15): return 1000000000000000;
					case (16): return 10000000000000000;
					case (17): return 100000000000000000;
					case (18): return 1000000000000000000;
					case (19): return 10000000000000000000;
					default:
					throw new OverflowException();
				}
			}
		}

		/// <summary>
		/// Gets the number of times object has been enumerated.
		/// </summary>
		public int IterationCount
		{
			get
			{
				return _iterationCount;
			}
		}

		/// <summary>
		/// Gets the current state.
		/// </summary>
		public string Current
		{
			get { return ToString(); }
		}

		/// <summary>
		/// Gets a value indicating whether there additional items to iterate over.
		/// </summary>
		public bool CanIterate
		{
			get
			{
				return !_doneIterating;
			}
		}

		/// <summary>
		/// Returns a list of operators as defined by the current state.
		/// </summary>
		public List<String> OpValues
		{
			get
			{
				List<string> output = new List<string>();

				foreach (var i in _state)
				{
					output.Add(IntToOperator(i));
				}

				return output;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			get { return ToString(); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="OpState"/>.
		/// </summary>
		/// <param name="num">Number of variables.</param>
		public OpState(int num)
		{
            OnCreated();

			_numVariables = num;

			Reset();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Advances the enumerator to the next possible operator combination.
		/// </summary>
		/// <returns>True if the state changed, false if there is not a next item.</returns>
		public bool MoveNext()
		{
			if (_doneIterating)
			{
				return false;
			}

			int cursor = _numVariables - 2;

			// start at the right
			while (cursor >= 0)
			{
				// Can increment current index?
				if (_state[cursor] + 1 <= MaxNumberOfOperators)
				{
					_state[cursor]++;

					_iterationCount++;

					return true;
				}

				// Can't increment. There's going to be a "wrap around"
				_state[cursor] = 1;

				cursor--;
			}

			// as a side effect, the state will be set to 1,1,1,1... so fix that
			cursor = _numVariables - 2;
			while (cursor >= 0)
			{
				_state[cursor--] = MaxNumberOfOperators;
			}

			_doneIterating = true;
			return false;
		}

		/// <summary>
		/// Resets the enumerator to initial conditions.
		/// </summary>
		public void Reset()
		{
			_doneIterating = false;

			_iterationCount = 1;

			_state = new List<int>();

			for (int i=0; i<_numVariables - 1; i++)
			{
				_state.Add(1);
			}
		}

		/// <summary>
		/// Dispose.
		/// </summary>
		public void Dispose()
		{
			// nothing to do
		}

		/// <summary>
		/// Gets the current state.
		/// </summary>
		/// <returns>String containing operators.</returns>
		public override string ToString()
		{
			string s = String.Empty;

			s += "{";

			int i;
			for (i = 0; i < _numVariables - 2; i++)
			{
				s += IntToOperator(_state[i]) + ", ";
			}

			s += IntToOperator(_state[i]);

			s += "}";

			return s;
		}

		/// <summary>
		/// Helper function to convert an int into a string.
		/// </summary>
		/// <param name="num">Number to convert.</param>
		/// <returns>String containing operator.</returns>
		private string IntToOperator(int num)
		{
			return _operators[num];
		}

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            OnCreated();
        }

        private void OnCreated()
        {
            _operators = new string[]
            {
                String.Empty,

                // start entries at index 1

                "+",
                "-",
                "*",
                "/",
                "%",
                ">",
                "<",
                "&",
                "|",
                "^"
            };
        }

		#endregion
	}
}

