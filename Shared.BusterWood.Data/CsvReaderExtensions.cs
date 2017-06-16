using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Data
{
    public static class CsvReaderExtensions
    {
        public static string ToCsv(this Schema schema, char delimiter = ',') => string.Join(delimiter.ToString(), schema.Columns.Select(c => c.Name));

        public static string ToCsv(this Row row, char delimiter = ',') => string.Join(delimiter.ToString(), row.Select(r => r.Value));

        public static DataSequence ToCsvDataSequence(this TextReader reader, string name, char delimiter = ',')
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var headerLine = reader.ReadLine();
            if (headerLine == null)
                throw new ArgumentException("Header line is missing");

            var header = headerLine.Split(delimiter);
            if (header.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Column name is missing from header line: " + headerLine);

            var cols = header.Select(h => new Column(h, typeof(string)));
            var schema = new Schema(name, cols);
            return new CsvDataSequence(reader, schema, delimiter);
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
                var colcount = Schema.Columns.Count;
                for(;;)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        yield break;

                    var values = line.Split(delimiter);
                    if (values.Length < Schema.Columns.Count)
                        values = values.Concat(Enumerable.Repeat("", Schema.Columns.Count - values.Length)).ToArray(); // some missing data, report this via event?
                    yield return new Row(Schema, values);
                }
            }
            
        }
    }
}