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
using System.Text;

namespace BusterWood.Data
{
    public static class CsvReaderExtensions
    {
        public static string ToCsv(this Schema schema, char delimiter = ',') => string.Join(delimiter.ToString(), schema.Select(c => c.Name));

        public static string ToCsv(this Row row, char delimiter = ',')
        {
            StringBuilder sb = new StringBuilder(80);
            foreach (var cv in row)
            {
                var value = cv.Value.ToString();
                if (value.IndexOf(delimiter) >= 0)
                    sb.Append('"').Append(value).Append('"');
                else
                    sb.Append(value);
                sb.Append(delimiter);
            }
            sb.Length -= 1; // remove last delimiter
            return sb.ToString();
        }

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
            readonly StringBuilder buffer = new StringBuilder();

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
                    string[] values = ParseLine();
                    if (values == null)
                        break;
                    yield return new OrderedArrayRow(Schema, columns, values);
                }
            }

            string[] ParseLine()
            {
                var values = new string[columns.Length];
                int vi = 0;
                for (;;)
                {
                    int next = reader.Read();
                    if (next == -1) // end of file
                    {
                        if (vi == 0 && buffer.Length == 0) // end of file on empty line
                            return null;

                        // end of file at end of line of values
                        values[vi] = buffer.ToString();
                        buffer.Clear();
                        vi++;
                        break;
                    }

                    char ch = (char)next;
                    if (ch == ',') // end of value
                    {                        
                        values[vi] = buffer.ToString();
                        buffer.Clear();
                        vi++;
                    }
                    else if (ch == '\r') // carrage return
                    {
                        // skip
                    }
                    else if (ch == '\n')  // end of line
                    {
                        
                        values[vi] = buffer.ToString();
                        buffer.Clear();
                        vi++;
                        break;
                    }
                    else if (ch == '"') // read quoted value
                    {                        
                        for(;;)
                        {
                            next = reader.Read();
                            if (next == -1) // end of file
                                throw new FormatException("Unexpected end of quoted value");

                            ch = (char)next;
                            if (ch == '"')  // end of quoted value
                                break; 
                            else // normal char inside quotes
                                buffer.Append(ch);
                        }
                    }
                    else // a normal char
                    {
                        buffer.Append(ch);
                    }
                }

                for (; vi < values.Length; vi++)
                {
                    values[vi] = "";
                }
                return values;
            }

        }

    }
}