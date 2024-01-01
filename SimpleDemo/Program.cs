using System;
using F;
using LC;
namespace SimpleDemo {
    class Program {
		static void Main(string[] args) {
			// our expression
			var exp = @"a(b*)";
			Console.WriteLine(exp);
			// parse it
			var nfa = FA.Parse(exp, 0, false);
			Console.WriteLine("foo is match: {0}", nfa.IsMatch("-10"));
			var opts = new FADotGraphOptions();
			// don't need to see accept symbol ids
			opts.HideAcceptSymbolIds = false;
			opts.AcceptSymbolNames = new string[] { "Accept" };
			// uncomment to show expanded epsilons
			nfa.Compact();
			// used for debugging
			nfa.SetIds();
			nfa.RenderToFile(@"..\..\..\ident_or_num_nfa.jpg",opts);
			var dfa = nfa.ToDfa();
			
			dfa.ToMinimized().RenderToFile(@"..\..\..\keyword_or_num_min.jpg",opts);


		}
    }
}
