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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
    /// <summary>A sequenece of UNIQUE rows which all have the same <see cref="Schema"/></summary>
    /// <remarks>See <see cref="CsvReaderExtensions.CsvToRelation(System.IO.TextReader, string, char)"/> and <see cref="DataReaderExtensions.ToRelation(System.Data.IDataReader, string)"/></remarks>
    public abstract class Relation : IEnumerable<Row>, ISchemaed
    {
        protected Relation(Schema schema)
        {
            Schema = schema;
        }

        /// <summary>The schema that applies to all rows in this sequence</summary>
        public Schema Schema { get; }

        /// <summary>Returns a sequence of zero or more <see cref="Row"/></summary>
        protected abstract IEnumerable<Row> GetSequence();

        public IEnumerator<Row> GetEnumerator() => GetSequence().Distinct().GetEnumerator(); // all rows must be unique

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }





}
