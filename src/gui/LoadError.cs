using Godot;
using System;

public partial class LoadError : Control
{
	private Label _savenameLabel;
	private TextEdit _exceptionBox;
	private Button _goBackButton;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_savenameLabel = GetNode<Label>("VBoxContainer/HBoxContainer/Savename");
		_exceptionBox = GetNode<TextEdit>("VBoxContainer/ExceptionBox");
		_goBackButton = GetNode<Button>("VBoxContainer/GoBack");

		_goBackButton.Pressed += GoBack;
	}

	public void ShowError(string savename, Exception exception)
	{
		_savenameLabel.Text = savename;
		_exceptionBox.Text = exception.ToString();
		Visible = true;
	}

	private void GoBack()
	{
		GetTree().ChangeSceneToFile(Constants.Scenes.MainMenu);
	}
}
