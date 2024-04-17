using Godot;
using System;

public partial class CameraFlySpeed : HBoxContainer
{
	private DoubleInput _input;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_input = GetNode<DoubleInput>("LineEdit");
		_input.SetValue(SettingsManager.Settings.CameraFlySpeed, false);
		_input.ValueChanged += d => SettingsManager.Settings.CameraFlySpeed = (float)d;
	}
}
