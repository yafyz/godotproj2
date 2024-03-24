using Godot;
using System;

public partial class WorkspaceInfo : RichTextLabel
{
	private Workspace workspace;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		workspace = GetParent<Workspace>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text =   $"[right]" 
		       + $"TimeFrozen={workspace.TimeFrozen}\n" 
		       + $"TimeScale={workspace.Timescale}\n"
		       + $"PhysicsRate={workspace.PhysicsRate}\n"
		       + $"VisualScale={workspace.VisualScale}\n"
		       + $"CameraMode={workspace.camera.Behavior}\n"
		       + $"SaveFile={workspace.SaveFile}\n"
		       + $"BodyCount={workspace.bodyMap.Count}"
		       + $"[/right]";
	}
}
