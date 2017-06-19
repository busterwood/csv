using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
    public interface ISchemaed
    {
        Schema Schema { get; }            
    }

    /// <summary>The metadata for a <see cref="DataSequence"/> of <see cref="Row"/></summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public struct Schema : IReadOnlyList<Column>, IEquatable<Schema>
    {
        readonly Column[] columns;

        public Schema(string name, IEnumerable<Column> columns) 
            : this(name, columns?.ToArray())   // take a copy and convert to array (array is the lightest type and fasted to iterate)            
        { }

        public Schema(string name, params Column[] columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (columns.Length == 0)
                throw new ArgumentException("Schema must have one or more columns");
            CheckColumnsAreUnqiue(columns);

            Name = name;
            this.columns = columns;
        }

        static void CheckColumnsAreUnqiue(Column[] columns)
        {
            var unique = new HashSet<Column>();
            foreach (var c in columns)
            {
                if (!unique.Add(c))
                    throw new ArgumentException($"Schema must have unqiue columns: {c} is duplicated");
            }
        }

        /// <summary>The name of this schema (optional)</summary>
        public string Name { get; }

        public int Count => columns?.Length ?? 0;

        public Column this[int index] => columns[index];

        /// <remarks>looking up the index will be fine if number of columns in low (16 or less), no need for a dictionary</remarks>
        public int ColumnIndex(string name) => columns.IndexOf(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        public IEnumerator<Column> GetEnumerator() => ((IEnumerable<Column>)columns).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => columns.GetEnumerator();

        public bool Equals(Schema other) => Count == other.Count && Enumerable.SequenceEqual(columns, other.columns);
        public override bool Equals(object obj) => obj is Schema && Equals((Schema)obj);
        public override int GetHashCode() => columns?.Sum(c => c.GetHashCode()) ?? 0;

        /// <summary>Does the <paramref name="left"/> schema has the same set of columns as the <param name="right"/> schema? (column order does not matter)</summary>
        public static bool SetEquals(Schema left, Schema right) => left.All(l => right.Contains(l));

        public static Schema Merge(Schema left, Schema right) => new Schema($"Merge of {left.Name} and {right.Name}", left.Concat(right.Where(r => !left.Contains(r))));
    }

    /// <remarks>This type is immutable and cannot be changed (mutated)</remarks>
    public struct Column : IEquatable<Column>
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

        public bool Equals(Column other) => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && Type == other.Type;
        public override bool Equals(object obj) => obj is Column && Equals((Column)obj);
        public override int GetHashCode() => Name?.GetHashCode() + Type?.GetHashCode() ?? 0;
        public override string ToString() => Name;
    }


    public static partial class Extensions
    {
        public static int IndexOf<T>(this T[] items, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                    return i;
                i++;
            }
            return -1;
        }
    }
}
