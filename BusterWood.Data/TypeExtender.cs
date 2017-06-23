using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using static System.Reflection.MethodAttributes;
using static System.Reflection.CallingConventions;
using System.Collections;

namespace BusterWood.Data
{
    public interface IExtenderFactory<T>
    {
        IHasSchema Create(object inner, T extra);
    }

    public interface IHasSchema : IEnumerable<ColumnValue>, IEquatable<IHasSchema>
    {
        int SchemaHashCode();
        new bool Equals(IHasSchema other);
    }

    public static class TypeExtender
    {
        static readonly Type[] EmptyTypes = new Type[0];
        static int id = 0;

        public static Type Extend(Type from, params Column[] extra)
        {
            string assemblyName = "Extension" + Interlocked.Increment(ref id);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndSave);
            var module = assemblyBuilder.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            TypeBuilder type = module.DefineType($"{assemblyName}.{from.Name}With{extra.Length}", TypeAttributes.Public | TypeAttributes.Sealed);

            type.AddInterfaceImplementation(typeof(IEnumerable<ColumnValue>));
            type.AddInterfaceImplementation(typeof(IEnumerable));
            type.AddInterfaceImplementation(typeof(IHasSchema));

            var innerFld = type.DefineField("_inner", from, FieldAttributes.Private | FieldAttributes.InitOnly);
            var extraFlds = extra.Select(e => type.DefineField("_" + e.Name, e.Type, FieldAttributes.Private | FieldAttributes.InitOnly)).ToList();

            var ctor = DefineConstructor(type, from, innerFld, extra, extraFlds);

            var readableProperties = from.GetProperties().Where(p => p.CanRead);

            var props = readableProperties.Select(p => DefineDelegatingProperty(type, innerFld, p))
                .Concat(extra.Select((e, i) => DefineProperty(type, extraFlds[i], e)))
                .ToList();

            var allColumns = readableProperties.Select(p => new Column(p.Name, p.PropertyType)).Concat(extra);

            int schemaHashCode = allColumns.Aggregate(1, (hc, col) => { unchecked { return hc * col.GetHashCode(); } });
            var shc = DefineSchemaHash(type, schemaHashCode);

            TypeBuilder enumTypeBuilder = EnumeratorBuilder.CreateEnumeratorType(module, type);
            var enumeratorCtor = EnumeratorBuilder.DefineEnumerator(enumTypeBuilder, type, props);
            var enm = DefineGetEnumerator(type, enumeratorCtor);
            DefineGetGenericEnumerator(type, enumeratorCtor);

            DefineEqualsHasSchema(type, props, shc);

            var dynType = type.CreateTypeInfo().AsType();
            var enumType = enumTypeBuilder.CreateTypeInfo().AsType();
#if DEBUG
            assemblyBuilder.Save(assemblyName + ".dll");
#endif
            return dynType;
        }

        static MethodBuilder DefineSchemaHash(TypeBuilder builder, int schemaHashCode)
        {
            var method = builder.DefineMethod("SchemaHashCode", Public|Virtual|Final, HasThis, typeof(int), Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4, schemaHashCode);
            il.Emit(OpCodes.Ret);
            return method;
        }

        static ConstructorBuilder DefineConstructor(TypeBuilder typeBuilder, Type from, FieldBuilder innerFld, Column[] extra, List<FieldBuilder> extraFlds)
        {
            var ctor = typeBuilder.DefineConstructor(Public, HasThis, new[] { from }.Concat(extra.Select(e => e.Type)).ToArray());
            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Call, typeof(object).GetConstructor(EmptyTypes)); // call object ctor

            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldarg_1); // push inner
            il.Emit(OpCodes.Stfld, innerFld); // store the parameter in the inner field

            int arg = 2;
            foreach (var e in extraFlds)
            {
                il.Emit(OpCodes.Ldarg_0); // push this
                il.Emit(OpCodes.Ldarg, arg++); // push extra
                il.Emit(OpCodes.Stfld, e); // store the parameter in the field
            }

            il.Emit(OpCodes.Ret);   // end of ctor
            return ctor;
        }

        static PropertyBuilder DefineProperty(TypeBuilder builder, FieldBuilder extraFld, Column col)
        {
            var prop = builder.DefineProperty(col.Name, PropertyAttributes.HasDefault, col.Type, null);
            var getMethod = builder.DefineMethod("get_" + col.Name, Public | SpecialName | HideBySig, col.Type, Type.EmptyTypes);
            var il = getMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldfld, extraFld);
            il.Emit(OpCodes.Ret);
            prop.SetGetMethod(getMethod);
            return prop;
        }

        static PropertyBuilder DefineDelegatingProperty(TypeBuilder builder, FieldBuilder innerFld, PropertyInfo p)
        {
            var prop = builder.DefineProperty(p.Name, PropertyAttributes.HasDefault, p.PropertyType, null);
            var getMethod = builder.DefineMethod("get_" + p.Name, Public|SpecialName|HideBySig, p.PropertyType, Type.EmptyTypes);
            var il = getMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldfld, innerFld);
            il.Emit(OpCodes.Callvirt, p.GetGetMethod());
            il.Emit(OpCodes.Ret);
            prop.SetGetMethod(getMethod);
            return prop;
        }

        static MethodBuilder DefineGetEnumerator(TypeBuilder builder, ConstructorBuilder enumeratorCtor)
        {
            var method = builder.DefineMethod("IEnumerable.GetEnumerator", Private | Virtual | Final | HideBySig, HasThis, typeof(IEnumerator), Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, enumeratorCtor);
            il.Emit(OpCodes.Ret);
            builder.DefineMethodOverride(method, typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator)));
            return method;
        }

        static void DefineGetGenericEnumerator(TypeBuilder builder, ConstructorBuilder enumeratorCtor)
        {
            var method = builder.DefineMethod(nameof(IEnumerable.GetEnumerator), Public | Virtual | Final | HideBySig, HasThis, typeof(IEnumerator<ColumnValue>), Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, enumeratorCtor);
            il.Emit(OpCodes.Ret);
        }

        private static void DefineEqualsHasSchema(TypeBuilder builder, List<PropertyBuilder> props, MethodBuilder schemaHasCode)
        {
            var method = builder.DefineMethod("Equals", Public | Virtual | Final | HideBySig, HasThis, typeof(bool), new[] { typeof(IHasSchema) });
            var il = method.GetILGenerator();
            var em = il.DeclareLocal(typeof(IEnumerator<ColumnValue>));
            var cv = il.DeclareLocal(typeof(ColumnValue));
            var notEqual = il.DefineLabel();

            CheckOtherNotNull(il, notEqual);
            CheckSchemaHash(schemaHasCode, il, notEqual);
            GetOtherEnumerator(il, em);

            foreach (var p in props.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
            {
                MoveNext(il, em, notEqual);
                IsPropertyEqual(il, em, notEqual, p, cv);
            }
            
            // return true
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);

            ReturnFalse(il, notEqual);
        }

        private static void CheckOtherNotNull(ILGenerator il, Label notEqual)
        {
            // if (other == null) return false;
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Brfalse, notEqual);
        }

        private static void CheckSchemaHash(MethodBuilder schemaHasCode, ILGenerator il, Label notEqual)
        {
            // if (other.SchemaHashCode() != SchemaHashCode()) return false;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, schemaHasCode);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, typeof(IHasSchema).GetMethod(nameof(IHasSchema.SchemaHashCode)));
            il.Emit(OpCodes.Bne_Un, notEqual);
        }

        private static void GetOtherEnumerator(ILGenerator il, LocalBuilder em)
        {
            // em = other.GetEnumerator();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerable<ColumnValue>).GetMethod(nameof(IEnumerable<ColumnValue>.GetEnumerator)));
            il.Emit(OpCodes.Stloc, em);
        }

        private static void MoveNext(ILGenerator il, LocalBuilder em, Label notEqual)
        {
            // if (!e.MoveNext()) return false;
            il.Emit(OpCodes.Ldloc, em);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
            il.Emit(OpCodes.Brfalse, notEqual);
        }

        private static void IsPropertyEqual(ILGenerator il, LocalBuilder em, Label notEqual, PropertyBuilder p, LocalBuilder cv)
        {
            //if (!Equals(Id, e.Current.Value)) return false;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, p.GetMethod);
            if (p.PropertyType.IsValueType)
                il.Emit(OpCodes.Box, p.PropertyType);
            il.Emit(OpCodes.Ldloc, em);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerator<ColumnValue>).GetProperty("Current").GetMethod);
            il.Emit(OpCodes.Stloc, cv); // store column value
            il.Emit(OpCodes.Ldloca, cv); // push reference to column value
            il.Emit(OpCodes.Call, typeof(ColumnValue).GetProperty("Value").GetMethod);
            il.Emit(OpCodes.Call, typeof(object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public));
            il.Emit(OpCodes.Brfalse, notEqual);
        }

        private static void ReturnFalse(ILGenerator il, Label notEqual)
        {
            // return false
            il.MarkLabel(notEqual);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
        }
    }
}
