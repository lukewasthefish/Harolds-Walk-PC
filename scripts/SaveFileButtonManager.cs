using Godot;
using System;
using System.IO;

public partial class SaveFileButtonManager : Container
{
	[Export]
	private Control rootControl;

	private bool deleteMode = false;
	private Button deleteButton;

	public override void _Ready()
	{
		GenerateButtons();

		GameManager.Instance.selectedCharacter = GameManager.SelectedCharacter.HAROLD;
	}

	public void GenerateButtons()
	{
		for(int i = 0; i < GameManager.Instance.saveManager.maxFiles; i++)
		{
			string filePath = GameManager.Instance.saveManager.GetFileSavePath() + i + ".dat";
			string imageFilePath = GameManager.Instance.saveManager.GetFileSavePath() + i + ".png";

			//GD.Print(filePath);
			//GD.Print(imageFilePath);
			if(Godot.FileAccess.FileExists(filePath))
			{
				SaveFileButton newButton = new SaveFileButton(i);

				newButton.Pressed += LoadSaveFromButton;

				if(File.Exists(imageFilePath))
				{
					Image imageLoaded = Image.LoadFromFile(imageFilePath);
					newButton.Icon = ImageTexture.CreateFromImage(imageLoaded);
				}

				newButton.Text = $"File {(i+1).ToString()}";

				this.AddChild(newButton);
			} else 
			{
				//GD.Print("file does not exist");
				SaveFileButton newButton = new SaveFileButton(i);
				newButton.Text = "-New Game-";
				newButton.Pressed += NewSaveFromButton;
				this.AddChild(newButton);
			}
		}

		deleteButton = new Button();
		deleteButton.Text = "Delete a Save File.";
		deleteButton.Pressed += ToggleDeleteMode;

		this.AddChild(deleteButton);
	}

	private void NewSaveFromButton()
	{
		if(deleteMode)
		{
			DeleteSaveFromButton();
			return;
		}

		//I've decided to remove this ability
	}

	/// <summary>
	/// When this is called we know that the button has a corresponding save attached
	/// </summary>
	private void LoadSaveFromButton()
	{
		if(deleteMode)
		{
			DeleteSaveFromButton();
			return;
		}

		Control focus = GetViewport().GuiGetFocusOwner();

		if(Extensions.IsValid(focus))
		{
			if(focus is SaveFileButton b)
			{
				if(GameManager.Instance.saveManager.Load(b.saveFileIndex))
				{
					GameManager.Instance.levelManager.LoadScene("mode_select_menu", false, false, true);
				} else 
				{
					GameManager.Instance.levelManager.LoadScene("main_menu", false, false, true);
					//TODO : Show some text explaining that the save file was corrupt.
				}
			}
		}
	}

	private void ToggleDeleteMode()
	{
		deleteMode = !deleteMode;

		if(deleteMode)
		{
			deleteButton.Text = "Return to File Select mode.";
		} else 
		{
			deleteButton.Text = "Enter File Deletion mode.";
		}
	}

	private void DeleteSaveFromButton()
	{
		Control focus = GetViewport().GuiGetFocusOwner();

		if(Extensions.IsValid(focus))
		{
			if(focus is SaveFileButton b)
			{
				GameManager.Instance.saveManager.saveFileIndex = b.saveFileIndex;
				GameManager.Instance.levelManager.LoadScene("file_deletion_confirmation_menu", false, false, true);
			}
		}
	}
}
