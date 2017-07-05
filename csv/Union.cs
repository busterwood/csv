using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Csv
{
    class Union
    {
        public static void Run(List<string> args)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");

                Relation input = Args.CsvRelation(args);

                var others = args
                    .Select(file => new { file, reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read)) })
                    .Select(r => r.reader.CsvToRelation(r.file))
                    .ToList();

                input.CheckSchemaCompatibility(others);

                var unionOp = all ? (Func<Relation, Relation, Relation>)Data.Extensions.UnionAll : Data.Extensions.Union;
                var result = others.Aggregate(input, (acc, o) => unionOp(acc, o));

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
            Console.Error.WriteLine($"csv union [--all] [--in file] [file ...]");
            Console.Error.WriteLine($"Outputs the set union of the input CSV and some additional files");
            Console.Error.WriteLine($"\t--all    do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
