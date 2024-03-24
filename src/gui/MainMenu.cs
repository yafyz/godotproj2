using Godot;
using System;

public partial class MainMenu : Control
{
	Button btn_exit;
	Button btn_new_workspace;
	Button btn_load_workspace;

	SavesGui savesGui;

	Panel saveNameDialog_Panel;
	TextEdit saveNameDialog_Text;
	Button saveNameDialog_Ok;
	Button saveNameDialog_Close;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		btn_exit = GetNode<Button>("Exit");
		btn_new_workspace = GetNode<Button>("NewWorkspace");
		btn_load_workspace = GetNode<Button>("LoadWorkspace");

		savesGui = GetNode<SavesGui>("Saves");

		saveNameDialog_Panel = GetNode<Panel>("SaveNameDialog");
		saveNameDialog_Text = saveNameDialog_Panel.GetNode<TextEdit>("Filename");
		saveNameDialog_Ok = saveNameDialog_Panel.GetNode<Button>("Ok");
		saveNameDialog_Close = saveNameDialog_Panel.GetNode<Button>("Close");


		btn_exit.Pressed += OnExit;
		btn_new_workspace.Pressed += OnNewWorkspace;
		btn_load_workspace.Pressed += OnLoadWorkspace;

		saveNameDialog_Close.Pressed += () => saveNameDialog_Panel.Visible = false;
		saveNameDialog_Ok.Pressed += SaveNameDialogConfirmed;
		saveNameDialog_Text.TextChanged += () => saveNameDialog_Ok.Disabled = string.IsNullOrEmpty(saveNameDialog_Text.Text);

		//GetTree().ChangeSceneToFile(Constants.Scenes.Workspace);
	}

	private void OnExit() {
		GetTree().Quit();
	}

	private void OnNewWorkspace() {
		saveNameDialog_Text.Text = "";
		saveNameDialog_Panel.Visible = true;
	}

	private void OnLoadWorkspace() {
		savesGui.ShowDialog();
	}

	private void SaveNameDialogConfirmed() {
		var scene = GD.Load<PackedScene>(Constants.Scenes.Workspace);
		var workspace = scene.Instantiate<Workspace>();

		workspace.SaveFile = saveNameDialog_Text.Text;

		GetTree().Root.AddChild(workspace);
		GetTree().CurrentScene.QueueFree();
		GetTree().CurrentScene = workspace;
	}
}
