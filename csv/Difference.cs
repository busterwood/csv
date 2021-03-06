﻿using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Csv
{
    class Difference
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");
                var reverse = args.Remove("--rev");

                var others = args
                    .Select(file => new { file, reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read)) })
                    .Select(r => r.reader.CsvToRelation(r.file))
                    .ToList();

                input.CheckSchemaCompatibility(others);

                var unionOp = all ? (Func<Relation, Relation, Relation>)AllExtensions.DifferenceAll : RelationalExtensions.Difference;
                Relation result = reverse
                    ? others.Aggregate(input, (acc, o) => unionOp(o, acc)) // reverse diff
                    : others.Aggregate(input, (acc, o) => unionOp(acc, o));

                return result;
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
            Console.Error.WriteLine($"csv diff[erence] [--all] [--in file] [--rev] [file ...]");
            Console.Error.WriteLine($"Outputs the rows in the input CSV that do not appear in any of the additional files");
            Console.Error.WriteLine($"\t--all    do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Console.Error.WriteLine($"\t--rev    reverse the difference");
            Programs.Exit(1);
        }
    }
}
