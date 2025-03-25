using Godot;
using System;

public partial class SaveFileButton : Button
{
	public int saveFileIndex = 0;

	public SaveFileButton(int saveFileIndex)
	{
		this.saveFileIndex = saveFileIndex;
	}
}
