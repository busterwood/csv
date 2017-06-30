using BusterWood.Data;
using BusterWood.Data.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BusterWood.orderby
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
                DataSequence csv = Args.GetDataSequence(args);
                Args.CheckColumnsAreValid(args, csv.Schema);

                Console.WriteLine(csv.Schema.ToCsv());
                IOrderedEnumerable<Row> result = SortRows(args, csv.Distinct(!all));
                foreach (var row in result)
                    Console.WriteLine(row.ToCsv());
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
            Programs.Exit(0);
        }

        static IOrderedEnumerable<Row> SortRows(List<string> args, IEnumerable<Row> csv)
        {
            var orderedRows = csv.OrderBy(row => row.Get(args[0]));
            var rest = args.Skip(1);
            return rest.Aggregate(orderedRows, (rows, arg) => rows.ThenBy(r => r.Get(arg)));
        }

        static void Help()
        {
            Console.Error.WriteLine($"{Programs.Name} [--all] [--in file] Column [Column ...]");
            Console.Error.WriteLine($"Sorts the input CSV by one or more columns");
            Console.Error.WriteLine($"\t--all  do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in   read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
