using F;

using LC;

using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F
{
	internal class CompiledProto : FARunner
	{
		protected override int MatchImpl(LexContext lc)
		{
			throw new NotImplementedException();
		}
		protected override FAMatch SearchImpl(LexContext lc)
		{
			lc.EnsureStarted();
			var position = lc.Position;
			var line = lc.Line;
			var column = lc.Column;
			var codepoint = lc.Current;
			if(codepoint==-1)
			{
				return FAMatch.Create(-1, null, 0, 0, 0);
			}
		q0:
			if(codepoint==' ')
			{
				lc.Capture();
				codepoint = lc.Advance();
				goto q1;
			}
			if(codepoint=='-')
			{
				lc.Capture();
				codepoint = lc.Advance();
				goto q2;
			}
			if (codepoint == '0')
			{
				lc.Capture();
				codepoint = lc.Advance();
				goto q4;
			}
			if (codepoint >= '1' && codepoint<= '9')
			{
				lc.Capture();
				codepoint = lc.Advance();
				goto q3;
			}
			if ((codepoint >= 'A' && codepoint <= 'Z' ) || (codepoint == '_') || (codepoint >= 'a' && codepoint <= 'z'))
			{
				lc.Capture();
				codepoint = lc.Advance();
				goto q5;
			}
			codepoint = lc.Advance();
			lc.ClearCapture();
			position = lc.Position;
			line = lc.Line;
			column = lc.Column;
			goto q0;
		q1:
			if(codepoint==' ')
			{
				goto q1;
			}
			return FAMatch.Create(0, lc.CaptureBuffer.ToString(), position, line, column);
		q2:
			if (codepoint >= '1' && codepoint <= '9')
			{
				lc.Capture();
				codepoint = lc.Advance();
				goto q3;
			}
			codepoint = lc.Advance();
			lc.ClearCapture();
			position = lc.Position;
			line = lc.Line;
			column = lc.Column;
			goto q0;
		q3:
			if (codepoint >= '0' && codepoint <= '9')
			{
				lc.Capture();
				codepoint = lc.Advance();
				goto q3;
			}
			return FAMatch.Create(1, lc.CaptureBuffer.ToString(), position, line, column);
		q4:
			return FAMatch.Create(1, lc.CaptureBuffer.ToString(), position, line, column);
		q5:
			if ((codepoint >= '0' && codepoint <= '9') || (codepoint >= 'A' && codepoint <= 'Z') || (codepoint == '_') || (codepoint >= 'a' && codepoint <= 'z'))
			{
				lc.Capture();
				codepoint = lc.Advance();
				goto q5;
			}
			return FAMatch.Create(2, lc.CaptureBuffer.ToString(), position, line, column);
		}
	}
}
