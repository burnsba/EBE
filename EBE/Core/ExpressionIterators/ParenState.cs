using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EBE.Core;
using System.Xml.Linq;

namespace EBE.Core.ExpressionIterators
{
    /*
     * Some benchmarks
     *
     * 15 - 372693519 - 9 min 29 sec
     * 14 - 71039373 - 1 min 47 sec
     * 13 - 13648869 - 0 min 20 sec
     * 12 - 2646723 - 0 min 3 sec
     * 11 - 518859 - 0 min 0 sec
     * */

    /// <summary>
    /// Used to build expressions containing non-redundant parentheses and output as string. Every well-formed possibility is enumerated.
    /// Parentheses around a single variable or around the entire expression are ignored.
    /// </summary>
    /// <remarks>
    /// https://oeis.org/A001003
    /// </remarks>
    public class ParenState : IteratorBase
    {
        #region Fields

        /// <summary>
        /// Operator to be used in output.
        /// </summary>
        private const string Op = Placeholder.Op;

        /// <summary>
        /// Variable to be used in output.
        /// </summary>
        private const string Var = Placeholder.Var;

        /// <summary>
        /// Number of variables to build expressions for.
        /// </summary>
        private int _numVariables;

        /// <summary>
        /// Current number of sets.
        /// </summary>
        private int _setLength;

        /// <summary>
        /// Describes current sub partitions.
        /// </summary>
        private List<int> _sets = null;

        /// <summary>
        /// Parentheses state for the object.
        /// </summary>
        private List<int> _state = null;

        /// <summary>
        /// Children for recursion.
        /// </summary>
        private List<ParenState> _subIps = null;

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
        /// of variables is larger than 28, an overflow exception occurs.
        /// </summary>
        public UInt64 VariablesMaxCount
        {
            get
            {
                switch (_numVariables)
                {
                    case (1):
                        return 1;

                    case (2):
                        return 3;

                    case (3):
                        return 11;

                    case (4):
                        return 45;

                    case (5):
                        return 197;

                    case (6):
                        return 903;

                    case (7):
                        return 4279;

                    case (8):
                        return 20793;

                    case (9):
                        return 103049;

                    case (10):
                        return 518859;

                    case (11):
                        return 2646723;

                    case (12):
                        return 13648869;

                    case (13):
                        return 71039373;

                    case (14):
                        return 372693519;

                    case (15):
                        return 1968801519;

                    case (16):
                        return 10463578353;

                    case (17):
                        return 55909013009;

                    case (18):
                        return 300159426963;

                    case (19):
                        return 1618362158587;

                    case (20):
                        return 8759309660445;

                    case (21):
                        return 47574827600981;

                    case (22):
                        return 259215937709463;

                    case (23):
                        return 1416461675464871;

                    case (24):
                        return 7760733824437545;

                    case (25):
                        return 42624971294485657;

                    case (26):
                        return 234643073935918683;

                    case (27):
                        return 1294379445480318899;

                    case (28):
                        return 7154203054548921813;

                    default:
                        throw new OverflowException();
                }
            }
        }

        public List<int> Sets
        {
            get
            {
                return _sets;
            }
        }

        public List<ParenState> SubIps
        {
            get
            {
                return _subIps;
            }
        }

        #endregion

        #region Constructors

        protected ParenState(XElement xe)
            : base(xe)
        {
            XElement xel = xe.Elements("NumVariables").FirstOrDefault();
            _numVariables = int.Parse(xel.Value);

            xel = xe.Elements("SetLength").FirstOrDefault();
            _setLength = int.Parse(xel.Value);

            _state = new List<int>();
            _sets = new List<int>();
            _subIps = new List<ParenState>();

            DoneIterating = _numVariables <= 2;

            xel = xe.Elements("Sets").FirstOrDefault();
            foreach(var x in xel.Descendants())
            {
                _sets.Add(int.Parse(x.Value));
            }

            xel = xe.Elements("State").FirstOrDefault();
            foreach(var x in xel.Descendants())
            {
                _state.Add(int.Parse(x.Value));
            }

            xel = xe.Elements("SubIps").FirstOrDefault();
            foreach(var x in xel.Descendants())
            {
                _subIps.Add(ParenState.FromXElement(x));
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ParenState"/>.
        /// </summary>
        /// <param name="num">Number of variables.</param>
        public ParenState(int num)
        {
            _numVariables = num;
            _state = new List<int>();

            for (int i = 0; i < _numVariables; i++)
            {
                _state.Add(0);
            }

            _subIps = new List<ParenState>();
            Reset();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>True if the current item changed, false if there is not a next item.</returns>
        /// <remarks>
        /// Pseudocode outline is as follows:
        ///
        /// Given n variables,
        ///     Iterate over all possible sets
        ///         For each partition in the set
        ///             Increment from right to left until no more partitions can be incremented
        ///
        /// In general, the "most significat bit" will be array index 0
        /// </remarks>
        public override bool MoveNext()
        {
            if (DoneIterating)
            {
                return false;
            }

            int right = _setLength - 1;
            int left = 0;
            int cursor;
            bool incrementSets = false;

            // Increment from right to left.
            for (int i = _setLength - 1; i >= 0 && i < _subIps.Count; i--)
            {
                // if an item on the left was incremented, reset everything to the right
                if (_subIps[i].MoveNext())
                {
                    for (int j = i + 1; j < _setLength && j < _subIps.Count; j++)
                    {
                        _subIps[j].Reset();
                    }

                    IterationCount++;
                    return true;
                }
            }

            #region Description of increment algorithm
            // Done with the recursion part, onto the increment part.
            //
            // For n=6, sets would be:
            // _setLength = 2
            // {5, 1}
            // {4, 2}
            // {3, 3}
            // ...
            // _setLength = 3
            // {4, 1, 1}
            // {3, 2, 1}
            // {3, 1, 2}
            // ...
            //
            // Outline:
            //     1) Start at the right and travel left until the first value larger than 1 is found.
            //   2.1) Check if right and left are not neigherbors:
            // 2.1.1) Carry involves taking the "extras" from the right and adding them to the right of
            //            the next partition to the left > 1. And then the left partition is decremented.
            //   2.2) Left and right are neighbors:
            // 2.2.1) Simply take one from the left and move it right.
            //     3) "Most reduced form" is when the right-most partition has all the "extras" and everything else is 1.
            //            In that case, incrementing is done for current _setLength.
            //     4) If (_setLength+1 == _numVariables) then done, else _setLength++ and goto 1)
            //
            // At the same time the partitions are being incremented, the parentheses state is being updated.
            #endregion

            // start at the right and travel left until the first value larger than 1 is found
            for (cursor = right - 1; cursor >= 0; cursor--)
            {
                if (_sets[cursor] > 1)
                {
                    left = cursor;
                    break;
                }
            }

            // Check if right and left are neighbors. If not, carry.
            if (right - left > 1)
            {
                if (cursor == -1)
                {
                    // couldn't find another partition, must be the last iteration for this set
                    incrementSets = true;
                }
                else
                {
                    // need to "carry".
                    // take the right most partition and push the extras around to the
                    // next leftmost column with values
                    int diff = _sets[_setLength - 1] - 1;
                    _sets[_setLength - 1] -= diff;
                    _sets[left]--;
                    _sets[left + 1] += diff + 1;
                }
            }
            // edge cases not otherwise handled
            else if (_setLength == 1 || (_setLength == 2 && _sets[left] == 1))
            {
                incrementSets = true;
            }
            else
            {
                // "push" the 1 to the right
                _sets[left]--;
                _sets[right]++;
            }

            if (incrementSets)
            {
                _setLength++;

                if (_setLength == _numVariables)
                {
                    DoneIterating = true;
                    _setLength--;
                    return false;
                }

                _sets.Clear();
                _sets.Add(_numVariables - _setLength + 1);

                for (int i = 1; i < _setLength; i++)
                {
                    _sets.Add(1);
                }
            }

            _subIps.Clear();

            // do this in reverse order
            for (int i = 0; i < _setLength; i++)
            {
                _subIps.Add(new ParenState(_sets[i]));
            }

            IterationCount++;
            return true;
        }

        /// <summary>
        /// Resets the enumerator to initial conditions.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            DoneIterating = _numVariables <= 2;

            _sets = new List<int>();

            _setLength = 1;
            _sets.Add(_numVariables);

            _subIps.Clear();
        }

        /// <summary>
        /// Gets the values of the sets currently being used.
        /// </summary>
        /// <returns>String containing values for the current sets.</returns>
        public string SetsToString()
        {
            return String.Join(",", _sets.Select(x => x.ToString()).ToList().ToArray());
        }

        /// <summary>
        /// Gets the current state as an expression.
        /// </summary>
        /// <returns>String containing expression containing parentheses state.</returns>
        public override string ToString()
        {
            string s = String.Empty;

            if (_setLength == 1)
            {
                if (_numVariables == 1)
                {
                    s = Var;
                }
                else
                {
                    for (int i = 0; i < _sets[0] - 1; i++)
                    {
                        s += Var + Op;
                    }

                    s += Var;
                }
            }
            else
            {
                int i;

                for (i = 0; i < _setLength - 1; i++)
                {
                    if (_sets[i] == 1)
                    {
                        s += Var;
                    }
                    else
                    {
                        s += "(" + _subIps[i].ToString() + ")";
                    }

                    s += Op;
                }

                if (_sets[i] == 1)
                {
                    s += Var;
                }
                else
                {
                    s += "(" + _subIps[i].ToString() + ")";
                }
            }

            return s;
        }

        public new XElement ToXElement()
        {
            XElement root = new XElement("ParenState");

            root.Add(new XElement("NumVariables", _numVariables));
            root.Add(new XElement("SetLength", _setLength));

            XElement sets = new XElement("Sets");

            foreach(var i in _sets)
            {
                sets.Add(new XElement("int", i));
            }

            root.Add(sets);

            XElement state = new XElement("State");

            foreach(var i in _state)
            {
                state.Add(new XElement("int", i));
            }

            root.Add(state);

            XElement subIps = new XElement("SubIps");

            foreach(var p in _subIps)
            {
                subIps.Add(p.ToXElement());
            }

            root.Add(subIps);

            root.Add(base.ToXElement());

            return root;
        }

        public static ParenState FromXElement(XElement xe)
        {
            return new ParenState(xe);
        }

        #endregion
    }
}

