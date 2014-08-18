using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
    [DataContract]
    public class VarState : IteratorBase
	{
		#region Fields

		/// <summary>
		/// Number of variables to build expressions for.
		/// </summary>
        [DataMember(Name="NumVariables", Order = 1)]
		private readonly int _numVariables;

		/// <summary>
		/// Variable state.
		/// </summary>
        [DataMember(Name="State", Order = 4)]
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
					case (1): return 1;
					case (2): return 2;
					case (3): return 5;
					case (4): return 15;
					case (5): return 52;
					case (6): return 203;
					case (7): return 877;
					case (8): return 4140;
					case (9): return 21147;
					case (10): return 115975;
					case (11): return 678570;
					case (12): return 4213597;
					case (13): return 27644437;
					case (14): return 190899322;
					case (15): return 1382958545;
					case (16): return 10480142147;
					case (17): return 82864869804;
					case (18): return 682076806159;
					case (19): return 5832742205057;
					case (20): return 51724158235372;
					case (21): return 474869816156751;
					case (22): return 4506715738447323;
					case (23): return 44152005855084346;
					case (24): return 445958869294805289;
					case (25): return 4638590332229999353;
					#region bell numbers 26 - 100
					/*
                    26 49631246523618756274
                    27 545717047936059989389
                    28 6160539404599934652455
                    29 71339801938860275191172
                    30 846749014511809332450147
                    31 10293358946226376485095653
                    32 128064670049908713818925644
                    33 1629595892846007606764728147
                    34 21195039388640360462388656799
                    35 281600203019560266563340426570
                    36 3819714729894818339975525681317
                    37 52868366208550447901945575624941
                    38 746289892095625330523099540639146
                    39 10738823330774692832768857986425209
                    40 157450588391204931289324344702531067
                    41 2351152507740617628200694077243788988
                    42 35742549198872617291353508656626642567
                    43 552950118797165484321714693280737767385
                    44 8701963427387055089023600531855797148876
                    45 139258505266263669602347053993654079693415
                    46 2265418219334494002928484444705392276158355
                    47 37450059502461511196505342096431510120174682
                    48 628919796303118415420210454071849537746015761
                    49 10726137154573358400342215518590002633917247281
                    50 185724268771078270438257767181908917499221852770
                    51 3263983870004111524856951830191582524419255819477
                    52 58205338024195872785464627063218599149503972126463
                    53 1052928518014714166107781298021583534928402714242132
                    54 19317287589145618265728950069285503257349832850302011
                    55 359334085968622831041960188598043661065388726959079837
                    56 6775685320645824322581483068371419745979053216268760300
                    57 129482661947506964462616580633806000917491602609372517195
                    58 2507136358984296114560786627437574942253015623445622326263
                    59 49176743336309621659000944152624896853591018248919168867818
                    60 976939307467007552986994066961675455550246347757474482558637
                    61 19652364471547941482114228389322789963345673460673370562378245
                    62 400237304821454786230522819234667544935526963060240082269259738
                    63 8250771700405624889912456724304738028450190134337110943817172961
                    64 172134143357358850934369963665272571125557575184049758045339873395
                    65 3633778785457899322415257682767737441410036994560435982365219287372
                    66 77605907238843669482155930857960017792778059887519278038000759795263
                    67 1676501284301523453367212880854005182365748317589888660477021013719409
                    68 36628224206696135478834640618028539032699174847931909480671725803995436
                    69 809212768387947836336846277707066239391942323998649273771736744420003007
                    70 18075003898340511237556784424498369141305841234468097908227993035088029195
                    71 408130093410464274259945600962134706689859323636922532443365594726056131962
                    72 9314528182092653288251451483527341806516792394674496725578935706029134658745
                    73 214834623568478894452765605511928333367140719361291003997161390043701285425833
                    74 5006908024247925379707076470957722220463116781409659160159536981161298714301202
                    75 117896026920858300966730642538212084059025603061199813571998059942386637656568797
                    76 2804379077740744643020190973126488180455295657360401565474468309847623573788115607
                    77 67379449595254843852699636792665969652321946648374400833740986348378276368807261348
                    78 1635000770532737216633829256032779450518375544542935181844299348876855151241590189395
                    79 40064166844084356404509204005730815621427040237270563024820379702392240194729249115029
                    80 991267988808424794443839434655920239360814764000951599022939879419136287216681744888844
                    81 24761288718465863816962119279306788401954401906692653427329808967315171931611751006838915
                    82 624387454429479848302014120414448006907125370284776661891529899343806658375826740689137423
                    83 15892292813296951899433594303207669496517041849871581501737510069308817348770226226653966474
                    84 408248141291805738980141314733701533991578374164094348787738475995651988600158415299211778933
                    85 10583321873228234424552137744344434100391955309436425797852108559510434249855735357360593574749
                    86 276844443054160876160126038812506987515878490163433019207947986484590126191194780416973565092618
                    87 7306720755827530589639480511232846731775215754200303890190355852772713202556415109429779445622537
                    88 194553897403965647871786295024290690576513032341195649821051001205884166153194143340809062985041067
                    89 5225728505358477773256348249698509144957920836936865715700797250722975706153317517427783066539250012
                    90 141580318123392930464192819123202606981284563291786545804370223525364095085412667328027643050802912567
                    91 3868731362280702160655673912482765098905555785458740412264329844745080937342264610781770223818259614025
                    92 106611797892739782364113678801520610524431974731789913132104301942153476208366519192812848588253648356364
                    93 2962614388531218251190227244935749736828675583113926711461226180042633884248639975904464409686755210349399
                    94 83012043550967281787120476720274991081436431402381752242504514629481800064636673934392827445150961387102019
                    95 2345129936856330144543337656630809098301482271000632150222900693128839447045930834163493232282141300734566042
                    96 66790853422797408533421892496106177820862555650400879850993569405575404871887998514898872210341414631481213729
                    97 1917593350464112616752757157565032460248311804906650215954187246738986739924580790084847891233423398173059771233
                    98 55494677927746340698788238667452126040563242441827634980157203368430358083090722409217101274455481270374885095618
                    99 1618706027446068305855680628161135741330684513088812399898409470089128730792407044351108134019449028191480663320741
                    100 47585391276764833658790768841387207826363669686825611466616334637559114497892442622672724044217756306953557882560751
                     */
					#endregion
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

		/// <summary>
		/// Initializes a new instance of <see cref="VarState"/>.
		/// </summary>
		/// <param name="num">Number of variables.</param>
		public VarState(int num)
		{
			_numVariables = num;

			Reset();
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

                if(cursor == _numVariables - 1)
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

			for (int i=0; i<_numVariables; i++)
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

		#endregion
	}
}

