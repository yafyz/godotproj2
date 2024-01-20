using Godot;
using System;

public partial class Camera : Camera3D
{
	public enum CameraBehavior {
		Freecam, Orbit
	}

	float Speed = 4f;
	float SpeedFastMultiplier = 4f;

	Vector2 LastMousePos;
	Vector2 MouseDelta;

	public Node3D OrbitSubject;
	float orbitDistance = 2;
	Vector2 orbitAngle;
	
	public CameraBehavior Behavior = CameraBehavior.Freecam;

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

		switch (Behavior) {
			case CameraBehavior.Freecam:
				/* Camera rotation */
				
				if (Input.IsActionPressed(Constants.KeyBindings.RMB)) {
					rotation.Y += -Settings.MouseSensitivity*MouseDelta.X;
					rotation.X += -Settings.MouseSensitivity*MouseDelta.Y;
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
		
				break;
			case CameraBehavior.Orbit:
				if (OrbitSubject == null) {
					Behavior = CameraBehavior.Freecam;
					_PhysicsProcess(delta);
					return;
				}

				/* Camera rotation */

				var orbitPos = OrbitSubject.Position;

				if (Input.IsActionPressed(Constants.KeyBindings.RMB))
				{
					orbitAngle.X += MouseDelta.X * Settings.MouseSensitivity;
					orbitAngle.Y = Math.Clamp(orbitAngle.Y - MouseDelta.Y * Settings.MouseSensitivity, 0.0001f, MathF.PI*0.9999f);
				}

				Vector2 zxPlane = new Vector2(MathF.Cos(orbitAngle.X), Mathf.Sin(orbitAngle.X)) * orbitDistance * MathF.Sin(orbitAngle.Y);
				Vector3 newPos = orbitPos + new Vector3(zxPlane.X, orbitDistance * MathF.Cos(orbitAngle.Y), zxPlane.Y);

				LookAtFromPosition(newPos, orbitPos);
				
				break;
		}

		LastMousePos = curr_mouse_pos;
	}

    public override void _UnhandledInput(InputEvent evt)
    {
		/* Orbit camera scroll zoom */
        if (Behavior == CameraBehavior.Orbit
			&& evt is InputEventMouseButton evtm
			&& evtm.IsPressed()) {
			
			var factor = evtm.Factor == 0 ? 1 : evtm.Factor;

			if (evtm.ButtonIndex == MouseButton.WheelUp) {
				orbitDistance += Settings.ScrollSpeed * factor;
			}
			if (evtm.ButtonIndex == MouseButton.WheelDown) {
				orbitDistance -= Settings.ScrollSpeed * factor;
			}

			orbitDistance = MathF.Max(1, orbitDistance);
		}
    }

	public void SetToFreecam() {
		Behavior = CameraBehavior.Freecam;
		OrbitSubject = null;
	}

	public void SetToOrbit(Node3D obj) {
		Behavior = CameraBehavior.Orbit;
		OrbitSubject = obj;
		
		Vector3 relativePos = Position - OrbitSubject.Position;
		Vector2 c = new Vector2(relativePos.X, relativePos.Z).Normalized();
		orbitAngle = new Vector2(c.Angle(), MathF.Acos(relativePos.Normalized().Y));
		orbitDistance = (obj.Position - Position).Length();
	}
}
