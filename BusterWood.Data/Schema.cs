using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        internal readonly ImmutableDictionary<string, Column> columns;

        public Schema(string name, ImmutableDictionary<string, Column> columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (columns.Count == 0)
                throw new ArgumentException($"Schema '{name}' must have one or more columns");

            Name = name;
            this.columns = columns;

        }

        public Schema(string name, IEnumerable<Column> columns) 
        {
            Name = name;
            var temp = ImmutableDictionary<string, Column>.Empty.ToBuilder();
            temp.KeyComparer = StringComparer.OrdinalIgnoreCase;
            foreach (var c in columns)
            {
                if (temp.ContainsKey(c.Name))
                    throw new ArgumentException($"Schema must have unqiue columns: {c} is duplicated");
                temp.Add(c.Name, c);
            }
            if (temp.Count == 0)
                throw new ArgumentException($"Schema '{name}' must have one or more columns");
            this.columns = temp.ToImmutable();
        }

        public Schema(string name, params Column[] columns)
            : this(name, (IEnumerable<Column>)columns)
        {
        }

        /// <summary>The name of this schema (optional)</summary>
        public string Name { get; }

        public int Count => columns?.Count ?? 0;

        public Column this[string name]
        {
            get
            {
                Column col;
                if (!columns.TryGetValue(name, out col))
                    throw new UnknownColumnException($"Cannot find column '{name}' in schema '{Name}'");
                return col;
            }
        }

        public IEnumerator<Column> GetEnumerator() => columns.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => columns.GetEnumerator();

        public bool Equals(Schema other) => Count == other.Count && Enumerable.SequenceEqual(columns, other.columns);
        public override bool Equals(object obj) => obj is Schema && Equals((Schema)obj);
        public override int GetHashCode() => columns?.Sum(c => c.GetHashCode()) ?? 0;

        public static bool operator ==(Schema left, Schema right) => left.Equals(right);
        public static bool operator !=(Schema left, Schema right) => !left.Equals(right);

        public static Schema Merge(Schema left, Schema right) => 
            new Schema($"Merge of {left.Name} and {right.Name}", left.columns.AddRange(right.Where(r => !left.columns.ContainsKey(r.Name)).Select(c => new KeyValuePair<string, Column>(c.Name, c))));

        internal void ThrowWhenUnknownColumn(string name)
        {
            if (columns?.ContainsKey(name) != true)
                throw new UnknownColumnException($"Unkown column {name} in schema '{Name}'");
        }
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

        public static bool operator ==(Column left, Column right) => left.Equals(right);
        public static bool operator !=(Column left, Column right) => !left.Equals(right);
    }

    public static partial class Extensions
    {
        public static ColumnValue Value(this Column column, object val) => new ColumnValue(column, val);
    }
}
