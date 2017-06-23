using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using static System.Reflection.MethodAttributes;
using static System.Reflection.CallingConventions;

namespace BusterWood.Data
{
    public static class TypeRestrictor
    {
        static readonly Type[] EmptyTypes = new Type[0];
        static int id = 0;

        public static Type Restrict(Type from, string[] properties)
        {
            string assemblyName = "Restriction" + Interlocked.Increment(ref id);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            TypeBuilder builder = moduleBuilder.DefineType($"{assemblyName}.{from.Name}", TypeAttributes.Public | TypeAttributes.Sealed);

            var innerFld = builder.DefineField("_inner", from, FieldAttributes.Private | FieldAttributes.InitOnly);

            var ctor = DefineConstructor(builder, from, innerFld);

            var only = new HashSet<string>(properties, Column.NameEquality);
            foreach (var p in from.GetProperties().Where(p => p.CanRead && only.Contains(p.Name)))
            {
                DefineDelegatingProperty(builder, innerFld, p);
            }

            var dynType = builder.CreateTypeInfo().AsType();
            //assemblyBuilder.Save(assemblyName + ".dll");
            return dynType;
        }

        static ConstructorBuilder DefineConstructor(TypeBuilder typeBuilder, Type from, FieldBuilder innerFld)
        {
            var ctor = typeBuilder.DefineConstructor(Public, HasThis, new[] { from });
            var il = ctor.GetILGenerator();
            il.This(); // push this
            il.Call(typeof(object).GetConstructor(EmptyTypes));  // call object ctor
            il.This(); // push this
            il.Arg1().Store(innerFld); // store the parameter in the inner field
            il.Return();
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
            var getMethod = builder.DefineMethod("get_" + p.Name, Public|SpecialName|HideBySig, p.PropertyType, Type.EmptyTypes);
            var il = getMethod.GetILGenerator();
            il.This().Load(innerFld).CallVirt(p.GetGetMethod());
            il.Return();
            prop.SetGetMethod(getMethod);
            return prop;
        }

    }
}
