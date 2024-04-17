using Godot;
using System;

public partial class Vector3DAngular : VBoxContainer
{
	public Vector3D Value {
		get {
			double length = VelocityInput.Value;
			double yaw = (Math.PI/180)*YawInput.Value;
			double pitch = (Math.PI/180)*PitchInput.Value;

			Vector3D direction = new(Math.Cos(yaw)*Math.Cos(pitch), Math.Sin(pitch), Math.Sin(yaw)*Math.Cos(pitch));
			
			return direction.Normalized()*length;
		}
	}

	public event Action<Vector3D> ValueChanged;

	private DoubleInput VelocityInput;
	private DoubleSlider YawInput;
	private DoubleSlider PitchInput;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		VelocityInput = GetNode<DoubleInput>("Velocity/LineEdit");
		YawInput = GetNode<DoubleSlider>("Yaw/DoubleSlider");
		PitchInput = GetNode<DoubleSlider>("Pitch/DoubleSlider");

		YawInput.MinValue = -180;
		YawInput.MaxValue = 180;

		PitchInput.MinValue = -180;
		PitchInput.MaxValue = 180;

		VelocityInput.ValueChanged += _ => ValueChanged?.Invoke(Value);
		YawInput.ValueChanged += _ => ValueChanged?.Invoke(Value);
		PitchInput.ValueChanged += _ => ValueChanged?.Invoke(Value);
	}
	
	public void SetValue(Vector3D value, bool invokeEvent)
	{
		double length = value.Length();
		double pitch = 0;
		double yaw = 0;

		if (length != 0) {
			Vector3D unit = value.Normalized();
			pitch = Math.Asin(unit.Y);
			yaw = -(Math.Atan2(unit.X, unit.Z)-Math.PI/2);
		}
		
		VelocityInput.SetValue(length, false);
		PitchInput.SetValue(pitch/(Math.PI/180), false);
		YawInput.SetValue(yaw/(Math.PI/180), false);
		
		if (invokeEvent)
			ValueChanged?.Invoke(value);
	}
}
