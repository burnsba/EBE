using System;
using Mono.Options;
using EBE.Core.Evaluation;

namespace EBE.Parse
{
    public class MainClass
    {
        private const string versionString = "0.0.1";

        public static void Main (string[] args)
        {
            bool show_help = false;
            bool showVersion = false;
            int maxBits = 1;

            string rawExpression = String.Empty;

            var p = new OptionSet () {
                { "b|bits=", "Max number of bits to use when evaluating expression (default=1)",
                    (int v) => maxBits = v },
                { "v|version", "Print version information",
                    v => showVersion = true },
                { "h|help",  "show this message and exit", 
                    v => show_help = v != null },
            };

            try
            {
                var r = p.Parse (args);

                r.ForEach(x => rawExpression += x);
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

            if(show_help)
            {
                ShowHelp (p);
                return;
            }

            if(maxBits < 1)
            {
                Console.WriteLine("Max bit must be greater or equal to 1.");
                return;
            }

            Expression expression = new Expression(maxBits);

            expression.Parse(rawExpression);

            Evaluator e = new Evaluator(expression, expression.VariableKeys.Count, maxBits);

            Console.WriteLine("Expression: " + rawExpression);

            var result = e.Eval();

            Console.WriteLine(
                result == null
                ? "null"
                : result.Value.ToString()
            );
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

