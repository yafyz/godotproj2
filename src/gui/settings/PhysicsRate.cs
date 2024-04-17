using Godot;
using System;

public partial class PhysicsRate : HBoxContainer
{
	private Workspace _workspace;
	private DoubleInput _input;

	private bool _debounce;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_workspace = GetNode<Workspace>("/root/Workspace");
		_input = GetNode<DoubleInput>("LineEdit");
		
		_input.ValueChanged += SetSettingsValue;
		_workspace.Settings.PropertyChanged += SettingsOnPropertyChanged;

		RefreshValue();
	}

	private void SettingsOnPropertyChanged(string prop, object _)
	{
		if (_debounce) {
			_debounce = false;
			return;
		}
		
		if (prop == "PhysicsRate")
			RefreshValue();
	}

	public void SetSettingsValue(double d)
	{
		_debounce = true;
		_workspace.Settings.PhysicsRate = d;
	}
	
	public void RefreshValue()
	{
		_input.SetValue(_workspace.Settings.PhysicsRate, false);
	}
}
