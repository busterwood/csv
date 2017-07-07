using System;
using System.Linq;
using System.Collections.Generic;
using BusterWood.Data;

namespace BusterWood.Csv
{
    class Restrict
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");
                var invert = args.Remove("--away"); // not equals flag, invert match
                var contains = args.Remove("--contains");

                if (args.Count % 2 != 0)
                    throw new Exception("You must supply at pairs of paremters: column value [column value...]");

                Args.CheckColumnsAreValid(args.Where((a, i) => i % 2 == 0), input.Schema);

                Func<Row, bool> predicate = contains ? ContainsPredicate(args) : EqualPredicate(args);
                if (all)
                    return invert ? input.RestrictAwayAll(predicate) : input.RestrictAll(predicate);
                else
                    return invert ? input.RestrictAway(predicate) : input.Restrict(predicate);
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
                return null;
            }
        }

        private static Func<Row, bool> EqualPredicate(List<string> args)
        {
            var nameValues = NameValueSequence(args).ToList();
            var rowPredicates = EqualFunctions(nameValues).ToList();
            return row => rowPredicates.Any(p => p(row));
        }

        private static Func<Row, bool> ContainsPredicate(List<string> args)
        {
            var nameValues = NameValueSequence(args).ToList();
            var rowPredicates = ContainsFunctions(nameValues).ToList();
            return row => rowPredicates.Any(p => p(row));
        }

        static IEnumerable<NameValue> NameValueSequence(IReadOnlyList<string> args)
        {
            for (int i = 0; i < args.Count; i += 2)
                yield return new NameValue(args[i], args[i + 1]);
        }

        static IEnumerable<Func<Row, bool>> EqualFunctions(IEnumerable<NameValue> args)
        {
            // turn tuples of column/value into a test: does the row contain the value in the column?
            return args.Select(cond => new Func<Row, bool>(row => row.Get(cond.ColumnName).ToString().Equals(cond.Value, StringComparison.OrdinalIgnoreCase)));
        }

        static IEnumerable<Func<Row, bool>> ContainsFunctions(IEnumerable<NameValue> args)
        {
            // turn tuples of column/value into a test: does the row contain the value in the column?
            return args.Select(cond => new Func<Row, bool>(row => row.Get(cond.ColumnName).ToString().IndexOf(cond.Value, StringComparison.OrdinalIgnoreCase) >= 0));
        }
        
        struct NameValue
        {
            public readonly string ColumnName;
            public readonly string Value;

            public NameValue(string c, string v)
            {
                ColumnName = c;
                Value = v;
            }
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv restrict [--all] [--in file] [--away] [--equal] Column Value [Column Value ...]");
            Console.Error.WriteLine($"Outputs rows of the input CSV where Column equals the string Value.  Multiple tests are supported.");
            Console.Error.WriteLine($"\t--all       do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in        read the input from a file path (rather than standard input)");
            Console.Error.WriteLine($"\t--away      removes rows from the input that match the test(s)");
            Console.Error.WriteLine($"\t--contains  changes the test to be Column contains Value, rather that equality");
            Programs.Exit(1);
        }
    }
}
