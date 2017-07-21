using System;
using System.Linq;
using System.Collections.Generic;
using BusterWood.Data;
using BusterWood.Json;

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

                input.Distinct(!all).WriteJson(Console.Out);
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
            return null; // should not be used as input
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
