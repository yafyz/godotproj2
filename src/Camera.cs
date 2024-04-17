using Godot;
using System;

public partial class Camera : Camera3D
{
	public enum CameraBehavior {
		Freecam, Orbit
	}

	public float ScrollSpeed = 1;
	float Speed => SettingsManager.Settings.CameraFlySpeed;
	float SpeedFastMultiplier => SettingsManager.Settings.CameraFlySpeedMultiplier;

	Vector2 LastMousePos;
	Vector2 MouseDelta;

	public MeshInstance3D OrbitSubject;
	public float orbitDistance { get; set; } = 2;
	Vector2 orbitAngle;
	
	public CameraBehavior Behavior = CameraBehavior.Freecam;

	private UIFocus uiFocus;

	public Marker3D LookMarker;
	public Vector3 LookVector => (Position - LookMarker.GlobalPosition).Normalized();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		uiFocus = GetNode<UIFocus>(Constants.Singletons.UIFocus);
		LookMarker = GetNode<Marker3D>("Marker3D");
		
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
					rotation.Y += -SettingsManager.Settings.MouseSensitivity*MouseDelta.X;
					rotation.X += -SettingsManager.Settings.MouseSensitivity*MouseDelta.Y;
				}

				/* Camera movement */

				if (Behavior == CameraBehavior.Freecam && !uiFocus.IsFocused) {
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
					orbitAngle.X += MouseDelta.X * SettingsManager.Settings.MouseSensitivity;
					orbitAngle.Y = Math.Clamp(orbitAngle.Y - MouseDelta.Y * SettingsManager.Settings.MouseSensitivity, 0.0001f, MathF.PI*0.9999f);
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
				orbitDistance += SettingsManager.Settings.CameraScrollSpeed * factor * ScrollSpeed;
			}
			if (evtm.ButtonIndex == MouseButton.WheelDown) {
				orbitDistance -= SettingsManager.Settings.CameraScrollSpeed * factor * ScrollSpeed;
			}

			orbitDistance = MathF.Max(0.1f, orbitDistance);
		}
    }

	public void SetToFreecam() {
		Behavior = CameraBehavior.Freecam;
		OrbitSubject = null;
	}

	public void SetToOrbit(MeshInstance3D obj, float scrollSpeed = -1, float distance = -1, bool adjustAngle = true) {
		Behavior = CameraBehavior.Orbit;
		OrbitSubject = obj;

		if (adjustAngle || Behavior == CameraBehavior.Freecam) {
			Vector3 relativePos = Position - OrbitSubject.Position;
			Vector2 c = new Vector2(relativePos.X, relativePos.Z).Normalized();
			orbitAngle = new Vector2(c.Angle(), MathF.Acos(relativePos.Normalized().Y));
		}
		
		orbitDistance = distance switch {
			not -1 => distance,
			_ => (obj.Position - Position).Length()
		};
		ScrollSpeed = scrollSpeed switch {
			not -1 => scrollSpeed,
			_ => 1
		};
	}

	public float CalculateOrbitDistance(float radius)
		=> radius * 10;

	public float CalculateScrollSpeed(float radius)
		=> radius * 5;
}
