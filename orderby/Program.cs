using BusterWood.Data;
using BusterWood.Data.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.orderby
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
                Args.CheckColumnsAreValid(args, csv.Schema);

                Console.WriteLine(csv.Schema.Join());
                IOrderedEnumerable<Row> orderedRows = SortRows(args, csv);
                var lines = orderedRows.Select(r => r.ToString()).Where(l => keepLine(l));
                foreach (var line in lines)
                    Console.WriteLine(line);
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Environment.Exit(1);
            }
        }

        static IOrderedEnumerable<Row> SortRows(List<string> args, DataSequence csv)
        {
            var orderedRows = csv.OrderBy(row => row.Get(args[0]));
            foreach (var a in args.Skip(1))
                orderedRows = orderedRows.ThenBy(r => r.Get(a));
            return orderedRows;
        }
    }
}
