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
            var orderedColumns = ParseColumns(reader, delimiter).ToArray();
            return new CsvDataSequence(reader, orderedColumns, name, delimiter);
        }

        public static Schema ToSchema(this TextReader reader, string name, char delimiter)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return ToSchema(reader.ReadLine(), name, delimiter);
        }

        static Schema ToSchema(string headerLine, string name, char delimiter) => new Schema(name, ParseColumns(headerLine, delimiter));

        static IEnumerable<Column> ParseColumns(TextReader reader, char delimiter) => ParseColumns(reader.ReadLine(), delimiter);

        static IEnumerable<Column> ParseColumns(string headerLine, char delimiter)
        {
            if (headerLine == null)
                throw new ArgumentException("Header line is missing");
            var header = headerLine.Split(delimiter);
            if (header.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Column name is missing from header line: " + headerLine);
            return header.Select(h => new Column(h, typeof(string)));
        }

        class CsvDataSequence : DataSequence
        {
            readonly TextReader reader;
            readonly char delimiter;
            readonly Column[] columns;

            public CsvDataSequence(TextReader reader, Column[] columns, string schemaName, char delimiter) : base(new Schema(schemaName, columns))
            {
                this.columns = columns;
                this.reader = reader;
                this.delimiter = delimiter;
            }


            protected override IEnumerable<Row> GetSequence()
            {
                for(;;)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        yield break;

                    string[] values = ParseLine(line);
                    yield return new OrderedArrayRow(Schema, columns, values);
                }
            }

            string[] ParseLine(string line)
            {
                var values = line.Split(delimiter);
                return values.Length == Schema.Count ? values : PadLine(values);
            }

            private string[] PadLine(string[] values)
            {
                return values.Concat(Enumerable.Repeat("", Schema.Count - values.Length)).ToArray(); // some missing data, report this via event?
            }
        }


    }
}