using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Unix;
using System.Runtime.Serialization;
using System.Xml;
using System.Text.RegularExpressions;
using Mono.Options;
using EBE.Data;
using System.Reflection;
using EBE.Core.Utilities;

namespace EBE
{
	class MainClass
	{
        private const string versionString = "0.0.2";

		// Catch SIGINT
		private static UnixSignal[] signals = new UnixSignal [] {
			new UnixSignal (Mono.Unix.Native.Signum.SIGINT)
		};

		public static void Main (string[] args)
		{
            Application.LoadLibraries();

            bool show_help = false;
            bool continue_processing = false;
            bool pauseAfterEach = false;
            bool showVersion = false;
            int numVariables = 2;
            int maxBits = 1;

            var p = new OptionSet () {
                { "c|continue", "Load latest context file and continue processing",
                    v => continue_processing = true },
                { "n|num=", "Number of variables to generate expressions for",
                    (int v) => numVariables = v },
                { "p|pause", "Pause after generating each expression, waiting for input",
                    v => pauseAfterEach = true },
                { "b|bits=", "Max number of bits to use when evaluating expression (default=1)",
                    (int v) => maxBits = v },
                { "v|version", "Print version information",
                    v => showVersion = true },
                { "h|help",  "show this message and exit", 
                    v => show_help = v != null },
            };

            try
            {
                p.Parse (args);
            }
            catch
            {
                Console.WriteLine ("Try `gen --help' for more information.");
                return;
            }

            if(showVersion)
            {
                ShowVersion();
                return;
            }

            if(show_help || (numVariables < 1 && continue_processing == false))
            {
                ShowHelp (p);
                return;
            }

            if(maxBits < 1)
            {
                Console.WriteLine("Max bit must be greater or equal to 1.");
                return;
            }

			Generator g = null;

			Thread signal_thread = new Thread (delegate () 
			{
				while (true)
				{
					// Wait for a signal to be delivered
					UnixSignal.WaitAny (signals, -1);

					if (g != null)
					{
						g.ReceivedInterrupt = true;
					}

					break;
				}
			});

			signal_thread.Start();

            bool loadFromFile = false;
            string fileName = String.Empty;

            if(continue_processing)
            {
                // look for files matching the save-as filename

                string currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

                currentPath = currentPath.Replace("file:", "");

                var files = Directory.GetFiles(currentPath);
                List<string> contextFiles = new List<string>();

                foreach(var s in files)
                {
                    Match match = Regex.Match(s, @"t[0-9]{14}\.context",
                                          RegexOptions.IgnoreCase);
                    if(match.Success)
                    {
                        contextFiles.Add(s);
                    }
                }

                contextFiles.Sort();

                if(contextFiles.Count > 0)
                {
                    fileName = contextFiles.Last();
                    loadFromFile = true;
                }
            }

			var a = Stopwatch.StartNew();

			using (StreamWriter sw = new StreamWriter(Console.OpenStandardOutput()))
			{
                if(loadFromFile)
                {
                    try
                    {
                        DataContractSerializer dcs = new DataContractSerializer(typeof(Generator));
                        FileStream fs = new FileStream(fileName, FileMode.Open);

                        using(XmlDictionaryReader reader =
                        XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                        {
                            g = (Generator)dcs.ReadObject(reader);
                        }

                        g.ResumeFromSave = true;
                        g.OutputStream = sw;
                    }
                    catch
                    {
                        g = new Generator(numVariables, sw);

                        g.MaxBits = maxBits;
                    }
                }
                else
                {
                    g = new Generator(numVariables, sw);

                    g.MaxBits = maxBits;
                }

                if(pauseAfterEach)
                {
                    g.PauseAfterEach = pauseAfterEach;
                }

                // alright with all the setup, go do stuff

				g.DoWork();
			}

			signal_thread.Abort();

			a.Stop();

			Console.WriteLine("Run time: " + 
                              a.Elapsed.Hours + " hr " + 
                              a.Elapsed.Minutes + " min " + 
                              a.Elapsed.Seconds + " sec");
			Console.WriteLine("total count: " + g.IterationCount);
            Console.WriteLine("Skipped {0} existing records, added {1} new ones.", g.DbSkip, g.DbNew);
		}

        static void ShowHelp (OptionSet p)
        {
            Console.WriteLine ("Usage: gen [OPTIONS]+");
            Console.WriteLine ();
            Console.WriteLine ("Options:");
            p.WriteOptionDescriptions (Console.Out);
        }

        static void ShowVersion()
        {
            Console.WriteLine("Version " + versionString);
        }
	}
}
