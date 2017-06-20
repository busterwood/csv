using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace BusterWood.Data
{
    public static partial class Extensions
    {
        /// <summary>
        /// Filter the source releation, e.g. a "Where" clause
        /// </summary>
        public static DataSequence Restrict(this DataSequence seq, Func<Row, bool> predicate) => new DerivedDataSequence(seq.Schema, seq.Where(predicate));

        /// <summary>Returns a new sequence with that only contains the requested <paramref name="columns"/> from the source <paramref name="seq"/></summary>
        public static DataSequence Project(this DataSequence seq, params string[] columns)
        {
            var existing = seq.Schema.columns;
            var set = new HashSet<string>(columns, StringComparer.OrdinalIgnoreCase);
            var newSchema = existing.RemoveRange(existing.Keys.Where(k => !set.Contains(k)));
            return new DerivedDataSequence(new Schema("", newSchema), seq); //TODO: do we need to remove data from each row?
        }

        /// <summary>Returns a new sequence with <paramref name="columns"/> removed from the source <paramref name="seq"/></summary>
        public static DataSequence ProjectAway(this DataSequence seq, params string[] columns)
        {
            var existing = seq.Schema.columns;
            var newSchema = existing.RemoveRange(columns);
            return new DerivedDataSequence(new Schema("", newSchema), seq); //TODO: do we need to remove data from each row?
        }

        public static DataSequence Extend<T>(this DataSequence seq, string columnName, Func<Row, T> func)
        {
            var existing = seq.Schema.columns;
            var col = new Column(columnName, typeof(T));
            var newSchema = new Schema("", existing.Add(columnName, col));
            var rows = seq.Select(r => new DerivedRow(newSchema, r, new ColumnValue(col, func(r))));
            return new DerivedDataSequence(newSchema, rows);
        }

        private class DerivedRow : Row
        {
            private Row baseRow;
            private ColumnValue extension;

            public DerivedRow(Schema schema, Row baseRow, ColumnValue columnValue) : base(schema)
            {
                this.baseRow = baseRow;
                this.extension = columnValue;
            }

            public override object Get(string name)
            {
                if (string.Equals(name, extension.Name, StringComparison.OrdinalIgnoreCase))
                    return extension.Value;
                return baseRow.Get(name);
            }

            //TODO: override other methods?
        }
    }
}