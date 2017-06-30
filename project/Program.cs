using BusterWood.Data;
using BusterWood.Data.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.project
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
                HashSet<string> keep = ColumnsToKeep(args, csv.Schema.Select(c => c.Name));
                Args.CheckColumnsAreValid(args, csv.Schema);

                var result = csv.Project(keep);
                Console.WriteLine(result.Schema.ToCsv());
                foreach (var row in result.Distinct(!all))
                    Console.WriteLine(row.ToCsv());
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
            Programs.Exit(0);
        }

        static HashSet<string> ColumnsToKeep(List<string> args, IEnumerable<string> schemaCols)
        {
            if (args.Remove("--away"))
                // project away columns, i.e. original schema without the columns listed in args
                return new HashSet<string>(schemaCols.Except(args, Column.NameEquality), Column.NameEquality);
            
            // args contains the columns to keep
            return new HashSet<string>(args, StringComparer.OrdinalIgnoreCase);
        }

        static void Help()
        {
            Console.Error.WriteLine($"{Programs.Name} [--all] [--in file] [--away] Column [Column ...]");
            Console.Error.WriteLine($"Outputs in the input CSV with only the specified columns");
            Console.Error.WriteLine($"\t--all   do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in    read the input from a file path (rather than standard input)");
            Console.Error.WriteLine($"\t--away  removes the input columns from the source");
            Programs.Exit(1);
        }
    }

}
