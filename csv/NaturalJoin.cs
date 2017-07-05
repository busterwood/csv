using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Csv
{
    class NaturalJoin
    {
        public static void Run(List<string> args)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var observer = Args.VerboseJoinObserver(args);

                DataSequence input = Args.GetDataSequence(args);

                var others = args
                    .Select(file => new { file, reader = new StreamReader(file) })
                    .Select(r => r.reader.ToCsvDataSequence(r.file))
                    .ToList();

                var result = others.Aggregate(input, (left, right) => left.NaturalJoin(right, observer));

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
            Console.Error.WriteLine($"csv join [--in file] [file ...]");
            Console.Error.WriteLine($"Outputs the natural join of the input CSV and some additional files based on common columns");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
