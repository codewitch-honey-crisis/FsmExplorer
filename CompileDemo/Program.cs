using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using F;

using LC;
namespace CompileDemo
{
	internal class Program
	{
		static void Main()
		{
			var ident = FA.Parse("[A-Z_a-z][0-9A-Z_a-z]*", 0, false);
			var num = FA.Parse("0|-?[1-9][0-9]*", 1, false);
			var ws = FA.Parse("[ ]+", 2, false);
			
			var lexer = FA.ToLexer(new FA[] { ident, num, ws }, true);
			
			var runner = lexer.Compile();
			var matches = "the quick brown #$*%( fox jumped over the lazy -10 dog".Split(' ');
			foreach (var match in matches)
			{
				Console.WriteLine("{0}:{1}",match, runner.Match(match));
			}
			//runner.Match("bar");
		}
		static void Main2()
		{
			var ident = FA.Parse("[A-Z_a-z][0-9A-Z_a-z]*", 0, false);
			var num = FA.Parse("0|-?[1-9][0-9]*", 1, false);
			var ws = FA.Parse("[ ]+", 2, false);

			var lexer = FA.ToLexer(new FA[] { ident, num, ws }, true);

			var runner = lexer.Compile();
			foreach (var match in runner.Search(LexContext.Create("the quick brown fox jumped over the lazy -10 dog")))
			{
				Console.WriteLine("{0}:{1} at {2}", match.SymbolId, match.Value, match.Position);
			}
			//runner.Match("bar");
		}
	}
}
