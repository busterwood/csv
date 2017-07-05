using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Csv
{
    class Exists
    {
        public static void Run(List<string> args)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var observer = Args.VerboseJoinObserver(args);

                Relation input = Args.CsvRelation(args);

                var others = args
                    .Select(file => new { file, reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read)) })
                    .Select(r => r.reader.ToCsvDataSequence(r.file))
                    .ToList();

                var result = others.Aggregate(input, (left, right) => left.SemiJoin(right, observer));

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

        static void Help()
        {
            Console.Error.WriteLine($"csv exists [--in file] [file ...]");
            Console.Error.WriteLine($"Outputs the input CSV where only if a row exists in the additional input files with matching values in common columns.");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
