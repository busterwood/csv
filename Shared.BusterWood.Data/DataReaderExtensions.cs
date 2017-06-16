using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BusterWood.Data
{
    public static class DataReaderExtensions
    {
        public static DataSequence ToDataSequence(this IDataReader reader, string name)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var cols = Enumerable.Range(0, reader.FieldCount).Select(i => new Column(reader.GetName(i), reader.GetFieldType(i)));
            var schema = new Schema(name, cols);
            return new DbDataSequence(reader, schema);
        }

        class DbDataSequence : DataSequence
        {
            readonly IDataReader reader;

            public DbDataSequence(IDataReader reader, Schema schema) : base(schema)
            {
                this.reader = reader;
            }

            public override IEnumerator<Row> GetEnumerator()
            {
                var colcount = Schema.Columns.Count;
                while (reader.Read())
                {
                    var values = new object[colcount];
                    reader.GetValues(values);
                    yield return new Row(Schema, values);
                }
            }

        }        
    }
}