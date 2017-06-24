using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.MethodAttributes;
using static System.Reflection.CallingConventions;
using System.Collections;

namespace BusterWood.Data
{
    public static class HasSchemaBuilder
    {
        public static MethodBuilder DefineSchemaHash(TypeBuilder builder, int schemaHashCode)
        {
            var method = builder.DefineMethod("SchemaHashCode", Public | Virtual | Final, HasThis, typeof (int), Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.Constant(schemaHashCode);
            il.Return();
            return method;
        }

        public static MethodBuilder DefineGetEnumerator(TypeBuilder builder, ConstructorBuilder enumeratorCtor)
        {
            var method = builder.DefineMethod("IEnumerable.GetEnumerator", Private | Virtual | Final | HideBySig, HasThis, typeof (IEnumerator), Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.This().New(enumeratorCtor);
            il.Return();
            builder.DefineMethodOverride(method, typeof (IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator)));
            return method;
        }

        public static void DefineGetGenericEnumerator(TypeBuilder builder, ConstructorBuilder enumeratorCtor)
        {
            var method = builder.DefineMethod(nameof(IEnumerable.GetEnumerator), Public | Virtual | Final | HideBySig, HasThis, typeof (IEnumerator<ColumnValue>), Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.This().New(enumeratorCtor);
            il.Return();
        }

        public static MethodBuilder DefineEqualsObject(TypeBuilder builder, MethodBuilder equalsHasSchema)
        {
            var method = builder.DefineMethod("Equals", Public | Virtual | HideBySig, HasThis, typeof (bool), new[]{typeof (object)});
            var il = method.GetILGenerator();
            il.This();
            il.Arg1().AsType(typeof (IHasSchema));
            il.CallVirt(equalsHasSchema);
            il.Return();
            builder.DefineMethodOverride(method, typeof (object).GetMethod(nameof(object.Equals), new[]{typeof (object)}));
            return method;
        }

        public static MethodBuilder DefineEqualsHasSchema(TypeBuilder builder, List<PropertyBuilder> props, MethodBuilder schemaHasCode)
        {
            var method = builder.DefineMethod("Equals", Public | Virtual | Final | HideBySig, HasThis, typeof (bool), new[]{typeof (IHasSchema)});
            var il = method.GetILGenerator();
            var em = il.DeclareLocal<IEnumerator<ColumnValue>>();
            var cv = il.DeclareLocal<ColumnValue>();
            var notEqual = il.DefineLabel();
            CheckOtherNotNull(il, notEqual);
            CheckSchemaHash(schemaHasCode, il, notEqual);
            GetOtherEnumerator(il, em);
            foreach (var p in props.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
            {
                MoveNext(il, em, notEqual);
                IsPropertyEqual(il, em, notEqual, p, cv);
            }

            il.Return(true);
            il.MarkLabel(notEqual);
            il.Return(false);
            return method;
        }

        private static void CheckOtherNotNull(ILGenerator il, Label notEqual)
        {
            // if (other == null) return false;
            il.Arg1();
            il.BranchIfFalse(notEqual);
        }

        private static void CheckSchemaHash(MethodBuilder schemaHasCode, ILGenerator il, Label notEqual)
        {
            // if (other.SchemaHashCode() != SchemaHashCode()) return false;
            il.This().Call(schemaHasCode);
            il.Arg1().CallVirt<IHasSchema>(nameof(IHasSchema.SchemaHashCode));
            il.BranchIfNotEqual(notEqual);
        }

        private static void GetOtherEnumerator(ILGenerator il, LocalBuilder em)
        {
            // em = other.GetEnumerator();
            il.Arg1().CallVirt<IEnumerable<ColumnValue>>("GetEnumerator");
            il.Store(em);
        }

        private static void MoveNext(ILGenerator il, LocalBuilder em, Label notEqual)
        {
            // if (!e.MoveNext()) return false;
            il.Load(em).CallVirt<IEnumerator>("MoveNext");
            il.BranchIfFalse(notEqual);
        }

        private static void IsPropertyEqual(ILGenerator il, LocalBuilder em, Label notEqual, PropertyBuilder p, LocalBuilder cv)
        {
            //if (!Equals(Id, e.Current.Value)) return false;
            il.This().Call(p.GetMethod);
            if (p.PropertyType.IsValueType)
                il.Box(p.PropertyType);
            il.Load(em).GetPropertyValue<IEnumerator<ColumnValue>>("Current");
            il.Store(cv); // store column value
            il.LoadAddress(cv); // push reference to column value
            il.Call(typeof (ColumnValue).GetProperty("Value").GetMethod);
            il.Call(typeof (object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public));
            il.BranchIfFalse(notEqual);
        }

        internal static void DefineGetHashCode(TypeBuilder type, MethodBuilder schemaHashCode, IEnumerable<PropertyInfo> props)
        {
            var method = type.DefineMethod("GetHashCode", Public | Virtual | Final | HideBySig, HasThis, typeof (int), null);
            var il = method.GetILGenerator();
            var locals = props.Select(p => p.PropertyType.IsValueType ? il.DeclareLocal(p.PropertyType) : null).ToList();
            il.This().Call(schemaHashCode);
            int i = 0;
            foreach (var p in props)
            {
                il.This().CallGetProperty(p);
                if (locals[i] != null)
                {
                    il.Store(locals[i]);
                    il.LoadAddress(locals[i]);
                    il.Call(p.PropertyType.GetMethod("GetHashCode"));
                }
                else
                    il.CallVirt(p.PropertyType.GetMethod("GetHashCode"));
                il.Emit(OpCodes.Add);
                i++;
            }

            il.Return();
            type.DefineMethodOverride(method, typeof (object).GetMethod("GetHashCode"));
        }
    }
}