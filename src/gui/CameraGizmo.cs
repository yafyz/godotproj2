using Godot;
using System;

public partial class CameraGizmo : Control
{
	Workspace workspace;
	
	Line2D xLine;
	Line2D yLine;
	Line2D zLine;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		workspace = GetNode<Workspace>("/root/Workspace");

		xLine = GetNode<Line2D>("LineX");
		yLine = GetNode<Line2D>("LineY");
		zLine = GetNode<Line2D>("LineZ");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector3 center3d = workspace.camera.Position+workspace.camera.LookVector*10;
		
		Vector2 abs_center2d = workspace.camera.UnprojectPosition(center3d);
		Vector2 abs_x2d = workspace.camera.UnprojectPosition(center3d + new Vector3(1, 0, 0));
		Vector2 abs_y2d = workspace.camera.UnprojectPosition(center3d + new Vector3(0, 1, 0));
		Vector2 abs_z2d = workspace.camera.UnprojectPosition(center3d + new Vector3(0, 0, 1));
		
		Vector2 rel_x2d = abs_center2d-abs_x2d;
		Vector2 rel_y2d = abs_center2d-abs_y2d;
		Vector2 rel_z2d = abs_center2d-abs_z2d;

		Vector2 true_center = Size / 2;
		
		xLine.Points = [true_center, true_center + rel_x2d];
		yLine.Points = [true_center, true_center + rel_y2d];
		zLine.Points = [true_center, true_center + rel_z2d];
	}
}
