using Godot;
using System;

public partial class PausedText : Control
{
	private Workspace _workspace;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_workspace = GetNode<Workspace>("/root/Workspace");

		_workspace.Settings.PropertyChanged += (s,_) => {
			if (s == "TimeFrozen") {
				Visible = _workspace.Settings.TimeFrozen;
			}
		};
	}
}
