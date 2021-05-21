using System;

namespace BusterWood.Data
{
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

}
