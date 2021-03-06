﻿using BusterWood.Data;
using System;
using System.Collections.Generic;

namespace BusterWood.Csv
{
    class Rename
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");

                if (args.Count % 2 != 0)
                    throw new Exception("You must supply at pairs of paremters: old new [old new...]");

                var changes = Changes(args);

                return all ? input.RenameAll(changes) : input.Rename(changes);
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
                return null;
            }
        }

        private static Dictionary<string, string> Changes(List<string> args)
        {
            var changes = new Dictionary<string, string>(Data.Column.NameEquality);
            for (int i = 0; i < args.Count; i += 2)
                changes.Add(args[i], args[i + 1]);
            return changes;
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv rename [--in file] [old new...]");
            Console.Error.WriteLine($"Outputs the input CSV chaning the name of one or more columns.");
            Console.Error.WriteLine($"\t--all    do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }
    }
}
