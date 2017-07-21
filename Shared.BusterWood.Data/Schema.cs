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
    public interface ISchemaed
    {
        Schema Schema { get; }            
    }

    /// <summary>The metadata for a <see cref="Relation"/> of <see cref="Row"/></summary>
    /// <remarks>This type is immuatable and cannot be changed (mutated)</remarks>
    public class Schema : IReadOnlyCollection<Column>, IEquatable<Schema>
    {
        readonly Column[] columns;
        readonly int hashCode;

        public Schema(string name, IEnumerable<Column> columns) : this(name, columns?.ToArray())
        {
        }

        public Schema(string name, params Column[] columns)
        {
            Name = name;
            this.columns = columns ?? throw new ArgumentNullException(nameof(columns));
            CheckForDuplicateColumns(columns);
            hashCode = columns.Aggregate(0, (hc, c) => { unchecked { return hc + c.GetHashCode(); } });
        }

        static void CheckForDuplicateColumns(Column[] columns)
        {
            var temp = new HashSet<string>(Column.NameEquality);
            foreach (var c in columns)
            {
                if (!temp.Add(c.Name))
                    throw new ArgumentException($"Schema must have unqiue columns: {c} is duplicated");
            }
        }

        /// <summary>The name of this schema (optional)</summary>
        public string Name { get; }

        public int Count => columns?.Length ?? 0;

        public bool Contains(string name)
        {
            var eq = Column.NameEquality;
            foreach (var c in columns)
            {
                if (eq.Equals(c.Name, name))
                    return true;
            }
            return false;
        }

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

        public override string ToString() => Name;

        //public static Schema Merge(Schema left, Schema right) => 
        //    new Schema($"Merge of {left.Name} and {right.Name}", left.columns.AddRange(right.Where(r => !left.columns.ContainsKey(r.Name)).Select(c => new KeyValuePair<string, Column>(c.Name, c))));

        internal void ThrowWhenUnknownColumn(string name)
        {
            if (columns?.Any(c => c.NameEquals(name)) != true)
                throw new UnknownColumnException($"Unknown column {name} in schema '{Name}'");
        }
    }


}
