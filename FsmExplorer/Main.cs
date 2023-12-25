﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using F;
namespace FsmExplorer
{
	public partial class Main : Form
	{
		FA nfa = null;
		FA dfa = null;
		FA dfa_min = null;
		public Main()
		{
			InitializeComponent();
		}
		private void Render()
		{
			if (nfa == null)
			{
				Graph.Image = null;
				return;
			}
			var opts = new FA.DotGraphOptions();
			opts.HideAcceptSymbolIds = true;
			opts.DebugString = Input.Text;
			FA fa = null;
			if (dfa == null)
			{
				dfa = nfa.ToDfa();
			}
			if (NfaDfa.Checked)
			{
				opts.DebugSourceNfa = nfa;
				opts.DebugShowNfa = true;
				fa = dfa;
			} else if(OptimizedDfa.Checked)
			{
				if(dfa_min == null)
				{
					dfa_min = dfa.ToMinimized();
				}
				fa = dfa_min;
			}
			using (var stm = fa.RenderToStream("jpg", false, opts))
			{
				Graph.Image = Image.FromStream(stm);
			}

		}
		private void Regex_Validated(object sender, EventArgs e)
		{
			Render();
		}

		private void Regex_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				nfa = FA.Parse(Regex.Text,0,false);
				dfa = null;
				dfa_min = null;
			}
			catch
			{
				e.Cancel = true;
			}
		}

		private void Input_TextChanged(object sender, EventArgs e)
		{
			Timeout.Enabled = false;
			Timeout.Enabled = true;
		}

		private void Timeout_Tick(object sender, EventArgs e)
		{
			Timeout.Enabled = false;
			Render();
		}

		private void OptimizedDfa_CheckedChanged(object sender, EventArgs e)
		{
			Timeout.Enabled = false;
			Render();
		}
	}
}
