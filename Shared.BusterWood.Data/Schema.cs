using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
    /// <summary>The metadata for a <see cref="DataSequence"/> of <see cref="Row"/></summary>
    public struct Schema
    {
        readonly Column[] columns;

        public Schema(string name, IEnumerable<Column> columns)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Name = name;

            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            this.columns = columns.ToArray(); // take a copy and convert to array (array is the lightest type and fasted to iterate)

            if (this.columns.Length == 0)
                throw new ArgumentException("Schema must have one or more columns");
        }

        /// <summary>The name of this schema</summary>
        public string Name { get; }

        /// <summary>The columns </summary>
        public IReadOnlyList<Column> Columns => columns;

        /// <remarks>
        /// looking up the index will be fine if number of columns in low (16 or less), no need for a dictionary
        /// </remarks>
        public int ColumnIndex(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            int i = 0;
            foreach (var c in Columns)
            {
                if (string.Equals(c.Name, name, comparison))
                    return i;
                i++;
            }
            return -1;
        }
    }

    public struct Column
    {
        public Column(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (type == null) throw new ArgumentNullException(nameof(type));
            Name = name;
            Type = type;
        }

        /// <summary>Name of this column</summary>
        public string Name { get; }

        /// <summary>the <see cref="System.Type"/> of this column</summary>
        public Type Type { get; }

        public override string ToString() => Name;
    }

    /// <summary>A sequenece of rows which all have the same <see cref="Schema"/></summary>
    /// <remarks>See <see cref="CsvReaderExtensions.ToCsvDataSequence(System.IO.TextReader, string, char)"/> and <see cref="DataReaderExtensions.ToDataSequence(System.Data.IDataReader, string)"/></remarks>
    public abstract class DataSequence : IEnumerable<Row>
    {
        protected DataSequence(Schema schema)
        {
            Schema = schema;
        }
        
        /// <summary>The schema that applies to all rows in this sequence</summary>
        public Schema Schema { get; }

        public abstract IEnumerator<Row> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>A row of data with a defined <see cref="Schema"/></summary>
    public struct Row : IReadOnlyList<ColumnValue>
    {
        readonly object[] values;

        public Row(Schema schema, object[] values)
        {
            this.values = values;
            Schema = schema;
        }

        public Schema Schema { get; }

        public object Get(int index) => values[index];

        public T Get<T>(int column)
        {
            var val = values[column];
            if (val == null && Schema.Columns[column].Type.IsValueType)
                return default(T);
            return (T)val;
        }

        public ColumnValue this[int index] => new ColumnValue(Schema.Columns[index], values[index]);

        int IReadOnlyCollection<ColumnValue>.Count => Schema.Columns.Count;

        public IEnumerator<ColumnValue> GetEnumerator()
        {
            int i = 0;
            foreach (var col in Schema.Columns)
                yield return new ColumnValue(col, values[i++]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public struct ColumnValue
    {
        public ColumnValue(Column column, object value)
        {
            Column = column;
            Value = value;
        }

        public Column Column { get; }
        public object Value { get; }
        public string Name => Column.Name;
        public override string ToString() => $"{Name} = {Value}";
    }

    public static class Extensions
    {
        public static object Get(this Row row, string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase) => row.Get(row.Schema.ColumnIndex(name, comparison));
        public static T Get<T>(this Row row, string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase) => (T)row.Get(name, comparison);
    }
}
