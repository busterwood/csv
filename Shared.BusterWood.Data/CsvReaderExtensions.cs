using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Data
{
    public static class CsvReaderExtensions
    {
        public static string ToCsv(this Schema schema, char delimiter = ',') => string.Join(delimiter.ToString(), schema.Select(c => c.Name));

        public static string ToCsv(this Row row, char delimiter = ',') => string.Join(delimiter.ToString(), row.Select(r => r.Value));

        public static DataSequence ToCsvDataSequence(this TextReader reader, string name, char delimiter = ',')
        {
            Schema schema = ToSchema(reader, name, delimiter);
            return new CsvDataSequence(reader, schema, delimiter);
        }

        public static Schema ToSchema(this TextReader reader, string name, char delimiter)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return ToSchema(reader.ReadLine(), name, delimiter);
        }

        static Schema ToSchema(string headerLine, string name, char delimiter)
        {
            return new Schema(name, ParseColumns(headerLine, delimiter));
        }

        static IEnumerable<Column> ParseColumns(string headerLine, char delimiter)
        {
            if (headerLine == null)
                throw new ArgumentException("Header line is missing");

            var header = headerLine.Split(delimiter);
            if (header.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Column name is missing from header line: " + headerLine);

            var cols = header.Select(h => new Column(h, typeof(string)));
            return cols;
        }

        class CsvDataSequence : DataSequence
        {
            readonly TextReader reader;
            readonly char delimiter;

            public CsvDataSequence(TextReader reader, Schema schema, char delimiter) : base(schema)
            {
                this.reader = reader;
                this.delimiter = delimiter;
            }

            public override IEnumerator<Row> GetEnumerator()
            {
                for(;;)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        yield break;

                    string[] values = ParseLine(line);
                    yield return new ArrayRow(Schema, values);
                }
            }

            string[] ParseLine(string line)
            {
                var values = line.Split(delimiter);
                if (values.Length < Schema.Count)
                    values = PadLine(values);
                return values;
            }

            private string[] PadLine(string[] values)
            {
                return values.Concat(Enumerable.Repeat("", Schema.Count - values.Length)).ToArray(); // some missing data, report this via event?
            }
        }
    }
}