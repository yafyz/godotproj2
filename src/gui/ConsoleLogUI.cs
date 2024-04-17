using Godot;
using System;
using System.Linq;

public partial class ConsoleLogUI : Window
{
	private ConsoleLog _consoleLog;
	private RichTextLabel _label;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_consoleLog = GetNode<ConsoleLog>(Constants.Singletons.ConsoleLog);
		_label = GetNode<RichTextLabel>("PanelContainer/RichTextLabel");

		CloseRequested += () => Hide();
		_consoleLog.OnLog += _ => Update();
		
		Update();
	}

	public void ShowGui()
	{
		_label.ScrollFollowing = true;
		PopupCentered();
	}
	
	public void Update()
	{
		var e = _consoleLog.Logs.Select(x => $"[color=#CCCCCC]{x.Time.TimeOfDay}[/color] {x.Text}");
		try {
			_label.Text = string.Join("\n", e);
		} catch {}
	}
}
