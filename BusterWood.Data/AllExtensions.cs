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
    /// <summary>
    /// Not strictly relational operators are they do not remove duplicate rows from the result relation
    /// </summary>
    public static class AllExtensions
    {
        public static Relation Distinct(this Relation rel) => new DerivedRelation(rel.Schema, ((IEnumerable<Row>)rel).Distinct());

        public static Relation Distinct(this Relation rel, bool enabled) => enabled ? Distinct(rel) : rel;

        /// <summary>Filter the source releation, e.g. a "Where" clause</summary>
        public static Relation RestrictAll(this Relation rel, Func<Row, bool> predicate) => new DerivedRelation(rel.Schema, Enumerable.Where(rel, predicate));

        /// <summary>Filter the source releation, e.g. a "Where" clause</summary>
        public static Relation RestrictAwayAll(this Relation rel, Func<Row, bool> predicate) => new DerivedRelation(rel.Schema, Enumerable.Where(rel, row => !predicate(row)));

        /// <summary>Returns a new sequence with that only contains the requested <paramref name="columns"/> from the source <paramref name="rel"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static Relation ProjectAll(this Relation rel, IEnumerable<string> columns)
        {
            var cols = columns.Select(c => rel.Schema[c]);
            var newSchema = new Schema("", cols);
            var newRows = rel.Select(r => new ProjectedTuple(newSchema, r));
            return new DerivedRelation(newSchema, newRows);
        }

        public static Relation DifferenceAll(this Relation rel, Relation other)
        {
            rel.EnsureSchemaMatches(other);
            return new DerivedRelation(rel.Schema, rel.Except(other));
        }

        public static Relation IntersectAll(this Relation rel, Relation other)
        {
            rel.EnsureSchemaMatches(other);
            return new DerivedRelation(rel.Schema, ((IEnumerable<Row>)rel).Intersect(other));
        }

        public static Relation UnionAll(this Relation rel, Relation other)
        {
            rel.EnsureSchemaMatches(other);
            return new DerivedRelation(rel.Schema, rel.Concat(other));
        }

        public static Relation RenameAll(this Relation rel, IDictionary<string, string> changes)
        {
            var newCols = rel.Schema.Select(col => changes.ContainsKey(col.Name) ? new Column(changes[col.Name], col.Type) : col);
            var newSchema = new Schema(rel.Schema.Name, newCols);
            var reversedChanges = changes.ToDictionary(pair => pair.Value, pair => pair.Key, Column.NameEquality);
            var newRows = rel.Select(r => new RenamedTuple(r, reversedChanges));
            return new DerivedRelation(newSchema, newRows);
        }

    }
}
