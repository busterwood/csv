using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Csv
{
    class OrderBy
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");
                Args.CheckColumnsAreValid(args, input.Schema);

                IOrderedEnumerable<Row> sortedRows = SortRows(args, input.Distinct(!all));
                return new DerivedRelation(input.Schema, sortedRows);
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
                return null;
            }
        }

        static IOrderedEnumerable<Row> SortRows(List<string> args, IEnumerable<Row> csv)
        {
            var orderedRows = csv.OrderBy(row => row.Get(args[0]));
            var rest = args.Skip(1);
            return rest.Aggregate(orderedRows, (rows, arg) => rows.ThenBy(r => r.Get(arg)));
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv orderby [--all] [--in file] Column [Column ...]");
            Console.Error.WriteLine($"Sorts the input CSV by one or more columns");
            Console.Error.WriteLine($"\t--all  do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in   read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
