using Godot;
using System;
using System.Linq;

public partial class SavesGui : Panel
{
	ItemList itemList;
	Button closeButton;
	Button loadButton;

	SavesManager savesManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		savesManager = GetNode<SavesManager>(Constants.Singletons.SavesManager);
		itemList = GetNode<ItemList>("SaveList");
		closeButton = GetNode<Button>("Close");
		loadButton = GetNode<Button>("Load");

		closeButton.Pressed += () => Visible = false;
		loadButton.Pressed += Load;

		loadButton.Disabled = true;
		itemList.ItemSelected += (x) => loadButton.Disabled = false;

		UpdateList();
	}

	public void UpdateList() {
		itemList.Clear();
		loadButton.Disabled = true;
		foreach (var file in savesManager.Files) {
			itemList.AddItem(file);
		}
	}

	public void Load() {
		var scene = GD.Load<PackedScene>(Constants.Scenes.Workspace);
		var workspace = scene.Instantiate<Workspace>();

		workspace.SaveFile = savesManager.Files[itemList.GetSelectedItems().First()];

		GetTree().Root.AddChild(workspace);
		GetTree().Root.RemoveChild(GetTree().CurrentScene);
	}

	public void ShowDialog() {
		UpdateList();
		Visible = true;
	}
}
