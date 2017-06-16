using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
    /// <summary>The metadata for a <see cref="DataSequence"/> of <see cref="Row"/></summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public struct Schema : IReadOnlyList<Column>
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

        public int Count => columns.Length;

        public Column this[int index] => columns[index];

        /// <remarks>looking up the index will be fine if number of columns in low (16 or less), no need for a dictionary</remarks>
        public int ColumnIndex(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            int i = 0;
            foreach (var c in columns)
            {
                if (string.Equals(c.Name, name, comparison))
                    return i;
                i++;
            }
            return -1;
        }

        public IEnumerator<Column> GetEnumerator() => ((IReadOnlyList<Column>)columns).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => columns.GetEnumerator();
    }

    /// <remarks>This type is immutable and cannot be changed (mutated)</remarks>
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

        /// <summary>Returns a sequence of zero or more <see cref="Row"/></summary>
        public abstract IEnumerator<Row> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Base class for rows of data with a fixed <see cref="Schema"/></summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public abstract class Row : IReadOnlyList<ColumnValue>
    {
        public Schema Schema { get; }

        protected Row(Schema schema)
        {
            Schema = schema;
        }

        int IReadOnlyCollection<ColumnValue>.Count => Schema.Count;

        /// <summary>Returns the <see cref="ColumnValue"/> of this <see cref="Row"/> with the specified <paramref name="index"/></summary>
        public virtual ColumnValue this[int index] => new ColumnValue(Schema[index], Get(index));

        /// <summary>Returns the value of a <see cref="Column"/> with the specified <paramref name="index"/></summary>
        public abstract object Get(int index);

        /// <summary>Returns the value of a <see cref="Column"/> with the specified <paramref name="index"/></summary>
        public virtual T Get<T>(int column)
        {
            var val = Get(column);
            if (val == null && Schema[column].Type.IsValueType)
                return default(T);
            return (T)val;
        }

        /// <summary>Returns the value of a <see cref="Column"/> with the specified <paramref name="name"/></summary>
        public virtual object Get(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase) => Get(Schema.ColumnIndex(name, comparison));

        /// <summary>Returns the value of a <see cref="Column"/> with the specified <paramref name="name"/></summary>
        public virtual T Get<T>(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase) => (T)Get(name, comparison);

        /// <summary>Returns a sequnce of values for each <see cref="Column"/> in the <see cref="Schema"/></summary>
        public virtual IEnumerator<ColumnValue> GetEnumerator()
        {
            int i = 0;
            foreach (var col in Schema)
                yield return new ColumnValue(col, Get(i++));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>A row of data with a defined <see cref="Schema"/></summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public class ArrayRow : Row 
    {
        readonly object[] values;

        public ArrayRow(Schema schema, object[] values) : base(schema)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            this.values = values;
        }

        public override object Get(int index) => values[index];
    }

    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
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

        public static Dictionary<string, object> ToDictionary(this Row row) => row.ToDictionary(cv => cv.Name, cv => cv.Value);
    }
}
