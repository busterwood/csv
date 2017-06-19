using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
    /// <summary>A sequenece of rows which all have the same <see cref="Schema"/></summary>
    /// <remarks>See <see cref="CsvReaderExtensions.ToCsvDataSequence(System.IO.TextReader, string, char)"/> and <see cref="DataReaderExtensions.ToDataSequence(System.Data.IDataReader, string)"/></remarks>
    public abstract class DataSequence : IEnumerable<Row>, ISchemaed
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
    public abstract class Row : IReadOnlyList<ColumnValue>, ISchemaed, IEquatable<Row>
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
        public virtual T Get<T>(int index) => ValueOrDefault<T>(Get(index));

        protected static T ValueOrDefault<T>(object val) => val == null && typeof(T).IsValueType ? default(T) : (T)val;

        /// <summary>Returns the value of a <see cref="Column"/> with the specified <paramref name="name"/></summary>
        public virtual object Get(string name) => Get(Schema.ColumnIndex(name));

        /// <summary>Returns the value of a <see cref="Column"/> with the specified <paramref name="name"/></summary>
        public virtual T Get<T>(string name) => ValueOrDefault<T>(Get(name));

        /// <summary>Returns a sequnce of values for each <see cref="Column"/> in the <see cref="Schema"/></summary>
        public virtual IEnumerator<ColumnValue> GetEnumerator()
        {
            int i = 0;
            foreach (var col in Schema)
                yield return new ColumnValue(col, Get(i++));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(Row other) => Schema == other?.Schema && Enumerable.SequenceEqual(this, other);
        public override bool Equals(object obj) => Equals(obj as Row);
        public override int GetHashCode() => this.Aggregate(0, (hash, cv) => { unchecked { return hash + cv.GetHashCode(); } });

        public static bool operator ==(Row left, Row right) => Equals(left, right);
        public static bool operator !=(Row left, Row right) => !Equals(left, right);
    }

    /// <summary>A row of data with a defined <see cref="Schema"/></summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public class ArrayRow : Row
    {
        readonly object[] values;

        public ArrayRow(Schema schema, params object[] values) : base(schema)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length != schema.Count) throw new ArgumentException("number of values does not match number of columns", nameof(values));
            this.values = values;
        }

        public override object Get(int index) => values[index];
    }

    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public struct ColumnValue : IEquatable<ColumnValue>
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

        public bool Equals(ColumnValue other) => Column == other.Column && Equals(Value, other.Value);
        public override bool Equals(object obj) => obj is ColumnValue && Equals((ColumnValue)obj);
        public override int GetHashCode() => Column.GetHashCode() + Value?.GetHashCode() ?? 0;

        public static bool operator ==(ColumnValue left, ColumnValue right) => left.Equals(right);
        public static bool operator !=(ColumnValue left, ColumnValue right) => !left.Equals(right);
    }

    public static partial class Extensions
    {
        public static Dictionary<string, object> ToDictionary(this Row row) => row.ToDictionary(cv => cv.Name, cv => cv.Value);        
    }

}
