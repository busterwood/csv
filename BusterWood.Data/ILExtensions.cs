using System;
using System.Reflection;
using System.Reflection.Emit;
namespace BusterWood.Data
{
    static class ILExtensions
    {
        public static LocalBuilder DeclareLocal<T>(this ILGenerator il)
        {
            return il.DeclareLocal(typeof(T));
        }

        public static ILGenerator Box(this ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Box, type);
            return il;
        }

        public static ILGenerator This(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            return il;
        }

        public static ILGenerator Arg0(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            return il;
        }

        public static ILGenerator Arg1(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_1);
            return il;
        }

        public static ILGenerator Arg(this ILGenerator il, int index)
        {
            il.Emit(OpCodes.Ldarg, index);
            return il;
        }

        public static ILGenerator BranchIfFalse(this ILGenerator il, Label notEqual)
        {
            il.Emit(OpCodes.Brfalse, notEqual);
            return il;
        }

        public static ILGenerator BranchIfNotEqual(this ILGenerator il, Label notEqual)
        {
            il.Emit(OpCodes.Bne_Un, notEqual);
            return il;
        }

        public static ILGenerator Call<T>(this ILGenerator il, string method) => il.Call(typeof(T).GetMethod(method));

        public static ILGenerator Call(this ILGenerator il, MethodInfo method)
        {
            il.Emit(OpCodes.Call, method);
            return il;
        }

        public static ILGenerator Call(this ILGenerator il, ConstructorInfo ctor)
        {
            il.Emit(OpCodes.Call, ctor);
            return il;
        }

        public static ILGenerator GetPropertyValue<T>(this ILGenerator il, string propName) => il.CallGetProperty(typeof(T).GetProperty(propName));

        public static ILGenerator CallGetProperty(this ILGenerator il, PropertyInfo prop)
        {
            il.CallVirt(prop.GetMethod);
            return il;
        }

        public static ILGenerator CallVirt<T>(this ILGenerator il, string method) => il.CallVirt(typeof(T).GetMethod(method));

        public static ILGenerator CallVirt(this ILGenerator il, MethodInfo method)
        {
            il.Emit(OpCodes.Callvirt, method);
            return il;
        }

        public static ILGenerator New(this ILGenerator il, ConstructorInfo ctor)
        {
            il.Emit(OpCodes.Newobj, ctor);
            return il;
        }

        public static ILGenerator Constant(this ILGenerator il, int i)
        {
            il.Emit(OpCodes.Ldc_I4, i);
            return il;
        }

        public static ILGenerator Multiply(this ILGenerator il)
        {
            il.Emit(OpCodes.Mul);
            return il;
        }

        public static ILGenerator LoadAddress(this ILGenerator il, LocalBuilder local)
        {
            il.Emit(OpCodes.Ldloca, local);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, string str)
        {
            il.Emit(OpCodes.Ldstr, str);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, int i)
        {
            il.Emit(OpCodes.Ldc_I4, i);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Ldtoken, type);
            il.Call(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, LocalBuilder local)
        {
            il.Emit(OpCodes.Ldloc, local);
            return il;
        }

        public static ILGenerator Store(this ILGenerator il, LocalBuilder local)
        {
            il.Emit(OpCodes.Stloc, local);
            return il;
        }

        public static ILGenerator AsType(this ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Isinst, type);
            return il;
        }

        public static ILGenerator Load(this ILGenerator il, FieldBuilder field)
        {
            il.Emit(OpCodes.Ldfld, field);
            return il;
        }

        public static ILGenerator Store(this ILGenerator il, FieldBuilder field)
        {
            il.Emit(OpCodes.Stfld, field);
            return il;
        }

        public static ILGenerator Return(this ILGenerator il)
        {
            il.Emit(OpCodes.Ret);
            return il;
        }

        public static ILGenerator Return(this ILGenerator il, bool val)
        {
            il.Emit(val ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
            return il;
        }

        public static ILGenerator Switch(this ILGenerator il, Label[] lables)
        {
            il.Emit(OpCodes.Switch, lables);
            return il;
        }

    }
}