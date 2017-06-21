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
    public struct Schema : IReadOnlyCollection<Column>, IEquatable<Schema>
    {
        internal readonly Column[] columns;
        readonly int hashCode;

        public Schema(string name, IEnumerable<Column> columns) : this(name, columns?.ToArray())
        {
        }

        public Schema(string name, params Column[] columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (columns.Length == 0)
                throw new ArgumentException($"Schema '{name}' must have one or more columns");
            
            Name = name;
            this.columns = columns;

            var temp = new HashSet<string>(Column.NameEquality);
            foreach (var c in columns)
            {
                if (temp.Contains(c.Name))
                    throw new ArgumentException($"Schema must have unqiue columns: {c} is duplicated");
                temp.Add(c.Name);
            }

            hashCode = columns.Aggregate(0, (hc, c) => { unchecked { return hc + c.GetHashCode(); } });
        }

        /// <summary>The name of this schema (optional)</summary>
        public string Name { get; }

        public int Count => columns?.Length ?? 0;

        public Column this[string name]
        {
            get
            {
                var eq = Column.NameEquality;
                foreach (var c in columns)
                {
                    if (eq.Equals(c.Name, name))
                        return c;
                }
                throw new UnknownColumnException($"Cannot find column '{name}' in schema '{Name}'");
            }
        }

        public IEnumerator<Column> GetEnumerator() => ((IEnumerable<Column>)columns).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => columns.GetEnumerator();

        public bool Equals(Schema other) => hashCode == other.hashCode && Count == other.Count && columns.All(other.columns.Contains);
        public override bool Equals(object obj) => obj is Schema && Equals((Schema)obj);
        public override int GetHashCode() => hashCode;

        public static bool operator ==(Schema left, Schema right) => left.Equals(right);
        public static bool operator !=(Schema left, Schema right) => !left.Equals(right);

        //public static Schema Merge(Schema left, Schema right) => 
        //    new Schema($"Merge of {left.Name} and {right.Name}", left.columns.AddRange(right.Where(r => !left.columns.ContainsKey(r.Name)).Select(c => new KeyValuePair<string, Column>(c.Name, c))));

        internal void ThrowWhenUnknownColumn(string name)
        {
            if (columns?.Any(c => c.NameEquals(name)) != true)
                throw new UnknownColumnException($"Unknown column {name} in schema '{Name}'");
        }
    }

    /// <remarks>This type is immutable and cannot be changed (mutated)</remarks>
    public struct Column : IEquatable<Column>
    {
        internal static readonly IEqualityComparer<string> NameEquality = StringComparer.OrdinalIgnoreCase;

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

        public bool NameEquals(string name) => NameEquality.Equals(Name, name);
        public bool Equals(Column other) => NameEquals(other.Name) && Type == other.Type;
        public override bool Equals(object obj) => obj is Column && Equals((Column)obj);
        public override int GetHashCode() => (Name?.GetHashCode() ?? 0) + (Type?.GetHashCode() ?? 0);
        public override string ToString() => Name;

        public static bool operator ==(Column left, Column right) => left.Equals(right);
        public static bool operator !=(Column left, Column right) => !left.Equals(right);
    }

    public static partial class Extensions
    {
        public static ColumnValue Value(this Column column, object val) => new ColumnValue(column, val);
    }
}
