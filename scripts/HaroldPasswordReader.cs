using Godot;
using System;

public partial class HaroldPasswordReader : Node
{
	[Export]
	private TextEdit rtl;
	public override void _Process(double delta)
	{
		string rtltext = rtl.Text.ToLower();

		if(rtltext.Contains("harold") && rtltext.Contains("is") && rtltext.Contains("a") && rtltext.Contains("virus"))
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement("decoder");

			GameManager.Instance.levelManager.LoadScene("automaton_lung", true, true);
			rtl.Text = "";
		}

		if(rtltext.Length > new String("harold is a virus").Length || rtltext.Contains('\n'))
		{
			rtl.Text = "";
		}
	}
}
