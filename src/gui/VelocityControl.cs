using Godot;
using System;

public partial class VelocityControl : Control
{
	public Vector3D Value;
	public ControlMode Mode = ControlMode.Vector;

	private CheckButton _modeSelector;
	private Control _vectorControl;
	private Vector3DAngular _angularControl;

	private Vector3DControl _vectorInput;
	
	public event Action<Vector3D> ValueChanged; 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_modeSelector = GetNode<CheckButton>("Toggle/CheckButton");
		_vectorControl = GetNode<Control>("Vector");
		_angularControl = GetNode<Vector3DAngular>("Angular");
		_vectorInput = _vectorControl.GetNode<Vector3DControl>("Velocity");
		
		_vectorInput.ValueChanged += VectorInputOnValueChanged;
		_angularControl.ValueChanged += AngularControlOnValueChanged;
		
		_modeSelector.Toggled += ModeSelectorOnToggled;
		
		SetMode(Mode);
	}

	private void ModeSelectorOnToggled(bool toggledon)
	{
		SetMode(toggledon ? ControlMode.Angular : ControlMode.Vector);
	}

	private void AngularControlOnValueChanged(Vector3D value)
	{
		_vectorInput.SetValue(value, false);
		ValueChanged?.Invoke(value);
	}

	private void VectorInputOnValueChanged(Vector3D value)
	{
		_angularControl.SetValue(value, false);
		ValueChanged?.Invoke(value);
	}

	public void SetMode(ControlMode mode)
	{
		Mode = mode;
		_vectorControl.Visible = (mode == ControlMode.Vector);
		_angularControl.Visible = (mode == ControlMode.Angular);
	}

	public void SetValue(Vector3D value, bool invokeEvent)
	{
		Value = value;
		_vectorInput.SetValue(value, false);
		_angularControl.SetValue(value, false);
		
		if (invokeEvent)
			ValueChanged?.Invoke(value);
	}
	
	public enum ControlMode
	{
		Vector,
		Angular
	}
}
