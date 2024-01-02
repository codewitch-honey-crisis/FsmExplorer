using System;
using F;
using LC;
namespace SimpleDemo {
    class Program {
		static void Main(string[] args) {
			// our expression
			var exp = @"[A-Z_a-z][A-Z_a-z0-9]*|0|\-?[1-9][0-9]*";
			// parse it
			var ast = RegexExpression.Parse(exp);
			ast.Visit((RegexExpression expr) => { Console.WriteLine(expr.GetType().Name +" "+ expr); return true; });
			var nfa = ast.ToFA(0,false);
			Console.WriteLine("-10 is match: {0}", nfa.IsMatch("-10"));
			var opts = new FADotGraphOptions();
			// don't need to see accept symbol ids
			opts.HideAcceptSymbolIds = false;
			opts.AcceptSymbolNames = new string[] { "accept" };
			// uncomment to hide expanded epsilons
			//nfa.Compact();
			// used for debugging
			nfa.SetIds();
			var array = nfa.ToArray();
			Console.WriteLine("NFA table length is {0} entries.",array.Length);
			nfa = FA.FromArray(array);
			nfa.RenderToFile(@"..\..\..\expression_nfa.jpg",opts);
			nfa.RenderToFile(@"..\..\..\expression_nfa.dot", opts);
			var dfa = nfa.ToDfa();
			var mdfa = dfa.ToMinimized();
			array = mdfa.ToArray();
			Console.WriteLine("Min DFA table length is {0} entries.", array.Length);
			mdfa.RenderToFile(@"..\..\..\expression_dfa_min.jpg",opts);

			mdfa.RenderToFile(@"..\..\..\expression_dfa_min.dot", opts);
		}
    }
}
