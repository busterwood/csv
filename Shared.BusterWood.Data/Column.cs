using System;
using System.Collections.Generic;

namespace BusterWood.Data
{
    /// <summary>An attribute of a relation. Called Column to avoid conflict with System.Attribute</summary>
    /// <remarks>This type is immutable and cannot be changed (mutated)</remarks>
    public struct Column : IEquatable<Column>
    {
        internal static readonly IEqualityComparer<string> NameEquality = StringComparer.OrdinalIgnoreCase;
        static readonly Type[] allowedTypes = { typeof(String), typeof(int), typeof(long), typeof(bool), typeof(double), typeof(decimal), typeof(DateTimeOffset), typeof(short) };

        public Column(string name, Schema schema) : this(name, (object)schema)
        {
        }

        public Column(string name, Type type) : this(name, (object)type)
        {
        }

        internal Column(string name, object type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            if (!IsAllowed(type)) throw new ArgumentOutOfRangeException(nameof(type), type, $"Not allowed");
        }

        public static bool IsAllowed(object type) => Array.IndexOf(allowedTypes, type) >= 0 || type is Schema;

        /// <summary>Name of this column</summary>
        public string Name { get; }

        /// <summary>the <see cref="System.Type"/> of this column or <see cref="Schema"/> for a nested relation</summary>
        public object Type { get; }

        public bool NameEquals(string name) => NameEquality.Equals(Name, name);
        public bool Equals(Column other) => NameEquals(other.Name) && Type == other.Type;
        public override bool Equals(object obj) => obj is Column && Equals((Column)obj);
        public override int GetHashCode() => (Name?.GetHashCode() ?? 0) ^ (Type?.GetHashCode() ?? 0);
        public override string ToString() => Name;
        public static bool operator ==(Column left, Column right) => left.Equals(right);
        public static bool operator !=(Column left, Column right) => !left.Equals(right);
    }

    public static partial class Extensions
    {
        public static ColumnValue Value(this Column column, object val) => new ColumnValue(column, val);
    }
}
