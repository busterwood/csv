using System;
using System.Collections.Generic;

namespace BusterWood.Data
{
    /// <summary>An attribute of a relation. Called Column to avoid conflict with System.Attribute</summary>
    /// <remarks>This type is immutable and cannot be changed (mutated)</remarks>
    public struct Column : IEquatable<Column>
    {
        internal static readonly IEqualityComparer<string> NameEquality = StringComparer.OrdinalIgnoreCase;

        public Column(string name, Type type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>Name of this column</summary>
        public string Name { get; }

        /// <summary>the <see cref="System.Type"/> of this column</summary>
        public Type Type { get; }

        public bool NameEquals(string name) => NameEquality.Equals(Name, name);
        public bool Equals(Column other) => NameEquals(other.Name) && Type == other.Type;
        public override bool Equals(object obj) => obj is Column && Equals((Column)obj);
        public override int GetHashCode() => NameHashCode() + (Type?.GetHashCode() ?? 0);
        int NameHashCode() => Name == null ? 0 : NameEquality.GetHashCode(Name);
        public override string ToString() => Name;

        public static bool operator ==(Column left, Column right) => left.Equals(right);
        public static bool operator !=(Column left, Column right) => !left.Equals(right);
    }


    public static partial class Extensions
    {
        public static ColumnValue Value(this Column column, object val) => new ColumnValue(column, val);
    }
}
