using Godot;
using System;

public partial class EscMenu : Control
{
	Button saveButton;
	Button menuButton;
	Button exitButton;
	Button txManButton;

	Workspace workspace;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		saveButton = GetNode<Button>("Panel/Save");
		menuButton = GetNode<Button>("Panel/Menu");
		exitButton = GetNode<Button>("Panel/Exit");
		txManButton = GetNode<Button>("Panel/TextureManager");
		workspace = GetParent<Workspace>();

		saveButton.Pressed += () => workspace.Save();
		menuButton.Pressed += () => GetTree().ChangeSceneToFile(Constants.Scenes.MainMenu);
		exitButton.Pressed += () => GetTree().Quit();
		txManButton.Pressed += () => {
			Hide();
			workspace.textureManager.Show();
		};
	}

    public override void _UnhandledKeyInput(InputEvent @event)
    {
		if (@event.IsActionPressed(Constants.KeyBindings.ESC)) {
			Visible = !Visible;
		}
	}
}
