using Godot;
using System;

public partial class Credits : Control
{
	public override void _Ready()
	{
		GenerateCredits();
	}

	public override void _Process(double delta)
	{
		this.Position = new Vector2(this.Position.X, this.Position.Y - (float)delta*64f);
	}
	public void GenerateCredits()
	{
		foreach(string key in GameManager.Instance.saveManager.saveDataValues.levelsUnlocked.Keys)
		{
			Label newLabel = new Label();
			newLabel.Text = Extensions.SceneNameToHumanFormat(key);
			this.AddChild(newLabel);

			TextureRect textureRect = new TextureRect();
			Texture2D icon = ImageTexture.CreateFromImage(Image.LoadFromFile(GameManager.Instance.saveManager.GetFileSaveDirectory() + key + ".png"));
			textureRect.Texture = icon;
			this.AddChild(textureRect);

			//Spacer
			Label newLabel2 = new Label();
			newLabel2.Text = "";
			this.AddChild(newLabel2);
		}
	}
}
