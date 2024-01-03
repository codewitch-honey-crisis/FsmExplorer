using System;
using F;
namespace SimpleDemo {
    class Program {
		static void Main() {
			// our expression
			var exp = @"[A-Z_a-z][A-Z_a-z0-9]*|0|\-?[1-9][0-9]*";
			// search through a string
			//foreach (var match in FA.Parse(exp).Search("the quick brown fox jumped over the -10 lazy dog"))
			//{
			//	Console.WriteLine("{0} at {1}", match.Value, match.Position);
			//}
	
			// parse it
			var ast = RegexExpression.Parse(exp);
			ast.Visit((parent, expr) => { Console.WriteLine(expr.GetType().Name +" "+ expr); return true; });
			var nfa = ast.ToFA(0,false);
			nfa.SetIds();
			var cloned = nfa.ClonePathTo(nfa.FindFirst((fa) => { return fa.Id == 12; }));
			cloned.RenderToFile(@"..\..\..\cloned_nfa.jpg");
			Console.WriteLine("-10 is match: {0}", nfa.IsMatch("-10"));
			var opts = new FADotGraphOptions();
			// show accept symbols
			opts.HideAcceptSymbolIds = false;
			// map symbol ids to names
			opts.AcceptSymbolNames = new string[] { "accept" };
			// uncomment to hide expanded epsilons
			//nfa.Compact();
			// compute the NFA table
			var array = nfa.ToArray();
			Console.WriteLine("NFA table length is {0} entries.",array.Length);
			// rebuild the NFA from the table
			nfa = FA.FromArray(array);
			// make a jpg
			nfa.RenderToFile(@"..\..\..\expression_nfa.jpg",opts);
			// make a dot file
			nfa.RenderToFile(@"..\..\..\expression_nfa.dot", opts);
			// make a DFA
			var dfa = nfa.ToDfa();
			// optimize the DFA
			var mdfa = dfa.ToMinimized();
			// make a DFA table
			array = mdfa.ToArray();
			Console.WriteLine("Min DFA table length is {0} entries.", array.Length);
			// search through a string
			foreach (var match in FA.Search(array, "the quick brown fox jumped over the -10 lazy dog"))
			{
				Console.WriteLine("{0} at {1}", match.Value, match.Position);
			}
			// make a jpg
			mdfa.RenderToFile(@"..\..\..\expression_dfa_min.jpg",opts);
			// make a dot file
			mdfa.RenderToFile(@"..\..\..\expression_dfa_min.dot", opts);

			var ident = FA.Parse("[A-Z_a-z][0-9A-Z_a-z]*",0);
			var num = FA.Parse("0|-?[1-9][0-9]*", 1);
			var ws = FA.Parse("[ ]+", 2);
			opts.AcceptSymbolNames = new string[] { "ident", "num", "ws" };
			var lexer = FA.ToLexer(new FA[] { ident, num, ws });
			lexer.RenderToFile(@"..\..\..\lexer_nfa.jpg", opts);
			foreach (var match in FA.Search(lexer.ToArray(),"the quick brown fox jumped over the -10 lazy dog"))
			{
				Console.WriteLine("{0}:{1} at {2}", match.SymbolId, match.Value, match.Position);
			}

		}
	}
}
