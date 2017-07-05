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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
    /// <summary>A sequenece of UNIQUE rows which all have the same <see cref="Schema"/></summary>
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
        protected abstract IEnumerable<Row> GetSequence();

        public IEnumerator<Row> GetEnumerator() => GetSequence().Distinct().GetEnumerator(); // all rows must be unique

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

    /// <summary>Materialized sequence that can be efficiently enumerated multiple times</summary>
    public class DataList : DataSequence, IReadOnlyCollection<Row>
    {
        readonly IReadOnlyList<Row> rows;

        public DataList(Schema schema, IEnumerable<Row> rows) : base(schema)
        {
            this.rows = rows.ToList(); // this *could* be lazerly created
        }

        protected override IEnumerable<Row> GetSequence() => rows;

        public int Count => rows.Count;
    }

    public class DerivedDataSequence : DataSequence
    {
        readonly IEnumerable<Row> rows;

        public DerivedDataSequence(Schema schema, IEnumerable<Row> rows) : base(schema)
        {
            this.rows = rows;
        }

        protected override IEnumerable<Row> GetSequence() => rows;
    }

    /// <summary>Base class for rows of data with a fixed <see cref="Schema"/></summary>
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

        protected static T ValueOrDefault<T>(object val) => val == null && typeof(T).IsValueType ? default(T) : (T)val;

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
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            if (values.Length != schema.Count) throw new ArgumentException("number of values does not match number of columns", nameof(values));
            this.columns = columns;
            this.values = values;
        }

        public override object Get(string name)
        {
            Schema.ThrowWhenUnknownColumn(name);  // allow column restriction without copying rows
            var idx = columns.IndexOf(col => Column.NameEquality.Equals(col.Name, name));
            return values[idx];
        }
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
        public override int GetHashCode() => Column.GetHashCode() + (Value?.GetHashCode() ?? 0);

        public static bool operator ==(ColumnValue left, ColumnValue right) => left.Equals(right);
        public static bool operator !=(ColumnValue left, ColumnValue right) => !left.Equals(right);
    }

    public static partial class Extensions
    {
        public static Dictionary<string, object> ToDictionary(this Row row) => row.ToDictionary(cv => cv.Name, cv => cv.Value);

        public static int IndexOf<T>(this T[] items, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var item in items) // foreach loop on array avoids array bounds checks
            {
                if (predicate(item))
                    return i;
                i++;
            }
            return -1;
        }
    }

}
