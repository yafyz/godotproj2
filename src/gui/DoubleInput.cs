using Godot;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

public partial class DoubleInput : LineEdit
{
	// Called when the node enters the scene tree for the first time.
	//private Regex regex = new("^[0-9]*\\.?[0-9]*$");
	private string lastText = "";
	public double Value = 0;

	public event Action<double> ValueChanged;
	
	public override void _Ready()
	{
		TextChanged += OnTextChanged;
		OnTextChanged(Text);
	}
	
	private void OnTextChanged(string text)
	{
		if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) {
			Text = lastText;
		} else if (text == "") {
			OnTextChanged("0");
		} else  {
			lastText = text;
			Value = v;
			ValueChanged?.Invoke(Value);
		}
	}

	public void SetValue(double v, bool invokeEvent)
	{
		Text = lastText = v.ToString(CultureInfo.InvariantCulture);
		Value = v;
		if (invokeEvent)
			ValueChanged?.Invoke(v);
	}

	//public override void _Input(InputEvent @event)
	//{
	//	if (HasFocus()) {
	//		_GuiInput(@event);
	//		//AcceptEvent();
	//	}
	//}
}
