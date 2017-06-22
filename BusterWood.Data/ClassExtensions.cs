using System;
using System.Collections;
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


        class ExpressionRow<T> : Row
        {
#pragma warning disable RECS0108 // Warns about static fields in generic types
            static readonly Dictionary<string, Func<T, object>> objByName = ReadObjs(ReadableMembers(typeof(T)).ToDictionary(m => m.Name, Column.NameEquality));
            static readonly SmallMap<string, Func<T, string>> stringByName = MembersOfType<string>();
            static readonly SmallMap<string, Func<T, int>> intByName = MembersOfType<int>();
            static readonly SmallMap<string, Func<T, DateTime>> dateTimeByName = MembersOfType<DateTime>();

            private static SmallMap<string, Func<T, TResult>> MembersOfType<TResult>()
            {
                return Read<TResult>(ReadableMembers(typeof(T)).Where(m => MemberType(m) == typeof(TResult)).ToSmallMap(m => m.Name, Column.NameEquality));
            }

            static Dictionary<string, Func<T, object>> ReadObjs(Dictionary<string, MemberInfo> members)
            {
                return members.Select(kv => new { kv.Key, Func = ReadObj(kv.Value) }).ToDictionary(x => x.Key, x => x.Func, Column.NameEquality);
            }

            static SmallMap<string, Func<T, TResult>> Read<TResult>(SmallMap<string, MemberInfo> members)
            {
                return members.Select(kv => new { kv.Key, Func = Read<TResult>(kv.Value) }).ToSmallMap(x => x.Key, x => x.Func, Column.NameEquality);
            }

            static Func<T, object> ReadObj(MemberInfo member)
            {
                var item = Expression.Parameter(typeof(T));
                var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.PropertyOrField(item, member.Name), typeof(object)), item);
                return lambda.Compile();
            }

            static Func<T, TResult> Read<TResult>(MemberInfo member)
            {
                var item = Expression.Parameter(typeof(T));
                var lambda = Expression.Lambda<Func<T, TResult>>(Expression.PropertyOrField(item, member.Name), item);
                return lambda.Compile();
            }

            readonly T item;

            public ExpressionRow(Schema schema, T item) : base(schema)
            {
                this.item = item;
            }

            public override object Get(string name) => objByName[name](item);
            public override string String(string name) => stringByName[name](item);
            public override int Int(string name) => intByName[name](item);
            public override DateTime DateTime(string name) => dateTimeByName[name](item);

        }

    }

    static partial class Extensions
    {
        internal static SmallMap<TKey, TValue> ToSmallMap<TKey, TValue>(this IEnumerable<TValue> items, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            var values = items.ToArray();
            var keys = values.Select(keySelector).Distinct(keyComparer).ToArray();
            return new SmallMap<TKey, TValue>(keys, values, keyComparer);
        }

        internal static SmallMap<TKey, TValue> ToSmallMap<T, TKey, TValue>(this IEnumerable<T> items, Func<T, TKey> keySelector, Func<T, TValue> valueSelector, IEqualityComparer<TKey> keyComparer)
        {
            var values = items.Select(valueSelector).ToArray();
            var keys = items.Select(keySelector).Distinct(keyComparer).ToArray();
            return new SmallMap<TKey, TValue>(keys, values, keyComparer);
        }
    }

    struct SmallMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        TKey[] keys;
        TValue[] values;
        IEqualityComparer<TKey> keyComparer;

        public SmallMap(TKey[] keys, TValue[] values, IEqualityComparer<TKey> keyComparer)
        {
            if (keys.Length != values.Length) throw new ArgumentException("Duplicate keys");
            this.keys = keys;
            this.values = values;
            this.keyComparer = keyComparer;
        }

        public IEqualityComparer<TKey> KeyComparer => keyComparer ?? EqualityComparer<TKey>.Default;

        public int Count => values?.Length ?? 0;

        public TValue this[TKey key] 
        {
            get
            {
                int idx = IndexOf(key);
                return idx < 0 ? default(TValue) : values[idx];
            }
        }

        int IndexOf(TKey key)
        {
            int i = 0;
            var eq = KeyComparer;
            foreach (var k in keys)
            {
                if (eq.Equals(k, key))
                    return i;
                i++;
            }
            return -1;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            int i = 0;
            foreach (var k in keys)
                yield return new KeyValuePair<TKey, TValue>(k, values[i++]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
