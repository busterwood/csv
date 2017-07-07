using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Csv
{
    class Project
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");
                HashSet<string> keep = ColumnsToKeep(args, input.Schema.Select(c => c.Name));
                Args.CheckColumnsAreValid(args, input.Schema);

                return all ? input.ProjectAll(keep) : input.Project(keep); 
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
                return null;
            }
        }

        static HashSet<string> ColumnsToKeep(List<string> args, IEnumerable<string> schemaCols)
        {
            if (args.Remove("--away"))
                // project away columns, i.e. original schema without the columns listed in args
                return new HashSet<string>(schemaCols.Except(args, Data.Column.NameEquality), Data.Column.NameEquality);
            
            // args contains the columns to keep
            return new HashSet<string>(args, StringComparer.OrdinalIgnoreCase);
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv project [--all] [--in file] [--away] Column [Column ...]");
            Console.Error.WriteLine($"Outputs in the input CSV with only the specified columns");
            Console.Error.WriteLine($"\t--all   do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in    read the input from a file path (rather than standard input)");
            Console.Error.WriteLine($"\t--away  removes the input columns from the source");
            Programs.Exit(1);
        }
    }

}
