using Godot;
using System;

public partial class ObjectDrag : Control
{
	Workspace workspace;
	MeshInstance3D Object;

	Panel xArrow;
	Panel yArrow;
	Panel zArrow;

	Line2D xLine;
	Line2D yLine;
	Line2D zLine;

	DragAxis draggingAxis = DragAxis.None;

	bool mouseDown = false;

	public event Action Dragging;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		workspace = GetNode<Workspace>("/root/Workspace");
		//Object = workspace.GetNode<MeshInstance3D>("MeshInstance3D2");
		
		xArrow = GetNode<Panel>("X");
		yArrow = GetNode<Panel>("Y");
		zArrow = GetNode<Panel>("Z");

		xLine = GetNode<Line2D>("LineX");
		yLine = GetNode<Line2D>("LineY");
		zLine = GetNode<Line2D>("LineZ");

		xArrow.Position = yArrow.Position = zArrow.Position = new Vector2(-100,-100);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Object == null)
			return;

		var window = GetWindow();
		
		var camPos = workspace.camera.Position;
		var objPos = Object.Position;
		var directionTo = camPos.DirectionTo(objPos);
		
		var a = camPos.DirectionTo(objPos).Dot(workspace.camera.Rotation);
		
		var dist = 0.1f;
		var distance3d = camPos.DistanceTo(objPos);
		var scale = distance3d * (workspace.camera.Fov * (MathF.PI / 180)) * dist * (1-MathF.Abs(a));
		
		/* drag */
		
		var mPos = window.GetMousePosition();
		
		if (draggingAxis != DragAxis.None)
		{
			var plane = draggingAxis switch {
				DragAxis.Z or DragAxis.X => new Plane(Vector3.Up, objPos),
				DragAxis.Y => new Plane((camPos-objPos).Normalized() with {Y = 0},objPos)
			};
			
			var origin = workspace.camera.ProjectRayOrigin(mPos);
			var normal = workspace.camera.ProjectRayNormal(mPos);

			var point = plane.IntersectsRay(origin, normal);
			
			if (point.HasValue)
			{
				Object.Position = objPos = draggingAxis switch {
					DragAxis.X => objPos with { X = point.Value.X-scale },
					DragAxis.Z => objPos with { Z = point.Value.Z-scale },
					DragAxis.Y => objPos with { Y = point.Value.Y-scale }
				};
			}
			
			Dragging?.Invoke();
		}
		
		/* visuals */

		if (!workspace.camera.IsPositionBehind(objPos))
		{
			var wSize = window.Size;
			var tolerance = wSize/2;
			bool vwcheck(Vector2 v) => !(v.X < -tolerance.X || v.Y < -tolerance.Y || v.X > wSize.X+tolerance.X || v.Y > wSize.Y+tolerance.Y);

			var b = 1 + MathF.Abs(a)*MathF.Sqrt(2);
			var center3d = camPos + directionTo*b;
			
			var center = workspace.camera.UnprojectPosition(center3d);
			
			var xArrowPos = workspace.camera.UnprojectPosition(center3d+new Vector3(dist,0,0));
			var yArrowPos = workspace.camera.UnprojectPosition(center3d+new Vector3(0,dist,0));
			var zArrowPos = workspace.camera.UnprojectPosition(center3d+new Vector3(0,0,dist));

			if (vwcheck(xArrowPos))
			{
				xLine.Points = [center, xArrowPos];
				xArrow.Position = xArrowPos - xArrow.Size / 2;
			}

			if (vwcheck(yArrowPos)) {
				yLine.Points = [center, yArrowPos];
				yArrow.Position = yArrowPos - yArrow.Size / 2;
			}
			
			if (vwcheck(zArrowPos)) {
				zLine.Points = [center, zArrowPos];
				zArrow.Position = zArrowPos - zArrow.Size / 2;
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouse evt)
			return;
		
		if ((evt.ButtonMask & MouseButtonMask.Left) == 0)
		{
			mouseDown = false;
			draggingAxis = DragAxis.None;
			return;
		}

		if (!mouseDown)
		{
			if (xArrow.GetRect().HasPoint(evt.Position))  {
				draggingAxis = DragAxis.X;
			} else if (yArrow.GetRect().HasPoint(evt.Position)) {
				draggingAxis = DragAxis.Y;
			} else if (zArrow.GetRect().HasPoint(evt.Position)) {
				draggingAxis = DragAxis.Z;
			}
		}

		mouseDown = true;
	}
	
	enum DragAxis {
		None,
		X,
		Y,
		Z,
	}

	public void SetDrag(MeshInstance3D o)
	{
		draggingAxis = DragAxis.None;
		mouseDown = false;
		Object = o;

		if (Object == null)
		{
			xArrow.Visible = yArrow.Visible = zArrow.Visible = false;
			xLine.Visible = yLine.Visible = zLine.Visible = false;
		}
		else
		{
			xArrow.Visible = yArrow.Visible = zArrow.Visible = true;
			xLine.Visible = yLine.Visible = zLine.Visible = true;
		}
	}
}
