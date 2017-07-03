using System;
using System.Linq;
using System.Collections.Generic;
using BusterWood.Data;
using BusterWood.Data.Shared;
using System.Text;

namespace BusterWood.Csv
{
    class PrettyPrint
    {
        public static void Run(List<string> args)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");
                var invert = args.Remove("--away"); // not equals flag, invert match
                var contains = args.Remove("--contains");
                DataList csv = Args.GetDataSequence(args).ToList();

                // work out maximum width of each column
                var maxColWidths = csv.Schema.ToDictionary(
                    col => col.Name, 
                    col => csv.Aggregate(col.Name.Length, (max, row) => Math.Max(max, row.Get(col.Name).ToString().Length)), 
                    Column.NameEquality
                );
                StringBuilder sb = new StringBuilder(maxColWidths.Values.Select(n => n+1).Sum() + 1);
                
                Console.WriteLine(Format(sb, csv.Schema, maxColWidths));

                foreach (var row in csv.Distinct(!all))
                    Console.WriteLine(Format(sb, row, maxColWidths));
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
        }

        private static string Format(StringBuilder sb, Schema schema, Dictionary<string, int> colWidths)
        {
            sb.Clear();
            foreach (var col in schema)
            {
                sb.Append('|').AppendFormat("{0,-" + colWidths[col.Name] + "}", col.ToString());
            }
            sb.Append('|');
            return sb.ToString();
        }

        private static string Format(StringBuilder sb, Row row, Dictionary<string, int> colWidths)
        {
            sb.Clear();
            foreach (var cv in row)
            {
                sb.Append('|').AppendFormat("{0,-" + colWidths[cv.Name] + "}", cv.Value.ToString());
            }
            sb.Append('|');
            return sb.ToString();
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv pretty [--all] [--in file]");
            Console.Error.WriteLine($"Outputs rows of the input CSV printed in aligned columns separated with pipe characters");
            Console.Error.WriteLine($"\t--all       do NOT remove duplicates from the result");
            Programs.Exit(1);
        }
    }
}
