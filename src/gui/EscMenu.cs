using Godot;
using System;

public partial class EscMenu : Control
{
	Button saveButton;
	Button menuButton;
	Button exitButton;
	Button txManButton;
	private Button settingsButton;

	Workspace workspace;

	private Node ButtonContainer;
	private SettingsGui _settingsGui;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ButtonContainer = GetNode<Node>("PanelContainer/VBoxContainer");
		_settingsGui = GetParent().GetNode<SettingsGui>("Settings");
		
		saveButton =  ButtonContainer.GetNode<Button>("Save");
		menuButton =  ButtonContainer.GetNode<Button>("Menu");
		exitButton =  ButtonContainer.GetNode<Button>("Exit");
		txManButton = ButtonContainer.GetNode<Button>("TextureManager");
		settingsButton = ButtonContainer.GetNode<Button>("Settings");
		workspace = GetParent<Workspace>();

		saveButton.Pressed += () => workspace.Save();
		menuButton.Pressed += () => GetTree().ChangeSceneToFile(Constants.Scenes.MainMenu);
		exitButton.Pressed += () => QuitHelper.Quit(GetTree());
		txManButton.Pressed += () => {
			Hide();
			workspace.textureManager.Show();
		};
		settingsButton.Pressed += () => _settingsGui.ShowGui();
	}

	public bool CheckMouse(Vector2 mpos)
	{
		return Visible && GetGlobalRect().HasPoint(mpos)
			|| _settingsGui.CheckMouse(mpos);
	}
	
    public override void _UnhandledKeyInput(InputEvent @event)
    {
		if (@event.IsActionPressed(Constants.KeyBindings.ESC)) {
			Visible = !Visible;
		}
	}
}
