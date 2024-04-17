using Godot;
using System;

public partial class Vector3DControl : Control
{
	private DoubleInput xInput;
	private DoubleInput yInput;
	private DoubleInput zInput;

	public Vector3D Value = new();

	public event Action<Vector3D> ValueChanged;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		xInput = GetNode<DoubleInput>("PanelContainer/HBoxContainer/X");
		yInput = GetNode<DoubleInput>("PanelContainer/HBoxContainer/Y");
		zInput = GetNode<DoubleInput>("PanelContainer/HBoxContainer/Z");

		xInput.ValueChanged += InputChanged;
		yInput.ValueChanged += InputChanged;
		zInput.ValueChanged += InputChanged;
	}

	private void InputChanged(double _)
	{
		Value = new Vector3D(xInput.Value, yInput.Value, zInput.Value);
		ValueChanged?.Invoke(Value);
	}

	public void SetValue(Vector3D v, bool invokeEvent)
	{
		xInput.SetValue(v.X, false);
		yInput.SetValue(v.Y, false);
		zInput.SetValue(v.Z, false);
		
		Value = v;
		
		if (invokeEvent)
			ValueChanged?.Invoke(v);
	}
}
