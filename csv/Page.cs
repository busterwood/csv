using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Csv
{
    class Page
    {
        public static Relation Run(List<string> args, Relation input)
        {
            try
            {
                if (args.Remove("--help")) Help();

                int pageSize = args.IntFlag("--page-size") ?? args.IntFlag("--top") ?? 0;
                if (pageSize <= 0)
                    throw new HelpException("Please provide --page-size or --top");

                int page = args.IntFlag("--page") ?? 1;

                return new PageRelation(input, pageSize, page-1); // user will request page 1,2,etc but algo expect 0,1,2
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
                return null;
            }
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv page [--top|--page-size] [--page] [--in file]");
            Console.Error.WriteLine($"Outputs a page of date");
            Console.Error.WriteLine($"\t--page-size (--top)    number of rows to return");
            Console.Error.WriteLine($"\t--page    which page to return, starting with page 1");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Programs.Exit(1);
        }

        private class PageRelation : Relation
        {
            private readonly Relation input;
            private readonly int pageSize;
            private readonly int page;

            /// <param name="input"></param>
            /// <param name="pageSize">size of a page, must be one or more</param>
            /// <param name="page">ZERO based page index</param>
            public PageRelation(Relation input, int pageSize, int page) : base(input.Schema)
            {
                this.input = input;
                this.pageSize = pageSize;
                this.page = page;
            }

            protected override IEnumerable<Row> GetSequence()
            {
                return input.Skip(pageSize * page).Take(pageSize);
            }
        }
    }
}
