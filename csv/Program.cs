﻿using BusterWood.Data;
using System;
using System.Linq;

namespace BusterWood.Csv
{
    class Program
    {
        static void Main(string[] argv)
        {
            try
            {
                var args = argv.ToList();
                if (args.Count == 0)
                    Help();
                var cmd = args[0];
                args.RemoveAt(0);
                switch (cmd.ToLower())
                {
                    case "diff":
                    case "difference":
                        Difference.Run(args);
                        break;
                    case "exists":
                    case "semijoin":
                        Exists.Run(args);
                        break;
                    case "intersect":
                        Intersect.Run(args);
                        break;
                    case "join":
                        NaturalJoin.Run(args);
                        break;
                    case "orderby":
                        OrderBy.Run(args);
                        break;
                    case "pretty":
                        PrettyPrint.Run(args);
                        break;
                    case "project":
                    case "select":
                        Project.Run(args);
                        break;
                    case "rename":
                        Rename.Run(args);
                        break;
                    case "restrict":
                    case "where":
                        Restrict.Run(args);
                        break;
                    case "union":
                        Union.Run(args);
                        break;
                    default:
                        throw new Exception("Unknown command");
                }
                Programs.Exit(0);
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv command [--help] [--in file] [args...]");
            Console.Error.WriteLine($"Reads CSV from StdIn or the --in file and outputs CSV.");
            Console.Error.WriteLine($"Command must be one of the following:");
            Console.Error.WriteLine($"\tdiff[erence]    set difference between the input and other file(s)");
            Console.Error.WriteLine($"\tsemijoin|exists rows from input where a matching row exists in other file(s)");
            Console.Error.WriteLine($"\tintersect       set intersection between the input and other file(s)");
            Console.Error.WriteLine($"\tjoin            natural join of the input and other file(s)");
            Console.Error.WriteLine($"\torderby         sorts the input by one or more columns");
            Console.Error.WriteLine($"\tpretty          formats the input CSV in aligned columns");
            Console.Error.WriteLine($"\tproject|select  removes columns from the input");
            Console.Error.WriteLine($"\trename          changes some of the input column names");
            Console.Error.WriteLine($"\trestrict|where  removes rows from the input");
            Console.Error.WriteLine($"\tunion           set union between the input and other file(s)");
            Programs.Exit(1);
        }

    }
}
