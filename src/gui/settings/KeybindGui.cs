using Godot;
using System;

public partial class KeybindGui : HBoxContainer
{
	public StringName Action;
	public string Name;
	
	private Label _nameLabel;
	private Button _rebindButton;

	private PressKeyDialog _pressKeyDialog;
	
	public static KeybindGui New(StringName action, string name, PressKeyDialog keyDialog)
	{
		var sc = GD.Load<PackedScene>(Constants.Scenes.KeybindGui);
		var inst = sc.Instantiate<KeybindGui>();

		inst.Action = action;
		inst.Name = name;
		inst._pressKeyDialog = keyDialog;
		
		return inst;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_nameLabel = GetNode<Label>("Name");
		_rebindButton = GetNode<Button>("Button");

		_nameLabel.Text = Name;
		RefreshButtonText();
		
		_rebindButton.Pressed += RebindButtonOnPressed;
	}

	private void RefreshButtonText()
	{
		Key key = ((InputEventKey)InputMap.ActionGetEvents(Action)[0])
			.GetPhysicalKeycodeWithModifiers();
		_rebindButton.Text = OS.GetKeycodeString(key);
	}
	
	private void RebindButtonOnPressed()
	{
		_pressKeyDialog.GetKeyInput(GetKeyInputCallback);
	}

	private void GetKeyInputCallback(InputEventKey evt)
	{
		if (evt == null)
			return;
		
		InputMap.ActionEraseEvents(Action);
		InputMap.ActionAddEvent(Action, evt);

		SettingsManager.Settings.Keybinds[Action] = evt.GetPhysicalKeycodeWithModifiers();
		
		RefreshButtonText();
	}
}
