using BusterWood.Data;
using BusterWood.Data.Shared;
using System;
using System.IO;
using System.Linq;

namespace BusterWood.difference
{
    class Program
    {
        static void Main(string[] argv)
        {
            try
            {
                var args = argv.ToList();
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");

                DataSequence input = Args.GetDataSequence(args);

                var others = args
                    .Select(file => new { file, reader = new StreamReader(file) })
                    .Select(r => r.reader.ToCsvDataSequence(r.file))
                    .ToList();

                input.CheckSchemaCompatibility(others);

                var unionOp = all ? (Func<DataSequence, DataSequence, DataSequence>)Data.Extensions.DifferenceAll : Data.Extensions.Difference;
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
            Programs.Exit(1);
        }

        static void Help()
        {
            Console.Error.WriteLine($"{Programs.Name} [--all] [--in file] [file ...]");
            Console.Error.WriteLine($"Outputs the rows in the input that do not appear in any of the additional files");
            Console.Error.WriteLine($"\t--all    do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
