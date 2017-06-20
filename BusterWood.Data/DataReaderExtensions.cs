using System;
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
            var cols = Columns(reader).ToArray();
            Schema schema = new Schema(name, cols);
            return new DbDataSequence(reader, cols, schema);
        }

        public static Schema ToSchema(this IDataReader reader, string name)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return new Schema(name, Columns(reader));
        }

        static IEnumerable<Column> Columns(IDataReader reader)
        {
            return Enumerable.Range(0, reader.FieldCount).Select(i => new Column(reader.GetName(i), reader.GetFieldType(i)));
        }

        class DbDataSequence : DataSequence
        {
            readonly IDataReader reader;
            readonly Column[] columns;

            public DbDataSequence(IDataReader reader, Column[] columns, Schema schema) : base(schema)
            {
                this.columns = columns;
                this.reader = reader;
            }

            public override IEnumerator<Row> GetEnumerator()
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