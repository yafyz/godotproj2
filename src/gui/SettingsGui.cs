using Godot;
using System;

public partial class SettingsGui : Control
{
	private Button CloseButton;
	private TabContainer Tabs;
	private Control TabInsert;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CloseButton = GetNode<Button>("PanelContainer/VBoxContainer/Close");
		Tabs = GetNode<TabContainer>("PanelContainer/VBoxContainer/Tabs");
		TabInsert = GetNode<Control>("TabInsert");
		
		if (TabInsert != null) {
			TabInsert.Visible = false;
			foreach (var __tab in TabInsert.GetChildren()) {
				if (__tab is not Control tab)
					continue;

				tab.Visible = false;
				tab.Reparent(Tabs);
			}
		}
        
		CloseButton.Pressed += CloseButtonClicked;
	}

	public void ShowGui()
	{
		Tabs.CurrentTab = 0;
		Visible = true;
	}
	
	public void CloseButtonClicked()
	{
		Tabs.CurrentTab = 0;
		Visible = false;
	}

	public bool CheckMouse(Vector2 mpos)
	{
		return Visible;
	}
}
