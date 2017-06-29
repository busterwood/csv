using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BusterWood.Data.Shared
{
    static class Extensions
    {
        /// <summary>Returns the value after a command line <paramref name="flag"/>, or the <paramref name="default"/> if the <paramref name="flag"/>, was not found</summary>
        /// <remarks>Removes the <paramref name="flag"/> and value from the list, if the <paramref name="flag"/> was found</remarks>
        public static string StringFlag(this List<string> args, string flag, string @default = null)
        {
            int index = args.IndexOf(flag);
            if (index < 0 || index + 1 == args.Count)
                return @default;
            args.RemoveAt(index);   // remove flag
            var value = args[index];
            args.RemoveAt(index);   // remove value
            return value;
        }

        public static string MandatoryStringFlag(this List<string> args, string flag)
        {
            var val = args.StringFlag(flag);
            if (val == null)
                throw new HelpException($"Flag {flag} must be provided");
            return val;
        }

        public static string Join(this IEnumerable items, string separator = ",") => string.Join(separator, items.Cast<object>());

    }

    public static class Args
    { 
        public static DataSequence GetDataSequence(List<string> args)
        {
            var filePath = args.StringFlag("--file");
            TextReader input = filePath == null ? Console.In : new StreamReader(filePath);
            return input.ToCsvDataSequence(args.FirstOrDefault() ?? "stdin");
        }

        public static Func<string, bool> KeepOrRemoveDupLines(List<string> args)
        {
            if (args.Remove("--dups")) // keep duplicates
                return line => true;

            // remove duplicate lines
            var soFar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return line => soFar.Add(line);
        }

        public static void CheckColumnsAreValid(IEnumerable<string> args, Schema schema)
        {
            if (!args.Any())
                throw new Exception("You must supply one or more columns");

            var invalidArgs = args.Where(a => !schema.Contains(a)).ToList();
            if (invalidArgs.Count > 0)
                throw new Exception("One or more columns do not exist: " + invalidArgs.Join());
        }
    }

    [System.Serializable]
    class HelpException : Exception
    {
        public HelpException() { }
        public HelpException(string message) : base(message) { }
        public HelpException(string message, Exception inner) : base(message, inner) { }
        protected HelpException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    static class StdErr
    {
        public static readonly string Name = Assembly.GetEntryAssembly().GetName().Name;

        public static void Warning(string message) => WriteLine(message, ConsoleColor.Red);

        public static void Info(string message) => WriteLine(message, ConsoleColor.Cyan);

        public static void WriteLine(string message, ConsoleColor color)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Error.WriteLine($"{Name}: {message}");
            Console.ForegroundColor = prev;
        }
    }
    
}
