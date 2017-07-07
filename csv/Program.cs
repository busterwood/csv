using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Csv
{
    class Program
    {
        static readonly string[] commands = {
            "diff",
            "difference",
            "exists",
            "semijoin",
            "intersect",
            "join",
            "orderby",
            "pretty",
            "project",
            "select",
            "rename",
            "restrict",
            "tojson",
            "toxml",
            "where",
            "union",
        };

        static void Main(string[] argv)
        {
            try
            {
                var args = argv.ToList();
                if (args.Count == 0)
                    Help();
                var input = Args.CsvRelation(args);
                var sections = args.SplitOn(str => commands.Contains(str.ToLower()));
                Relation result =  sections.Aggregate(input, (rel, cmd) => Run(cmd, rel));

                if (result != null)
                {
                    Console.WriteLine(result.Schema.ToCsv());
                    foreach (var row in result)
                        Console.WriteLine(row.ToCsv());
                }

                Programs.Exit(0);
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
        }

        private static Relation Run(IEnumerable<string> cmd, Relation input)
        {
            var args = cmd.Skip(1).ToList();
            string c = cmd.First().ToLower();
            if (input == null)
            {
                StdErr.Warning($"Input of command '{c}' is missing. Was the previous command 'pretty'?");
                Help();
            }
            switch (c)
            {
                case "diff":
                case "difference":
                    return Difference.Run(args, input);
                case "exists":
                case "semijoin":
                    return Exists.Run(args, input);
                case "intersect":
                    return Intersect.Run(args, input);
                case "join":
                    return NaturalJoin.Run(args, input);
                case "orderby":
                    return OrderBy.Run(args, input);
                case "pretty":
                    return PrettyPrint.Run(args, input);
                case "project":
                case "select":
                    return Project.Run(args, input);
                case "rename":
                    return Rename.Run(args, input);
                case "restrict":
                case "where":
                    return Restrict.Run(args, input);
                case "tojson":
                    return ToJson.Run(args, input);
                case "toxml":
                    return ToXml.Run(args, input);
                case "union":
                    return Union.Run(args, input);
                default:
                    throw new Exception("Unknown command");
            }
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv [--help] [--in file] command [args...] [command args...]");
            Console.Error.WriteLine($"Reads CSV from StdIn or the --in file and outputs CSV.");
            Console.Error.WriteLine($"Command must be one of the following:");
            Console.Error.WriteLine($"\tdiff[erence]    set difference between the input and other file(s)");
            Console.Error.WriteLine($"\tsemijoin|exists rows from input where a matching row exists in other file(s)");
            Console.Error.WriteLine($"\tintersect       set intersection between the input and other file(s)");
            Console.Error.WriteLine($"\tjoin            natural join of the input and other file(s)");
            Console.Error.WriteLine($"\tproject|select  removes columns from the input");
            Console.Error.WriteLine($"\trename          changes some of the input column names");
            Console.Error.WriteLine($"\trestrict|where  removes rows from the input");
            Console.Error.WriteLine($"\tunion           set union between the input and other file(s)");
            Console.Error.WriteLine($"Non-relational commands are:");
            Console.Error.WriteLine($"\torderby         sorts the input by one or more columns");
            Console.Error.WriteLine($"\tpretty          formats the input CSV in aligned columns");
            Console.Error.WriteLine($"\ttojson          outputs JSON array for the input CSV, one object per row");
            Console.Error.WriteLine($"\ttoxml           outputs XML for the input CSV, one element per row");
            Programs.Exit(1);
        }

    }
}
