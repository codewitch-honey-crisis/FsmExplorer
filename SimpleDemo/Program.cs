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
			Console.WriteLine(ast);

			var nfa = ast.ToFA(0);
			Console.WriteLine("-10 is match: {0}", nfa.IsMatch("-10"));
			var opts = new FADotGraphOptions();
			// don't need to see accept symbol ids
			opts.HideAcceptSymbolIds = false;
			opts.AcceptSymbolNames = new string[] { "Accept" };
			// uncomment to show expanded epsilons
			//nfa.Compact();
			// used for debugging
			nfa.SetIds();
			nfa = FA.FromTable(nfa.ToTable());
			nfa.RenderToFile(@"..\..\..\ident_or_num_nfa.jpg",opts);
			var dfa = nfa.ToDfa();
			
			dfa.ToMinimized().RenderToFile(@"..\..\..\keyword_or_num_min.jpg",opts);

			dfa.ToMinimized().RenderToFile(@"..\..\..\keyword_or_num_min.dot", opts);
		}
    }
}
