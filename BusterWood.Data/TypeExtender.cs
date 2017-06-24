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
            var shc = HasSchemaBuilder.DefineSchemaHash(type, schemaHashCode);

            TypeBuilder enumTypeBuilder = EnumeratorBuilder.CreateEnumeratorType(module, type);
            var enumeratorCtor = EnumeratorBuilder.DefineEnumerator(enumTypeBuilder, type, props);
            var enm = HasSchemaBuilder.DefineGetEnumerator(type, enumeratorCtor);
            HasSchemaBuilder.DefineGetGenericEnumerator(type, enumeratorCtor);

            var equals = HasSchemaBuilder.DefineEqualsHasSchema(type, props, shc);
            HasSchemaBuilder.DefineEqualsObject(type, equals);
            HasSchemaBuilder.DefineGetHashCode(type, shc, props);

            var dynType = type.CreateTypeInfo().AsType();
            var enumType = enumTypeBuilder.CreateTypeInfo().AsType();
#if DEBUG
            assemblyBuilder.Save(assemblyName + ".dll");
#endif
            return dynType;
        }

        static ConstructorBuilder DefineConstructor(TypeBuilder typeBuilder, Type from, FieldBuilder innerFld, Column[] extra, List<FieldBuilder> extraFlds)
        {
            var ctor = typeBuilder.DefineConstructor(Public, HasThis, new[] { from }.Concat(extra.Select(e => e.Type)).ToArray());
            var il = ctor.GetILGenerator();
            il.This().Call(typeof(object).GetConstructor(EmptyTypes)); // call object ctor - this is the return value

            il.This().Arg1().Store(innerFld); // store the parameter in the inner field

            int arg = 2;
            foreach (var e in extraFlds)
                il.This().Arg(arg++).Store(e); // store the parameter in the field

            il.Return();   // end of ctor
            return ctor;
        }

        static PropertyBuilder DefineProperty(TypeBuilder builder, FieldBuilder extraFld, Column col)
        {
            var prop = builder.DefineProperty(col.Name, PropertyAttributes.HasDefault, col.Type, null);
            var getMethod = builder.DefineMethod("get_" + col.Name, Public | SpecialName | HideBySig, col.Type, Type.EmptyTypes);
            var il = getMethod.GetILGenerator();
            il.This().Load(extraFld);
            il.Return();
            prop.SetGetMethod(getMethod);
            return prop;
        }

        static PropertyBuilder DefineDelegatingProperty(TypeBuilder builder, FieldBuilder innerFld, PropertyInfo p)
        {
            var prop = builder.DefineProperty(p.Name, PropertyAttributes.HasDefault, p.PropertyType, null);
            var getMethod = builder.DefineMethod("get_" + p.Name, Public | SpecialName | HideBySig, p.PropertyType, Type.EmptyTypes);
            var il = getMethod.GetILGenerator();
            il.This().Load(innerFld).CallVirt(p.GetGetMethod());
            il.Return();
            prop.SetGetMethod(getMethod);
            return prop;
        }
    }
}
