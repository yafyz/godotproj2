using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class JumpToObject : Window
{
	private Workspace _workspace;
	private ItemList _list;
	private Button _jumpButton;

	private List<SpaceObject> Objects = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_workspace = GetNode<Workspace>("/root/Workspace");
		
		_list = GetNode<ItemList>("ScrollContainer/VBoxContainer/ItemList");
		_jumpButton = GetNode<Button>("ScrollContainer/VBoxContainer/Button");

		_jumpButton.Pressed += JumpButtonOnPressed;

		CloseRequested += Hide;
	}

	private void JumpButtonOnPressed()
	{
		if (!_list.IsAnythingSelected())
			return;

		var o = Objects[_list.GetSelectedItems()[0]];
		var r = o.Mesh.Radius;
		_workspace.FocusSpaceObject(o, _workspace.camera.CalculateScrollSpeed(r), _workspace.camera.CalculateOrbitDistance(r), false);
		Hide();
	}

	public void ShowGui()
	{
		Objects.Clear();
		_list.Clear();
		
		Objects.AddRange(_workspace.bodyMap.Select(x => x.Value));
		Objects.ForEach(x => _list.AddItem(x.SimBody.Name));
		
		PopupCentered();
	}

	public bool CheckMouse(Vector2 mpos)
		=> WindowHelper.WindowHasMouse(this, mpos);
}
