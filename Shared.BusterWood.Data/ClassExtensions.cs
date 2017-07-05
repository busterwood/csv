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
using System.Linq.Expressions;
using System.Reflection;

namespace BusterWood.Data
{
    public static class Objects
    {
        public static DataSequence ToDataSequence<T>(this IEnumerable<T> items, string name = null)
        {
            var schema = ToSchema<T>(name);
            return new ObjectSequence<T>(schema, items);
        }
    
        public static DataSequence ToDataSequence<T>(T item, string name = null)
        {
            var schema = ToSchema<T>(name);
            return new SingleObjectSequence<T>(schema, item);
        }

        public static Schema ToSchema<T>(string name = null) => ToSchema(typeof(T), name);

        public static Schema ToSchema(this Type type, string name = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type.IsPrimitive) throw new ArgumentException($"Cannot get the schema for a primative type: {type.Name}");
            var cols = ReadableMembers(type).Select(m => new Column(m.Name, MemberType(m)));
            return new Schema(name ?? type.Name, cols);
        }

        internal static MemberInfo[] ReadableMembers(Type type) => type.GetProperties().Where(p => p.CanRead).Concat<MemberInfo>(type.GetFields()).ToArray();

        static Type MemberType(MemberInfo m) => m is PropertyInfo ? ((PropertyInfo)m).PropertyType : ((FieldInfo)m).FieldType;

        class ObjectSequence<T> : DataSequence
        {
            readonly IEnumerable<T> items;

            public ObjectSequence(Schema schema, IEnumerable<T> items) : base(schema)
            {
                if (items == null) throw new ArgumentNullException(nameof(items));
                this.items = items;
            }

            protected override IEnumerable<Row> GetSequence() => items.Select(item => new ExpressionRow<T>(Schema, item));
        }

        class SingleObjectSequence<T> : DataSequence
        {
            readonly T item;

            public SingleObjectSequence(Schema schema, T item) : base(schema)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));
                this.item = item;
            }

            protected override IEnumerable<Row> GetSequence()
            {
                if ((typeof(T).IsClass || IsNullableValueType()) && item != null)
                    yield return new ExpressionRow<T>(Schema, item);
            }

            private static bool IsNullableValueType() => typeof(T).IsValueType && typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        class ReflectionRow<T> : Row
        {
#pragma warning disable RECS0108 // Warns about static fields in generic types
            static readonly IReadOnlyDictionary<string, MemberInfo> membersByName = ReadableMembers(typeof(T)).ToDictionary(m => m.Name, Column.NameEquality);
            readonly object item;

            public ReflectionRow(Schema schema, object item) : base(schema) // force boxing of structs by requiring object
            {
                this.item = item;
            }

            public override object Get(string name) => GetValue(membersByName[name]);

            object GetValue(MemberInfo m) => m is PropertyInfo ? ((PropertyInfo)m).GetValue(item) : ((FieldInfo)m).GetValue(item);
        }


        class ExpressionRow<T> : Row
        {
#pragma warning disable RECS0108 // Warns about static fields in generic types
            static readonly IReadOnlyDictionary<string, Func<T, object>> funcsByName = BuildFunctions(ReadableMembers(typeof(T)).ToDictionary(m => m.Name, Column.NameEquality));

            static IReadOnlyDictionary<string, Func<T, object>> BuildFunctions(IReadOnlyDictionary<string, MemberInfo> members)
            {
                return members.Select(kv => new { kv.Key, Func = BuildFunction(kv.Value) }).ToDictionary(x => x.Key, x => x.Func, Column.NameEquality);
            }

            static Func<T, object> BuildFunction(MemberInfo member)
            {
                var itemParam = Expression.Parameter(typeof(T));
                var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.PropertyOrField(itemParam, member.Name), typeof(object)), itemParam);
                return lambda.Compile();
            }

            readonly T item;

            public ExpressionRow(Schema schema, T item) : base(schema)
            {
                this.item = item;
            }

            public override object Get(string name)
            {
                try
                {
                    return funcsByName[name](item);
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException($"Row does not contain column '{name}'");
                }
            }
        }

    }
}
