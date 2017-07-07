using System;
using System.Linq;
using System.Collections.Generic;
using BusterWood.Data;
using System.Text;

namespace BusterWood.Csv
{
    class ToJson
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");

                string format = BuildLineFormat(input.Schema);
                Console.WriteLine("[");

                var vals = new object[1 + input.Schema.Count];
                vals[0] = " ";
                foreach (var row in input.Distinct(!all))
                {
                    int i = 1;
                    foreach (var nv in row)
                        vals[i++] = nv.Value;
                    Console.WriteLine(format, vals);
                    vals[0] = ",";
                }

                Console.WriteLine("]");
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
            return null; // should not be used as input
        }

        private static string BuildLineFormat(Schema schema)
        {
            var sb = new StringBuilder();
            sb.Append(@"  {0}{{");
            int i = 1;
            foreach (var col in schema)
            {
                sb.Append(@"""").Append(col.Name).Append(@""": ");
                if (col.Type == typeof(string))
                    sb.Append(@"""{").Append(i).Append(@"}"", ");
                else if (col.Type == typeof(DateTime) || col.Type == typeof(DateTimeOffset))
                    sb.Append(@"""{").Append(i).Append(@":u}"", ");
                else
                    sb.Append("{").Append(i).Append("}, ");
                i++;
            }
            sb.Length -= 2; // trailing comma and space
            sb.Append(" }}");
            return sb.ToString();
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv tojson [--all] [--in file]");
            Console.Error.WriteLine($"Outputs rows of the input CSV printed as a JSON array ");
            Console.Error.WriteLine($"\t--all       do NOT remove duplicates from the result");
            Programs.Exit(1);
        }
    }
}
