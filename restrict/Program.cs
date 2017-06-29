using System;
using System.Linq;
using System.Collections.Generic;
using BusterWood.Data;
using BusterWood.Data.Shared;

namespace BusterWood.restrict
{
    class Program
    {
        static void Main(string[] argv)
        {
            try
            {
                var args = argv.ToList();
                var restrictAway = args.Remove("--away"); // not equals flag, invert match
                var keepLine = Args.KeepOrRemoveDupLines(args);
                DataSequence csv = Args.GetDataSequence(args);

                if (args.Count == 0 || args.Count % 2 != 0)
                    throw new Exception("You must supply at pairs of paremters: column value [column value...]");

                Args.CheckColumnsAreValid(args.Where((a, i) => i % 2 == 0), csv.Schema);

                Console.WriteLine(csv.Schema.Join());

                var argPairs = ArgsToPairs(args).ToList();
                var contains = ContainsFunctions(argPairs).ToList();

                foreach (var row in csv)
                {
                    if (contains.Any(f => f(row)) != restrictAway) // allow invertion of match
                    {
                        var line = row.ToString();
                        if (keepLine(line))
                            Console.WriteLine(row);
                    }
                }
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Environment.Exit(1);
            }
        }

        static IEnumerable<Func<Row, bool>> ContainsFunctions(IEnumerable<Tuple<string, string>> args)
        {
            // turn tuples of column/value into a test: does the row contain the value in the column?
            return args.Select(p => new Func<Row, bool>(row => row.Get(p.Item1).ToString().IndexOf(p.Item2, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        static IEnumerable<Tuple<string, string>> ArgsToPairs(IReadOnlyList<string> args)
        {
            for (int i = 0; i < args.Count; i += 2)
                yield return new Tuple<string, string>(args[i], args[i + 1]);
        }
        
    }
}
