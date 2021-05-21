using System;
using System.Linq;
using System.Collections.Generic;
using BusterWood.Data;
using System.Text;

namespace BusterWood.Csv
{
    class ToXml
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");

                string format = BuildLineFormat(input.Schema);
                Console.WriteLine($@"<relation name=""{input.Schema.Name}"">");

                var vals = new object[input.Schema.Count];
                foreach (var row in input.Distinct(!all))
                {
                    int i = 0;
                    foreach (var nv in row)
                        vals[i++] = nv.Value;
                    Console.WriteLine(format, vals);
                }

                Console.WriteLine("</relation>");
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
            sb.Append(@"  <row ");
            int i = 0;
            foreach (var col in schema)
            {
                sb.Append(col.Name).Append(@"=""");
                if (Equals(col.Type, typeof(DateTime)) || Equals(col.Type, typeof(DateTimeOffset)))
                    sb.Append("{").Append(i).Append(@":u}");
                else
                    sb.Append("{").Append(i).Append(@"}");
                sb.Append(@""" ");
                i++;
            }
            sb.Length -= 1; // trailing space
            sb.Append("/>");
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
