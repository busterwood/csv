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
using System.Data;
using System.Linq;

namespace BusterWood.Data
{
    public static class DataReaderExtensions
    {
        public static Relation ToRelation(this IDataReader reader, string name)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            var cols = Columns(reader).ToArray();
            var schema = new Schema(name, cols);
            return new DbRelation(reader, cols, schema);
        }

        public static Schema ToSchema(this IDataReader reader, string name)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return new Schema(name, Columns(reader));
        }

        static IEnumerable<Column> Columns(IDataReader reader) => Enumerable.Range(0, reader.FieldCount).Select(i => new Column(reader.GetName(i), reader.GetFieldType(i)));

        class DbRelation : Relation
        {
            readonly IDataReader reader;
            readonly Column[] columns;

            public DbRelation(IDataReader reader, Column[] columns, Schema schema) : base(schema)
            {
                this.reader = reader;
                this.columns = columns;
            }

            protected override IEnumerable<Row> GetSequence()
            {
                while (reader.Read())
                {
                    var values = new object[Schema.Count];
                    reader.GetValues(values);
                    yield return new OrderedArrayRow(Schema, columns, values);
                }
            }
        }

    }
}