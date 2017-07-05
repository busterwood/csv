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
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
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

        public static Relation Distinct(this Relation rel) => new DerivedRelation(rel.Schema, ((IEnumerable<Row>)rel).Distinct());

        public static Relation Distinct(this Relation rel, bool enabled) => enabled ? Distinct(rel) : rel;

        /// <summary>Filter the source releation, e.g. a "Where" clause</summary>
        public static Relation Restrict(this Relation rel, Func<Row, bool> predicate) => new DerivedRelation(rel.Schema, Enumerable.Where(rel, predicate));

        /// <summary>Filter the source releation, e.g. a "Where" clause</summary>
        public static Relation RestrictAway(this Relation rel, Func<Row, bool> predicate) => new DerivedRelation(rel.Schema, Enumerable.Where(rel, row => !predicate(row)));

        /// <summary>Returns a new sequence with that only contains the requested <paramref name="columns"/> from the source <paramref name="rel"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static Relation Project(this Relation rel, IEnumerable<string> columns)
        {
            var cols = columns.Select(c => rel.Schema[c]);
            var newSchema = new Schema("", cols);
            var newRows = rel.Select(r => new ProjectedTuple(newSchema, r));
            return new DerivedRelation(newSchema, newRows);
        }

        /// <summary>Returns a new sequence with that only contains the requested <paramref name="columns"/> from the source <paramref name="rel"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static Relation Project(this Relation rel, params string[] columns) => Project(rel, (IEnumerable<string>)columns);

        /// <summary>Returns a new sequence with <paramref name="columns"/> removed from the source <paramref name="rel"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static Relation ProjectAway(this Relation rel, params string[] columns) => ProjectAway(rel, (IEnumerable<string>)columns);

        /// <summary>Returns a new sequence with <paramref name="columns"/> removed from the source <paramref name="rel"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static Relation ProjectAway(this Relation rel, IEnumerable<string> columns)
        {
            var set = new HashSet<string>(columns, Column.NameEquality);
            var toRemove = rel.Schema.Where(c => set.Contains(c.Name));
            return ProjectAway(rel, toRemove);
        }

        public static Relation ProjectAway(this Relation rel, IEnumerable<Column> columns)
        {
            var copy = rel.Schema.Except(columns).ToArray();
            var newSchema = new Schema("", copy);
            var newRows = rel.Select(r => new ProjectedTuple(newSchema, r));
            return new DerivedRelation(newSchema, newRows);
        }

        /// <summary>Adds a new calculated column to an existing <paramref name="rel"/></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rel">The base data</param>
        /// <param name="columnName">name of the new column</param>
        /// <param name="func">function to calculate the value of the new column</param>
        public static Relation Extend<T>(this Relation rel, string columnName, Func<Row, T> func)
        {
            var col = new Column(columnName, typeof(T));
            var copy = rel.Schema.Concat(Enumerable.Repeat(col, 1)).ToArray();
            var newSchema = new Schema("", copy);
            var existing = rel.Schema.columns;
            var newRows = rel.Select(r => new ExtendedTuple(newSchema, r, new ColumnValue(col, func(r))));
            return new DerivedRelation(newSchema, newRows);
        }

        public static Relation DifferenceAll(this Relation rel, Relation other)
        {
            EnsureSchemaMatches(rel, other);
            return new DerivedRelation(rel.Schema, rel.Except(other));
        }

        private static void EnsureSchemaMatches(Relation rel, Relation other)
        {
            if (rel.Schema != other.Schema)
                throw new ArgumentException($"Schemas '{rel.Schema}' and '{other.Schema}' are incompatible");
        }

        public static Relation Difference(this Relation rel, Relation other)
        {
            EnsureSchemaMatches(rel, other);
            return new DerivedRelation(rel.Schema, rel.Except(other).Distinct());
        }

        public static Relation IntersectAll(this Relation rel, Relation other)
        {
            EnsureSchemaMatches(rel, other);
            return new DerivedRelation(rel.Schema, ((IEnumerable<Row>)rel).Intersect(other));
        }

        public static Relation Intersect(this Relation rel, Relation other)
        {
            EnsureSchemaMatches(rel, other);
            return new DerivedRelation(rel.Schema, ((IEnumerable<Row>)rel).Intersect(other).Distinct());
        }

        public static Relation UnionAll(this Relation rel, Relation other)
        {
            EnsureSchemaMatches(rel, other);
            return new DerivedRelation(rel.Schema, rel.Concat(other));
        }

        public static Relation Union(this Relation rel, Relation other)
        {
            EnsureSchemaMatches(rel, other);
            return new DerivedRelation(rel.Schema, rel.Concat(other).Distinct());
        }

        /// <summary>Joins to sequences based on the value of common columns</summary>
        public static Relation NaturalJoin(this Relation rel, Relation other, Action<IEnumerable<Column>> joinObserver = null)
        {
            List<Column> joinOn = CommonColumns(rel, other);
            joinObserver?.Invoke(joinOn);
            var joinSchema = new Schema("join", joinOn);
            var otherByKeys = other.ToLookup(row => new ProjectedTuple(joinSchema, row));
            var unionSchema = new Schema($"{rel} union {other}", rel.Schema.Union(other.Schema));
            var rows = rel.SelectMany(left => otherByKeys[new ProjectedTuple(joinSchema, left)], (left, right) => new UnionedTuple(unionSchema, left, right));
            return new DerivedRelation(unionSchema, rows.Distinct());
        }

        private static List<Column> CommonColumns(Relation rel, Relation other)
        {
            var joinOn = rel.Schema.Intersect(other.Schema).ToList();
            if (joinOn.Count == 0)
                throw new ArgumentException($"Schemas '{rel.Schema}' and '{other.Schema}' do not have any common columns");
            return joinOn;
        }

        /// <summary>Returns rows from <paramref name="rel"/> where a row EXISTS in <paramref name="other"/> with matching values in common columns</summary>
        /// <remarks>select * from X where exists (select * from Y where X.colA = Y.colA and X.colB = Y.colB and ....)</remarks>
        public static Relation SemiJoin(this Relation rel, Relation other, Action<IEnumerable<Column>> joinObserver = null)
        {
            List<Column> joinOn = CommonColumns(rel, other);
            joinObserver?.Invoke(joinOn);
            var joinSchema = new Schema("join", joinOn);
            var otherKeys = new HashSet<ProjectedTuple>(other.Select(row => new ProjectedTuple(joinSchema, row)));
            var resultSchema = new Schema($"{rel.Schema} exists {other.Schema}", rel.Schema);
            var rows = rel.Where(row => otherKeys.Contains(new ProjectedTuple(joinSchema, row)));
            return new DerivedRelation(resultSchema, rows.Distinct());
        }

        public static Relation RenameAll(this Relation rel, IDictionary<string, string> changes)
        {
            var newCols = rel.Schema.Select(col => changes.ContainsKey(col.Name) ? new Column(changes[col.Name], col.Type) : col);
            var newSchema = new Schema(rel.Schema.Name, newCols);
            var reversedChanges = changes.ToDictionary(pair => pair.Value, pair => pair.Key, Column.NameEquality);
            var newRows = rel.Select(r => new RenamedTuple(r, reversedChanges));
            return new DerivedRelation(newSchema, newRows);
        }

        public static Relation Rename(this Relation rel, IDictionary<string, string> changes)
        {
            var newCols = rel.Schema.Select(col => changes.ContainsKey(col.Name) ? new Column(changes[col.Name], col.Type) : col);
            var newSchema = new Schema(rel.Schema.Name, newCols);
            var reversedChanges = changes.ToDictionary(pair => pair.Value, pair => pair.Key, Column.NameEquality);
            var newRows = rel.Select(r => new RenamedTuple(r, reversedChanges));
            return new DerivedRelation(newSchema, newRows.Distinct());
        }

        public static MaterializedRelation Materialize(this Relation rel) => new MaterializedRelation(rel.Schema, rel);

        private class ExtendedTuple : Row
        {
            readonly Row inner;
            readonly ColumnValue extra;

            public ExtendedTuple(Schema schema, Row row, ColumnValue extra) : base(schema)
            {
                this.inner = row;
                this.extra = extra;
            }

            public override object Get(string name)
            {
                if (extra.Column.NameEquals(name))
                    return extra.Value;
                return inner.Get(name);
            }

            //TODO: override other methods?
        }

        private class ProjectedTuple : Row
        {
            readonly Row inner;

            public ProjectedTuple(Schema schema, Row row) : base(schema)
            {
                this.inner = row;
            }

            public override object Get(string name)
            {
                Schema.ThrowWhenUnknownColumn(name);
                return inner.Get(name);
            }

            //TODO: override other methods?
        }

        private class UnionedTuple : Row
        {
            readonly Row left;
            readonly Row right;

            public UnionedTuple(Schema schema, Row left, Row right) : base(schema)
            {
                this.left = left;
                this.right = right;
            }

            public override object Get(string name) => left.Schema.Contains(name) ? left.Get(name) : right.Get(name);

            //TODO: override other methods?
        }

        private class RenamedTuple : Row
        {
            private IDictionary<string, string> changes;
            private Row inner;

            public RenamedTuple(Row r, IDictionary<string, string> changes) : base(r.Schema)
            {
                this.inner = r;
                this.changes = changes;
            }

            public override object Get(string name)
            {
                string oldName;
                return changes.TryGetValue(name, out oldName) ? inner.Get(oldName) : inner.Get(name);
            }
        }
    }
}