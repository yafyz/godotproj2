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
		Visible = workspace.Settings.ShowDebug;
		
		Text =   $"[right]" 
		       + $"TimeFrozen={workspace.Settings.TimeFrozen}\n" 
		       + $"TimeScale={workspace.Settings.Timescale}\n"
		       + $"PhysicsRate={workspace.Settings.PhysicsRate}\n"
		       + $"VisualScale={workspace.Settings.VisualScale}\n"
		       + $"CameraMode={workspace.camera.Behavior}\n"
		       + $"SaveFile={workspace.SaveFile}\n"
		       + $"BodyCount={workspace.bodyMap.Count}\n"
		       + $"OrbitDistance={workspace.camera.orbitDistance}\n"
		       + $"CameraPos={workspace.camera.Position}"
		       + $"[/right]";
	}
}
