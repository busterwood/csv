using BusterWood.Data;
using BusterWood.Data.Shared;
using System;
using System.Collections.Generic;

namespace BusterWood.Csv
{
    class Rename
    {
        public static void Run(List<string> args)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");

                DataSequence input = Args.GetDataSequence(args);

                if (args.Count % 2 != 0)
                    throw new Exception("You must supply at pairs of paremters: column value [column value...]");

                var changes = Changes(args);

                var result = all ? input.RenameAll(changes) : input.Rename(changes);

                Console.WriteLine(result.Schema.ToCsv());
                foreach (var row in result)
                    Console.WriteLine(row.ToCsv());
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
        }

        private static Dictionary<string, string> Changes(List<string> args)
        {
            var changes = new Dictionary<string, string>(Column.NameEquality);
            for (int i = 0; i < args.Count; i += 2)
                changes.Add(args[i], args[i + 1]);
            return changes;
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv rename [--in file] [old new...]");
            Console.Error.WriteLine($"Outputs the input CSV chaning the name of one or more columns.");
            Console.Error.WriteLine($"\t--all    do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
