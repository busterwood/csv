/* Copyright 2017 BusterWood

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License. 
*/
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

        public static Relation CsvToRelation(this TextReader reader, string relationName, char delimiter=',')
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            var orderedColumns = ParseColumns(reader.ReadLine(), delimiter).ToArray();
            return new CsvRelation(reader, orderedColumns, relationName, delimiter);
        }

        static IEnumerable<Column> ParseColumns(string headerLine, char delimiter)
        {
            if (headerLine == null)
                throw new ArgumentException("Header line is missing");
            var header = headerLine.Split(delimiter);
            if (header.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Column name is missing from header line: " + headerLine);
            return header.Select(h => new Column(h, typeof(string)));
        }

        class CsvRelation : Relation
        {
            readonly TextReader reader;
            readonly char delimiter;
            readonly Column[] columns;

            public CsvRelation(TextReader reader, Column[] columns, string schemaName, char delimiter) : base(new Schema(schemaName, columns))
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