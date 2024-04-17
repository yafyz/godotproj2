using Godot;
using System;

public partial class DoubleSlider : Control
{
	private DoubleInput LineInput;
	private HSlider SliderInput;

	public double Value;
	public event Action<double> ValueChanged;

	public bool WrapValue = false;

	private bool _debounce = false;
	
	public double MinValue {
		get => SliderInput.MinValue;
		set => SliderInput.MinValue = value;
	}
	
	public double MaxValue {
		get => SliderInput.MaxValue;
		set => SliderInput.MaxValue = value;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LineInput = GetNode<DoubleInput>("PanelContainer/HBoxContainer/LineEdit");
		SliderInput = GetNode<HSlider>("PanelContainer/HBoxContainer/HSlider");
		
		LineInput.ValueChanged += LineInputValueChanged;
		SliderInput.ValueChanged += SliderInputValueChanged;
	}

	private void SliderInputValueChanged(double value)
	{
		if (_debounce)
			return;
		
		Value = value;
		LineInput.SetValue(value, false);
		ValueChanged?.Invoke(value);
	}

	private void LineInputValueChanged(double value)
	{
		if (_debounce)
			return;
		
		//value = WrapValue switch {
		//	true => (value - MinValue) % (MaxValue - MinValue) + MinValue,
		//	false => Math.Min(value - MinValue, MaxValue - MinValue) + MinValue 
		//};
		
		Value = value;
		
		_debounce = true;
		SliderInput.Value = value;
		_debounce = false;
		
		ValueChanged?.Invoke(value);
	}

	public void SetValue(double value, bool invokeEvent)
	{
		Value = value;
		
		_debounce = true;
		LineInput.SetValue(value, false);
		SliderInput.Value = value;
		_debounce = false;
		
		if (invokeEvent)
			ValueChanged?.Invoke(value);
	}
}
