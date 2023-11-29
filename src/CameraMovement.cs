using Godot;
using System;

public partial class CameraMovement : Camera3D
{
	enum CameraBehavior {
		Freecam, Orbit
	}

	float Speed = 4f;
	float SpeedFastMultiplier = 4f;

	Vector2 LastMousePos;
	Vector2 MouseDelta;

	CameraBehavior Behavior = CameraBehavior.Freecam;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("balls cam");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		var position = Position;
		var rotation = Rotation;

		var curr_mouse_pos = GetWindow().GetMousePosition();
		MouseDelta = new Vector2(curr_mouse_pos.X-LastMousePos.X, curr_mouse_pos.Y-LastMousePos.Y);

		/* Camera rotation */

		if (Input.IsActionPressed(Constants.KeyBindings.RMB)) {
			switch (Behavior) {
			case CameraBehavior.Freecam:
				rotation.Y += -Settings.MouseSensitivity*MouseDelta.X;
				rotation.X += -Settings.MouseSensitivity*MouseDelta.Y;
				break;
			case CameraBehavior.Orbit:
				break;
			}
		}

		/* Camera movement */

		if (Behavior == CameraBehavior.Freecam) {
			float length = (float)delta*Speed;
			var direction = Vector3Helper.AnglesToUnit(rotation.Y, rotation.X);
			var side_vector = new Vector3((float)-Math.Cos(rotation.Y), 0, (float)Math.Sin(rotation.Y))
									.Normalized();

			if (Input.IsActionPressed(Constants.KeyBindings.MoveFaster)) {
				length *= SpeedFastMultiplier;
			}

			if (Input.IsActionPressed(Constants.KeyBindings.MoveForward)) {
				position += length*direction;
			}

			if (Input.IsActionPressed(Constants.KeyBindings.MoveBackward)) {
				position += length*-direction;
			}

			if (Input.IsActionPressed(Constants.KeyBindings.MoveLeft)) {
				position += length*side_vector;
			}

			if (Input.IsActionPressed(Constants.KeyBindings.MoveRight)) {
				position += length*-side_vector;
			}

			if (Input.IsActionPressed(Constants.KeyBindings.MoveUp)) {
				position.Y += length;
			}

			if (Input.IsActionPressed(Constants.KeyBindings.MoveDown)) {
				position.Y -= length;
			}
		}

		/* Set shit */

		Position = position;
		Rotation = rotation;

		LastMousePos = curr_mouse_pos;
	}
}
