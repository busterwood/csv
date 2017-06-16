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
            Schema schema = ToSchema(reader, name);
            return new DbDataSequence(reader, schema);
        }

        public static Schema ToSchema(this IDataReader reader, string name)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return new Schema(name, reader.Columns());
        }

        static IEnumerable<Column> Columns(this IDataReader reader)
        {
            return Enumerable.Range(0, reader.FieldCount).Select(i => new Column(reader.GetName(i), reader.GetFieldType(i)));
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
                while (reader.Read())
                {
                    var values = new object[Schema.Count];
                    reader.GetValues(values);
                    yield return new ArrayRow(Schema, values);
                }
            }
        }        
    }
}