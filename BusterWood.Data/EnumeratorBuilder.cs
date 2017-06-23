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
    static class EnumeratorBuilder
    {
        static readonly Type[] EmptyTypes = new Type[0];

        public static TypeBuilder CreateEnumeratorType(ModuleBuilder module, TypeBuilder type)
        {
            return module.DefineType(type.Name + "Enumerator", TypeAttributes.NotPublic | TypeAttributes.Sealed);
        }

        public static ConstructorBuilder DefineEnumerator(TypeBuilder typeBuilder, TypeBuilder type, IEnumerable<PropertyInfo> columns)
        {
            typeBuilder.AddInterfaceImplementation(typeof(IEnumerator));
            typeBuilder.AddInterfaceImplementation(typeof(IEnumerator<ColumnValue>));
            typeBuilder.AddInterfaceImplementation(typeof(IDisposable));

            var obj = typeBuilder.DefineField("_obj", type, FieldAttributes.InitOnly | FieldAttributes.Private);
            var state = typeBuilder.DefineField("_state", typeof(int), FieldAttributes.Private);
            var current = typeBuilder.DefineField("_current", typeof(ColumnValue), FieldAttributes.Private);

            var ctor = DefineCtor(typeBuilder, obj);
            DefineCurrent(typeBuilder, current);
            DefineMoveNext(typeBuilder, obj, state, current, columns);
            DefineDispose(typeBuilder);
            DefineReset(typeBuilder, state);
            DefineEnumeratorCurrent(typeBuilder, current);
            return ctor;
        }

        static ConstructorBuilder DefineCtor(TypeBuilder typeBuilder, FieldBuilder obj)
        {
            var ctor = typeBuilder.DefineConstructor(Public, HasThis, new[] { obj.FieldType });
            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Call, typeof(object).GetConstructor(EmptyTypes)); // call object ctor

            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldarg_1); // push _obj
            il.Emit(OpCodes.Stfld, obj); // store the parameter in the inner field
            il.Emit(OpCodes.Ret);
            return ctor;
        }

        static void DefineCurrent(TypeBuilder typeBuilder, FieldBuilder current)
        {
            var prop = typeBuilder.DefineProperty("Current", PropertyAttributes.HasDefault, typeof(ColumnValue), null);
            var getMethod = typeBuilder.DefineMethod("get_Current", Public | Virtual | Final | SpecialName | HideBySig, HasThis, typeof(ColumnValue), Type.EmptyTypes);
            var il = getMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldfld, current);
            il.Emit(OpCodes.Ret);
            prop.SetGetMethod(getMethod);
        }

        static void DefineMoveNext(TypeBuilder typeBuilder, FieldBuilder obj, FieldBuilder state, FieldBuilder current, IEnumerable<PropertyInfo> allColumns)
        {
            // sort keys by name for equality can use Enumerable.SequenceEquals()
            var columns = allColumns.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToArray();

            var method = typeBuilder.DefineMethod(nameof(IEnumerator.MoveNext), Public|Virtual|Final, HasThis, typeof(bool), EmptyTypes);
            var il = method.GetILGenerator();
            LoadState(il, state);

            // define a switch label for each column + 1 for "before first"
            var labels = Enumerable.Range(0, columns.Length).Select(i => il.DefineLabel()).ToArray();
            il.Emit(OpCodes.Switch, labels);

            // default case is return false
            Return(il, false);

            // fore each column
            int idx = 0;
            foreach (var col in columns)
            {
                il.MarkLabel(labels[idx]);
                NewColumn(il, col);
                GetPropertyValue(il, obj, col);
                StoreCurrent(il, current);
                StoreNextState(il, state, idx + 1);
                Return(il, true);
                idx++;
            }
        }

        private static void LoadState(ILGenerator il, FieldBuilder state)
        {
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldfld, state);
        }

        static void NewColumn(ILGenerator il, PropertyInfo column)
        {
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldstr, column.Name);
            il.Emit(OpCodes.Ldtoken, column.PropertyType);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));
            il.Emit(OpCodes.Newobj, typeof(Column).GetConstructor(new[] { typeof(string), typeof(Type) }));
        }

        static void GetPropertyValue(ILGenerator il, FieldBuilder obj, PropertyInfo column)
        {
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldfld, obj);
            il.Emit(OpCodes.Callvirt, column.GetGetMethod());
            if (column.PropertyType.IsValueType)
               il.Emit(OpCodes.Box, column.PropertyType); // box so we can store as a column value
        }

        static void StoreCurrent(ILGenerator il, FieldBuilder current)
        {
            il.Emit(OpCodes.Newobj, typeof(ColumnValue).GetConstructor(new[] { typeof(Column), typeof(object) }));
            il.Emit(OpCodes.Stfld, current);
        }

        static void StoreNextState(ILGenerator il, FieldBuilder state, int value)
        {
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldc_I4, value); // push next state value;
            il.Emit(OpCodes.Stfld, state); // store the state
        }

        static void Return(ILGenerator il, bool result)
        {
            if (result)
                il.Emit(OpCodes.Ldc_I4_1); // return true
            else
                il.Emit(OpCodes.Ldc_I4_0); // return false
            il.Emit(OpCodes.Ret);
        }

        static void DefineDispose(TypeBuilder typeBuilder)
        {
            var method = typeBuilder.DefineMethod(nameof(IDisposable.Dispose), Public | Virtual | Final, HasThis);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ret);
        }

        static void DefineReset(TypeBuilder typeBuilder, FieldBuilder state)
        {
            var method = typeBuilder.DefineMethod(nameof(IEnumerator.Reset), Public | Virtual | Final, HasThis);
            var il = method.GetILGenerator();
            StoreNextState(il, state, 0); // rest state to zero
            il.Emit(OpCodes.Ret);
        }

        static void DefineEnumeratorCurrent(TypeBuilder typeBuilder, FieldBuilder current)
        {
            var prop = typeBuilder.DefineProperty(nameof(IEnumerator.Current), PropertyAttributes.HasDefault, typeof(object), null);
            var getMethod = typeBuilder.DefineMethod("get_Current", Private| Virtual | Final | SpecialName | HideBySig, HasThis, typeof(object), Type.EmptyTypes);
            var il = getMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // push this
            il.Emit(OpCodes.Ldfld, current);
            il.Emit(OpCodes.Box, typeof(ColumnValue)); // cast to object
            il.Emit(OpCodes.Ret);
            prop.SetGetMethod(getMethod);
            typeBuilder.DefineMethodOverride(getMethod, typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current)).GetGetMethod());
        }
    }
}