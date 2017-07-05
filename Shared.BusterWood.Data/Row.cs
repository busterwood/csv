using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Data
{
    /// <summary>A relational tuple with a fixed <see cref="Schema"/>, but called Row to avoid conflict with System.Tuple</summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public abstract class Row : IReadOnlyCollection<ColumnValue>, ISchemaed, IEquatable<Row>
    {
        public Schema Schema { get; }

        protected Row(Schema schema)
        {
            Schema = schema;
        }

        int IReadOnlyCollection<ColumnValue>.Count => Schema.Count;

        ///// <summary>Returns the <see cref="ColumnValue"/> of this <see cref="Row"/> with the specified <paramref name="index"/></summary>
        //public virtual ColumnValue this[string name] => new ColumnValue(Schema[name], Get(name));

        protected static T ValueOrDefault<T>(object val) => val == null && typeof(T).GetTypeInfo().IsValueType ? default(T) : (T)val;

        /// <summary>Returns the value of a <see cref="Column"/> with the specified <paramref name="name"/></summary>
        public abstract object Get(string name);

        /// <summary>Returns the value of a <see cref="Column"/> with the specified <paramref name="name"/></summary>
        public virtual T Get<T>(string name) => ValueOrDefault<T>(Get(name));

        //TODO: check the schema type before attempting type conversion
        public string String(string name) => (string)Get(name);
        public short Short(string name) => ValueOrDefault<short>(Get(name));
        public int Int(string name) => ValueOrDefault<int>(Get(name));
        public long Long(string name) => ValueOrDefault<long>(Get(name));
        public double Double(string name) => ValueOrDefault<double>(Get(name));
        public decimal Decimal(string name) => ValueOrDefault<decimal>(Get(name));
        public bool Bool(string name) => ValueOrDefault<bool>(Get(name));
        public DateTime DateTime(string name) => ValueOrDefault<DateTime>(Get(name));

        /// <summary>Returns a sequnce of values for each <see cref="Column"/> in the <see cref="Schema"/></summary>
        public virtual IEnumerator<ColumnValue> GetEnumerator() => Schema.Select(col => new ColumnValue(col, Get(col.Name))).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(Row other) => Schema == other.Schema && this.All(l => other.Contains(l));
        public override bool Equals(object obj) => Equals(obj as Row);

        public override int GetHashCode()
        {
            // optimized code - use the schema's hash code + the hash code of all values
            // which is faster that summing all the ColumnValue's hash codes 
            // as it avoid calculating hash codes for each Column
            unchecked
            {
                var hc = Schema.GetHashCode(); // get the cached shashcode from the schema, 
                foreach (var col in Schema)
                    hc += Get(col.Name)?.GetHashCode() ?? 0; // add on all the values
                return hc;
            }
        }

        public static bool operator ==(Row left, Row right) => Equals(left, right);
        public static bool operator !=(Row left, Row right) => !Equals(left, right);
    }

    /// <summary>A row of data with a defined <see cref="Schema"/></summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public class ArrayRow : Row
    {
        readonly ColumnValue[] values;

        public ArrayRow(Schema schema, params ColumnValue[] values) : base(schema)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length != schema.Count) throw new ArgumentException("number of values does not match number of columns", nameof(values));
            this.values = values;
        }

        public override object Get(string name)
        {
            Schema.ThrowWhenUnknownColumn(name);  // allow column restriction without copying rows
            var idx = values.IndexOf(col => Column.NameEquality.Equals(col.Name, name));
            return values[idx];
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hc = 0;
                foreach (var cv in this)
                    hc += cv.GetHashCode();
                return hc;
            }
        }
    }

    /// <summary>A row of data with a defined <see cref="Schema"/></summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public class OrderedArrayRow : Row
    {
        readonly Column[] columns;
        readonly object[] values;

        public OrderedArrayRow(Schema schema, Column[] columns, object[] values) : base(schema)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length != schema.Count) throw new ArgumentException("number of values does not match number of columns", nameof(values));
            this.columns = columns ?? throw new ArgumentNullException(nameof(columns));
            this.values = values;
        }

        public override object Get(string name)
        {
            Schema.ThrowWhenUnknownColumn(name);  // allow column restriction without copying rows
            var idx = columns.IndexOf(col => Column.NameEquality.Equals(col.Name, name));
            return values[idx];
        }
    }
}
