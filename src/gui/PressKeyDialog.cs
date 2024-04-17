using Godot;
using System;

public partial class PressKeyDialog : PanelContainer
{
	private Button _cancelButton;

	private Action<InputEventKey> _currentCallback;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_cancelButton = GetNode<Button>("VBoxContainer/Button");
		
		_cancelButton.Pressed += CancelButtonOnPressed;
	}

	public void GetKeyInput(Action<InputEventKey> callback)
	{
		if (_currentCallback != null) {
			throw new Exception("uh fuck idk err msg rn yea");
		}

		_currentCallback = callback;

		Visible = true;
	}

	private void Reset()
	{
		_currentCallback = null;
		Visible = false;
	}
	
	private void CancelButtonOnPressed()
	{
		_currentCallback = null;
		Reset();
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event is not InputEventKey evt)
			return;

		if (!evt.IsReleased())
			return;
		
		if (_currentCallback != null) {
			GetViewport().SetInputAsHandled();
			_currentCallback(evt);
			Reset();
		}
	}
}
