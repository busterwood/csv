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
                var keepLine = Args.KeepOrRemoveDupLines(args);
                DataSequence csv = Args.GetDataSequence(args);
                HashSet<string> keep = ColumnsToKeep(args, csv.Schema.Select(c => c.Name));
                Args.CheckColumnsAreValid(args, csv.Schema);

                Console.WriteLine(csv.Schema.Where(c => keep.Contains(c.Name)).Select(c => c.Name).Join());
                var lines = csv.Select(row => row.Where(v => keep.Contains(v.Name)).Select(v => v.Value).Join()).Where(l => keepLine(l));
                foreach (var line in lines)
                    Console.WriteLine(line);
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Environment.Exit(1);
            }
        }

        static HashSet<string> ColumnsToKeep(List<string> args, IEnumerable<string> schemaCols)
        {
            if (args.Remove("--away"))
            {
                // project away columns, i.e. original schema without the columns listed in args
                return new HashSet<string>(schemaCols.Except(args), StringComparer.OrdinalIgnoreCase);
            }
            
            // args contains the columns to keep
            return new HashSet<string>(args, StringComparer.OrdinalIgnoreCase);
        }

    }

}
