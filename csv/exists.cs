using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Csv
{
    class Exists
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var observer = Args.VerboseJoinObserver(args);
                var others = args
                    .Select(file => new { file, reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read)) })
                    .Select(r => r.reader.CsvToRelation(r.file))
                    .ToList();

                return others.Aggregate(input, (left, right) => left.SemiJoin(right, observer));
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
                return null;
            }
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv exists [--in file] [file ...]");
            Console.Error.WriteLine($"Outputs the input CSV where only if a row exists in the additional input files with matching values in common columns.");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
