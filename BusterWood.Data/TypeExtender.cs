using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Reflection.MethodAttributes;
using static System.Reflection.CallingConventions;

namespace BusterWood.Data
{
    public interface IExtenderFactory<T>
    {
        object Create(object inner, T extra);
    }

    public static class TypeExtender
    {
        static readonly Type[] EmptyTypes = new Type[0];
        static int id = 0;

        public static Type Extend(Type from, params Column[] extra)
        {
            string assemblyName = "Extension" + Interlocked.Increment(ref id);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            TypeBuilder builder = moduleBuilder.DefineType($"{assemblyName}.{from.Name}With{extra.Length}", TypeAttributes.Public | TypeAttributes.Sealed);

            var innerFld = builder.DefineField("_inner", from, FieldAttributes.Private | FieldAttributes.InitOnly);
            var extraFlds = extra.Select(e => builder.DefineField("_" + e.Name, e.Type, FieldAttributes.Private | FieldAttributes.InitOnly)).ToList();

            var ctor = DefineConstructor(builder, from, innerFld, extra, extraFlds);

            int schemaHashCode = 1;

            foreach (var p in from.GetProperties().Where(p => p.CanRead))
            {
                DefineDelegatingProperty(builder, innerFld, p);
                schemaHashCode *= Column.NameEquality.GetHashCode(p.Name);
            }

            int i = 0;
            foreach (var e in extra)
            {
                DefineProperty(builder, extraFlds[i++], e);
                schemaHashCode *= Column.NameEquality.GetHashCode(e.Name);
            }

            DefineSchemaHash(builder, schemaHashCode);
            var dynType = builder.CreateTypeInfo().AsType();
            assemblyBuilder.Save(assemblyName + ".dll");
            return dynType;
        }

        static MethodBuilder DefineSchemaHash(TypeBuilder builder, int schemaHashCode)
        {
            var getMethod = builder.DefineMethod("SchemaHashCode", Public, typeof(int), Type.EmptyTypes);
            var il = getMethod.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4, schemaHashCode);
            il.Emit(OpCodes.Ret);
            return getMethod;
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
                il.Emit(OpCodes.Stfld, e); // store the parameter in the inner field
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

    }
}
