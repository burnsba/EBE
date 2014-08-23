using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace EBE.Core.ExpressionIterators
{
    /* Benchmarks
     *
     * 13 - 27644437 - 0 min 5 sec
     * 14 - 190899322 - 0 min 34 sec
     * 15 - 1382958545 - 4 min 16 sec
     * */

    /// <summary>
    /// Variable state.
    /// </summary>
    public class VarState : IteratorBase
    {
        #region Fields

        /// <summary>
        /// Number of variables to build expressions for.
        /// </summary>
        private readonly int _numVariables;

        /// <summary>
        /// Variable state.
        /// </summary>
        private List<int> _state = null;

        /// <summary>
        /// Number of unique variables in the current iteration.
        /// </summary>
        private int _uniqueVariablesCount = 0;

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
        /// Gets the number of unique variables in the current iteration.
        /// </summary>
        public int UniqueVariablesCount
        {
            get
            {
                return _uniqueVariablesCount;
            }
        }

        /// <summary>
        /// Gets the maximum number of times the object will be iterated. If the number
        /// of variables is larger than 25, an overflow exception occurs.
        /// </summary>
        /// <remarks>
        /// Bell numbers
        /// https://oeis.org/A000110
        /// </remarks>
        public UInt64 VariablesMaxCount
        {
            get
            {
                switch (_numVariables)
                {
                    case (1):
                        return 1;

                    case (2):
                        return 2;

                    case (3):
                        return 5;

                    case (4):
                        return 15;

                    case (5):
                        return 52;

                    case (6):
                        return 203;

                    case (7):
                        return 877;

                    case (8):
                        return 4140;

                    case (9):
                        return 21147;

                    case (10):
                        return 115975;

                    case (11):
                        return 678570;

                    case (12):
                        return 4213597;

                    case (13):
                        return 27644437;

                    case (14):
                        return 190899322;

                    case (15):
                        return 1382958545;

                    case (16):
                        return 10480142147;

                    case (17):
                        return 82864869804;

                    case (18):
                        return 682076806159;

                    case (19):
                        return 5832742205057;

                    case (20):
                        return 51724158235372;

                    case (21):
                        return 474869816156751;

                    case (22):
                        return 4506715738447323;

                    case (23):
                        return 44152005855084346;

                    case (24):
                        return 445958869294805289;

                    case (25):
                        return 4638590332229999353;

                    default:
                        throw new OverflowException();
                }
            }
        }

        /// <summary>
        /// Returns a list of variables as defined by the current state.
        /// </summary>
        public List<String> VarNames
        {
            get
            {
                List<string> output = new List<string>();

                foreach (var i in _state)
                {
                    output.Add(IntToVarName(i));
                }

                return output;
            }
        }

        #endregion

        #region Constructors

        protected VarState(XElement xe)
            : base(xe)
        {
            XElement xel = xe.Elements("NumVariables").FirstOrDefault();
            _numVariables = int.Parse(xel.Value);

            _state = new List<int>();

            xel = xe.Elements("State").FirstOrDefault();
            foreach(var x in xel.Descendants())
            {
                _state.Add(int.Parse(x.Value));
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="VarState"/>.
        /// </summary>
        /// <param name="num">Number of variables.</param>
        public VarState(int num)
        {
            _numVariables = num;
            Reset();
        }

        public static VarState FromXElement(XElement xe)
        {
            return new VarState(xe);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Advances the enumerator to the next possible variable combination.
        /// </summary>
        /// <returns>True if the state changed, false if there is not a next item.</returns>
        public override bool MoveNext()
        {
            if (DoneIterating)
            {
                return false;
            }

            int max = 1;
            int cursor = _numVariables - 1;

            // start at the right
            while (cursor >= 0)
            {
                // Find larget value current index can attain.
                max = FindMaxLeft(cursor);

                if (cursor == _numVariables - 1)
                {
                    _uniqueVariablesCount = max - 1;
                }

                // Can increment current index?
                if (_state[cursor] + 1 <= max)
                {
                    _state[cursor]++;
                    IterationCount++;
                    return true;
                }

                // Can't increment. There's going to be a "wrap around"
                _state[cursor] = 1;
                cursor--;
            }

            // as a side effect, the state will be set to 1,1,1,1... so fix that
            cursor = _numVariables - 1;
            int c = _numVariables;

            while (cursor >= 0)
            {
                _state[cursor--] = c--;
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
            DoneIterating = _numVariables < 2;
            _state = new List<int>();

            for (int i = 0; i < _numVariables; i++)
            {
                _state.Add(1);
            }
        }

        /// <summary>
        /// Gets the current state.
        /// </summary>
        /// <returns>String containing variables state.</returns>
        public override string ToString()
        {
            string s = String.Empty;
            s += "{";
            int i;

            for (i = 0; i < _numVariables - 1; i++)
            {
                s += IntToVarName(_state[i]) + ", ";
            }

            s += IntToVarName(_state[i]);
            s += "}";
            return s;
        }

        /// <summary>
        /// Helper function to convert an int into a string. 1 becomes "a", 2 becomes "b", etc.
        /// </summary>
        /// <param name="num">Number to convert.</param>
        /// <returns>String containing variable name.</returns>
        private string IntToVarName(int num)
        {
            string s = String.Empty;
            int t = num;
            int rem;
            List<String> outList = new List<string>();

            while (t > 0)
            {
                rem = t % 26;

                if (rem == 0)
                {
                    rem = 26;
                }

                outList.Add(((char)('a' + rem - 1)).ToString());
                t -= rem;
                t /= 26;
            }

            outList.Reverse();
            s = String.Join("", outList);
            return s;
        }

        /// <summary>
        /// Helper function which finds the maximum value the position as the start index can take.
        /// </summary>
        /// <param name="startIndex">Position to find max potential value for.</param>
        /// <returns>Max value position can take.</returns>
        private int FindMaxLeft(int startIndex)
        {
            int max = 0;
            int cursor;

            for (cursor = startIndex - 1; cursor >= 0; cursor--)
            {
                max = _state[cursor] > max ? _state[cursor] : max;
            }

            max++;
            return max;
        }

        public new XElement ToXElement()
        {
            XElement root = new XElement("VarState");

            root.Add(new XElement("NumVariables", _numVariables));

            XElement state = new XElement("State");

            foreach(var i in _state)
            {
                state.Add(new XElement("int", i));
            }

            root.Add(state);

            root.Add(base.ToXElement());

            return root;
        }

        #endregion
    }
}

