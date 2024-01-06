﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using LC;
using System.Runtime.InteropServices;
using System.Data;
using System.Runtime.ExceptionServices;

namespace F
{
	
	public static partial class FACompiler
    {
		private static void _EmitConst(ILGenerator il, int i)
		{
			switch (i)
			{
				case -1:
					il.Emit(OpCodes.Ldc_I4_M1);
					break;
				case 0:
					il.Emit(OpCodes.Ldc_I4_0);
					break;
				case 1:
					il.Emit(OpCodes.Ldc_I4_1);
					break;
				case 2:
					il.Emit(OpCodes.Ldc_I4_2);
					break;
				case 3:
					il.Emit(OpCodes.Ldc_I4_3);
					break;
				case 4:
					il.Emit(OpCodes.Ldc_I4_4);
					break;
				case 5:
					il.Emit(OpCodes.Ldc_I4_5);
					break;
				case 6:
					il.Emit(OpCodes.Ldc_I4_6);
					break;
				case 7:
					il.Emit(OpCodes.Ldc_I4_7);
					break;
				case 8:
					il.Emit(OpCodes.Ldc_I4_8);
					break;
				default:
					if (i >= -128 && i < 128)
					{
						il.Emit(OpCodes.Ldc_I4_S, i);
					} else
					{
						il.Emit(OpCodes.Ldc_I4, i);
					}
					break;
			}
		}
		private static void _GenerateRangesExpression(ILGenerator il, int state, Label dest, Label final, int[] ranges)
		{
			for (int i = 0; i < ranges.Length; i+=2)
			{
				int first = ranges[i];
				int last = ranges[i+1];
				var next = il.DefineLabel();
				if (first != last)
				{
					il.Emit(OpCodes.Ldloc_3);
					_EmitConst(il, first);
					il.Emit(OpCodes.Blt, final);
					il.Emit(OpCodes.Ldloc_3);
					_EmitConst(il, last);
					il.Emit(OpCodes.Ble, dest);
				} else
				{
					il.Emit(OpCodes.Ldloc_3);
					_EmitConst(il, first);
					il.Emit(OpCodes.Beq, dest);

				}
				il.MarkLabel(next); 
			}
			il.Emit(OpCodes.Br, final); 
		

		}
		private static Label[] _DeclareLabelsForStates(ILGenerator il, IList<FA> closure)
		{
			var result = new Label[closure.Count];
			for(int i = 0; i < result.Length;i++)
			{
				result[i] = il.DefineLabel();
			}
	
			return result;
		}
		static readonly MethodInfo _ZDbgWL = typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
		static readonly MethodInfo _ZDbgW = typeof(Console).GetMethod("Write", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
		private static void _DbgWriteLine(ILGenerator il, string format, params object[] args)
		{
			var str = (args.Length==0)?format:string.Format(format, args);
			il.Emit(OpCodes.Ldstr, str);
			il.EmitCall(OpCodes.Call, _ZDbgWL,null);
		}
		private static void _DbgWrite(ILGenerator il, string format, params object[] args)
		{
			var str = (args.Length == 0) ? format : string.Format(format, args);
			il.Emit(OpCodes.Ldstr, str);
			il.EmitCall(OpCodes.Call, _ZDbgW, null);
		}
		private static void _DbgOutCP(ILGenerator il)
		{
			var cw = typeof(Console).GetMethod("Write", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
			var cvt = typeof(char).GetMethod("ConvertFromUtf32");
			il.Emit(OpCodes.Ldloc_3);
			il.EmitCall(OpCodes.Call, cvt, null);
			il.EmitCall(OpCodes.Call,cw, null);
		}
		public static FARunner Compile(this FA fa, IProgress<int> progress = null)
		{
			if (fa == null) throw new ArgumentNullException(nameof(fa));
			fa = fa.ToDfa(progress);
			var closure = fa.FillClosure();
			var name = "FARunner" + fa.GetHashCode();
			var asmName = new AssemblyName(name);
			var asm = Thread.GetDomain().DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
	
			ModuleBuilder mod = asm.DefineDynamicModule("M"+name);
			TypeBuilder type = mod.DefineType(name, TypeAttributes.Public | TypeAttributes.Sealed, typeof(FARunner));
			Type[] paramTypes = new Type[] { typeof(LexContext) };
			Type searchReturnType = typeof(FAMatch);
			MethodBuilder searchImpl = type.DefineMethod("SearchImpl", MethodAttributes.Public | MethodAttributes.ReuseSlot |
				MethodAttributes.Virtual | MethodAttributes.HideBySig, searchReturnType, paramTypes);
			
			ILGenerator il = searchImpl.GetILGenerator();
			MethodInfo createMatch = typeof(FAMatch).GetMethod("Create",BindingFlags.Static| BindingFlags.Public);
			PropertyInfo lcpos = typeof(LexContext).GetProperty("Position");
			PropertyInfo lcline = typeof(LexContext).GetProperty("Line");
			PropertyInfo lccol = typeof(LexContext).GetProperty("Column");
			PropertyInfo lccur = typeof(LexContext).GetProperty("Current");
			PropertyInfo lccapb = typeof(LexContext).GetProperty("CaptureBuffer");
			MethodInfo lccapbts = typeof(StringBuilder).GetMethod("ToString",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);
			MethodInfo lcadv = typeof(LexContext).GetMethod("Advance");
			MethodInfo lccap = typeof(LexContext).GetMethod("Capture");
			MethodInfo lccc = typeof(LexContext).GetMethod("ClearCapture");

			il.DeclareLocal(typeof(long)); // position 0
			il.DeclareLocal(typeof(int)); // line 1
			il.DeclareLocal(typeof(int)); // column 2
			il.DeclareLocal(typeof(int)); // current 3

			var states = _DeclareLabelsForStates(il, closure);
			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Call, lcpos.GetGetMethod(), null);
			il.Emit(OpCodes.Stloc_0);
			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Call, lcline.GetGetMethod(), null);
			il.Emit(OpCodes.Stloc_1);
			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Call, lccol.GetGetMethod(), null);
			il.Emit(OpCodes.Stloc_2);
			var start_machine = il.DefineLabel();

			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Call, lccur.GetGetMethod(), null);
			il.Emit(OpCodes.Stloc_3);
			il.Emit(OpCodes.Ldloc_3);
			il.Emit(OpCodes.Ldc_I4_M1);
			il.Emit(OpCodes.Ceq);
			il.Emit(OpCodes.Brfalse_S, start_machine);

			var retEmpty = il.DefineLabel();
			il.MarkLabel(retEmpty);
			il.Emit(OpCodes.Ldc_I4_M1);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Conv_I8);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Ldc_I4_0);
			il.EmitCall(OpCodes.Call, createMatch, null);
			il.Emit(OpCodes.Ret);

			il.MarkLabel(start_machine);
			
			for (int i = 0; i < closure.Count; ++i)
			{
				var cfa = closure[i];
				il.MarkLabel(states[i]);
				var trnsgrp = cfa.FillInputTransitionRangesGroupedByState();
				int j = 0;
				foreach (var trn in trnsgrp)
				{
					var dest = il.DefineLabel();
					var final = il.DefineLabel();
					_GenerateRangesExpression(il,i, dest, final,trn.Value);
					
					
					il.MarkLabel(dest);
					// matched
					var si = closure.IndexOf(trn.Key);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Callvirt, lccap, null);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Callvirt, lcadv, null);
					il.Emit(OpCodes.Stloc_3);
					il.Emit(OpCodes.Br, states[si]);
					il.MarkLabel(final);
					++j;

				}
				// not matched
				if (cfa.IsAccepting)
				{
					_EmitConst(il, cfa.AcceptSymbol);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Callvirt, lccapb.GetGetMethod(), null);
					il.EmitCall(OpCodes.Callvirt, lccapbts, null);
					il.Emit(OpCodes.Ldloc_0);
					il.Emit(OpCodes.Ldloc_1);
					il.Emit(OpCodes.Ldloc_2);
					il.EmitCall(OpCodes.Call, createMatch, null);
					il.Emit(OpCodes.Ret);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Callvirt, lcadv, null);
					il.Emit(OpCodes.Stloc_3);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Call, lccc, null);

					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Call, lcpos.GetGetMethod(), null);
					il.Emit(OpCodes.Stloc_0);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Call, lcline.GetGetMethod(), null);
					il.Emit(OpCodes.Stloc_1);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Call, lccol.GetGetMethod(), null);
					il.Emit(OpCodes.Stloc_2);
					il.Emit(OpCodes.Ldc_I4_M1);
					il.Emit(OpCodes.Ldloc_3);
					il.Emit(OpCodes.Beq, retEmpty);
					il.Emit(OpCodes.Br, states[0]);
				}
			}
			il.Emit(OpCodes.Br, retEmpty);

			MethodInfo searchImplBase = typeof(FARunner).GetMethod("SearchImpl",BindingFlags.NonPublic | BindingFlags.Instance);
			type.DefineMethodOverride(searchImpl, searchImplBase);

			Type matchReturnType = typeof(int);
			MethodBuilder match = type.DefineMethod("MatchImpl", MethodAttributes.Public | MethodAttributes.ReuseSlot |
				MethodAttributes.Virtual | MethodAttributes.HideBySig, matchReturnType, paramTypes);
			il = match.GetILGenerator();

			il.DeclareLocal(typeof(long)); // position 0
			il.DeclareLocal(typeof(int)); // line 1
			il.DeclareLocal(typeof(int)); // column 2
			il.DeclareLocal(typeof(int)); // current 3

			states = _DeclareLabelsForStates(il, closure);
			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Call, lcpos.GetGetMethod(), null);
			il.Emit(OpCodes.Stloc_0);
			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Call, lcline.GetGetMethod(), null);
			il.Emit(OpCodes.Stloc_1);
			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Call, lccol.GetGetMethod(), null);
			il.Emit(OpCodes.Stloc_2);
			start_machine = il.DefineLabel();

			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Call, lccur.GetGetMethod(), null);
			il.Emit(OpCodes.Stloc_3);
			il.Emit(OpCodes.Ldloc_3);
			il.Emit(OpCodes.Ldc_I4_M1);
			il.Emit(OpCodes.Ceq);
			il.Emit(OpCodes.Brfalse_S, start_machine);

			retEmpty = il.DefineLabel();
			il.MarkLabel(retEmpty);
			if(fa.IsAccepting)
			{
				_EmitConst(il, fa.AcceptSymbol);
				il.Emit(OpCodes.Ret);
			} else
			{
				_EmitConst(il, -1);
				il.Emit(OpCodes.Ret);
			}

			il.MarkLabel(start_machine);

			for (int i = 0; i < closure.Count; ++i)
			{
				var cfa = closure[i];
				il.MarkLabel(states[i]);
				var trnsgrp = cfa.FillInputTransitionRangesGroupedByState();
				int j = 0;
				foreach (var trn in trnsgrp)
				{
					var dest = il.DefineLabel();
					var final = il.DefineLabel();
					_GenerateRangesExpression(il, i, dest, final, trn.Value);

					il.MarkLabel(dest);
					// matched
					var si = closure.IndexOf(trn.Key);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Callvirt, lcadv, null);
					il.Emit(OpCodes.Stloc_3);
					il.Emit(OpCodes.Br, states[si]);
					il.MarkLabel(final);
					++j;

				}
				// not matched
				if (cfa.IsAccepting)
				{
					il.Emit(OpCodes.Ldloc_3);
					_EmitConst(il,-1);
					var no_eof = il.DefineLabel();
					il.Emit(OpCodes.Bgt,no_eof);
					_EmitConst(il, cfa.AcceptSymbol);
					il.Emit(OpCodes.Ret);
					il.MarkLabel(no_eof);
					_EmitConst(il, -1);
					il.Emit(OpCodes.Ret);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Callvirt, lcadv, null);
					il.Emit(OpCodes.Stloc_3);
					il.Emit(OpCodes.Ldc_I4_M1);
					il.Emit(OpCodes.Ldloc_3);
					il.Emit(OpCodes.Beq, retEmpty);
					il.Emit(OpCodes.Br, states[0]);
				}
			}
			il.Emit(OpCodes.Br, retEmpty);
			MethodInfo matchBase = typeof(FARunner).GetMethod("MatchImpl", BindingFlags.NonPublic | BindingFlags.Instance);
			type.DefineMethodOverride(match, matchBase);

			Type newType = type.CreateType();

			return (FARunner)Activator.CreateInstance(newType);

		}
	}

	// declare the interface
	public interface IFactorial
	{
		int myfactorial();
	}

	public class SampleFactorialFromEmission
	{
		// emit the assembly using op codes
		private Assembly EmitAssembly(int theValue)
		{
			// create assembly name
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = "FactorialAssembly";

			// create assembly with one module
			AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder newModule = newAssembly.DefineDynamicModule("MFactorial");

			// define a public class named "CFactorial" in the assembly
			TypeBuilder myType = newModule.DefineType("CFactorial", TypeAttributes.Public);

			// Mark the class as implementing IFactorial.
			myType.AddInterfaceImplementation(typeof(IFactorial));

			// define myfactorial method by passing an array that defines
			// the types of the parameters, the type of the return type,
			// the name of the method, and the method attributes.

			Type[] paramTypes = new Type[0];
			Type returnType = typeof(Int32);
			MethodBuilder simpleMethod = myType.DefineMethod("myfactorial", MethodAttributes.Public | MethodAttributes.Virtual, returnType, paramTypes);

			// obtain an ILGenerator to emit the IL
			ILGenerator generator = simpleMethod.GetILGenerator();

			// Ldc_I4 pushes a supplied value of type int32
			// onto the evaluation stack as an int32.
			// push 1 onto the evaluation stack.
			// foreach i less than theValue,
			// push i onto the stack as a constant
			// multiply the two values at the top of the stack.
			// The result multiplication is pushed onto the evaluation
			// stack.
			generator.Emit(OpCodes.Ldc_I4, 1);

			for (Int32 i = 1; i <= theValue; ++i)
			{
				generator.Emit(OpCodes.Ldc_I4, i);
				generator.Emit(OpCodes.Mul);
			}

			// emit the return value on the top of the evaluation stack.
			// Ret returns from method, possibly returning a value.

			generator.Emit(OpCodes.Ret);

			// encapsulate information about the method and
			// provide access to the method metadata
			MethodInfo factorialInfo = typeof(IFactorial).GetMethod("myfactorial");

			// specify the method implementation.
			// pass in the MethodBuilder that was returned
			// by calling DefineMethod and the methodInfo just created
			myType.DefineMethodOverride(simpleMethod, factorialInfo);

			// create the type and return new on-the-fly assembly
			myType.CreateType();
			return newAssembly;
		}

		// check if the interface is null, generate assembly
		// otherwise it is already there, where it is to be...
		public double DoFactorial(int theValue)
		{
			if (thesample == null)
			{
				GenerateCode(theValue);
			}

			// call the method through the interface
			return (thesample.myfactorial());
		}

		// emit the assembly, create an instance and
		// get the interface IFactorial
		public void GenerateCode(int theValue)
		{
			Assembly theAssembly = EmitAssembly(theValue);
			thesample = (IFactorial)theAssembly.CreateInstance("CFactorial");
		}

		// private member data
		IFactorial thesample = null;
	}


}
