using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EBE.Core;

namespace EBE.Core.ExpressionIterators
{
    /// <summary>
    /// Operator state.
    /// </summary>
    [DataContract]
    public class OpState : IteratorBase
    {
        #region Fields

        private const int MaxNumberOfOperators = 10;

        /// <summary>
        /// Number of variables to build expressions for.
        /// </summary>
        [DataMember(Name = "NumVariables", Order = 1)]
        private readonly int _numVariables;

        /// <summary>
        /// Variable state.
        /// </summary>
        [DataMember(Name = "State", Order = 4)]
        private List<Operator> _state = null;

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
        /// Gets the maximum number of times the object will be iterated.
        /// </summary>
        public UInt64 VariablesMaxCount
        {
            get
            {
                UInt64 result = 1;

                for (int i = 0; i < _state.Count; i++)
                {
                    result *= (UInt64)_state[i].NumberCombinations;
                }

                return result;
            }
        }

        /// <summary>
        /// Returns a list of operators as defined by the current state.
        /// </summary>
        public List<String> OpValues
        {
            get
            {
                List<string> output = _state.Select(x => x.ToString()).ToList();
                return output;
            }
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
        public override bool MoveNext()
        {
            if (DoneIterating)
            {
                return false;
            }

            int cursor = _numVariables - 2;
            bool foundIncrement = false;

            // start at the right
            while (cursor >= 0)
            {
                // Can increment current index?
                if (_state[cursor].MoveNext())
                {
                    IterationCount++;
                    foundIncrement = true;
                    break;
                }

                cursor--;
            }

            // reset all to the right of cursor.
            if (foundIncrement)
            {
                cursor++;

                while (cursor <= _numVariables - 2)
                {
                    _state[cursor] = Operator.First();
                    cursor++;
                }

                return true;
            }

            // as a side effect, the state will be set to 1,1,1,1... so fix that
            cursor = _numVariables - 2;

            while (cursor >= 0)
            {
                _state[cursor] = _state[cursor].FamilyLast();
                cursor--;
            }

            DoneIterating = true;
            return false;
        }

        /// <summary>
        /// Resets the enumerator to initial conditions.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _state = new List<Operator>();

            for (int i = 0; i < _numVariables - 1; i++)
            {
                _state.Add(Operator.First());
            }
        }

        /// <summary>
        /// Gets the current state.
        /// </summary>
        /// <returns>String containing operators.</returns>
        public override string ToString()
        {
            string s = String.Empty;

            s += "{";

            List<string> opIds = _state.Select(x => x.ToString()).ToList();
            s += String.Join(", ", opIds);

            s += "}";

            return s;
        }

        #endregion
    }
}

