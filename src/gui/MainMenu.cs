using Godot;
using System;
using System.Linq;

public partial class MainMenu : Control
{
	Button btn_exit;
	Button btn_new_workspace;
	Button btn_load_workspace;
	private Button btn_settings;

	SavesGui savesGui;

	Panel saveNameDialog_Panel;
	LineEdit saveNameDialog_Text;
	Button saveNameDialog_Ok;
	Button saveNameDialog_Close;

	private SettingsGui _settingsGui;

	private Panel Overlay;

	private SavesManager savesManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		savesManager = GetNode<SavesManager>(Constants.Singletons.SavesManager);

		Overlay = GetNode<Panel>("Overlay");

		_settingsGui = GetNode<SettingsGui>("Settings");
		
		btn_exit = GetNode<Button>("VBoxContainer/Exit");
		btn_new_workspace = GetNode<Button>("VBoxContainer/NewWorkspace");
		btn_load_workspace = GetNode<Button>("VBoxContainer/LoadWorkspace");
		btn_settings = GetNode<Button>("VBoxContainer/Settings");

		savesGui = GetNode<SavesGui>("Saves");

		saveNameDialog_Panel = GetNode<Panel>("SaveNameDialog");
		saveNameDialog_Text = saveNameDialog_Panel.GetNode<LineEdit>("Filename");
		saveNameDialog_Ok = saveNameDialog_Panel.GetNode<Button>("HBoxContainer/Ok");
		saveNameDialog_Close = saveNameDialog_Panel.GetNode<Button>("HBoxContainer/Close");


		btn_exit.Pressed += OnExit;
		btn_new_workspace.Pressed += OnNewWorkspace;
		btn_load_workspace.Pressed += OnLoadWorkspace;
		btn_settings.Pressed += () => _settingsGui.ShowGui();

		saveNameDialog_Close.Pressed += () => saveNameDialog_Panel.Visible = Overlay.Visible = false;
		saveNameDialog_Ok.Pressed += SaveNameDialogConfirmed;
		saveNameDialog_Text.TextChanged += SaveNameDialogTextChanged;

		//GetTree().ChangeSceneToFile(Constants.Scenes.Workspace);
	}

	private void OnExit()
	{
		QuitHelper.Quit(GetTree());
	}

	private void OnNewWorkspace() {
		saveNameDialog_Text.Text = "";
		saveNameDialog_Panel.Visible = true;
		Overlay.Visible = true;
	}

	private void OnLoadWorkspace() {
		savesGui.ShowDialog();
	}

	private void SaveNameDialogTextChanged(string text)
	{
		saveNameDialog_Ok.Disabled = string.IsNullOrEmpty(text);
		if (savesManager.Files.Contains(text)) {
			saveNameDialog_Ok.Text = "overwrite";
		} else {
			saveNameDialog_Ok.Text = "ok";
		}
	}
	
	private void SaveNameDialogConfirmed() {
		var scene = GD.Load<PackedScene>(Constants.Scenes.Workspace);
		var workspace = scene.Instantiate<Workspace>();

		workspace.SaveFile = saveNameDialog_Text.Text;

		if (savesManager.Files.Contains(workspace.SaveFile)) {
			savesManager.DeleteSave(workspace.SaveFile);
		}
		
		SceneHelper.SwitchToScene(GetTree(), workspace);
	}
}
