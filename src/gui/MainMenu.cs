using Godot;
using System;

public partial class MainMenu : Control
{
	Button btn_exit;
	Button btn_new_workspace;
	Button btn_load_workspace;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		btn_exit = GetNode<Button>("Exit");
		btn_new_workspace = GetNode<Button>("NewWorkspace");
		btn_load_workspace = GetNode<Button>("LoadWorkspace");

		btn_exit.Pressed += OnExit;
		btn_new_workspace.Pressed += OnNewWorkspace;
		btn_load_workspace.Pressed += OnLoadWorkspace;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnExit() {
		GetTree().Quit();
	}

	private void OnNewWorkspace() {
		GetTree().ChangeSceneToFile(Constants.Scenes.Workspace);
	}

	private void OnLoadWorkspace() {
		throw new NotImplementedException("OnLoadWorkspace");
	}
}
