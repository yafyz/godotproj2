using Godot;
using System;
using System.Linq;
using Godot.Collections;

public partial class KeybindsSettings : VBoxContainer
{
	public static Dictionary<StringName, string> NameMap = new() {
		[Constants.KeyBindings.MoveForward] = "Fly forward",
		[Constants.KeyBindings.MoveBackward] = "Fly backward",
		[Constants.KeyBindings.MoveLeft] = "Fly left",
		[Constants.KeyBindings.MoveRight] = "Fly right",
		
		[Constants.KeyBindings.MoveUp] = "Fly up",
		[Constants.KeyBindings.MoveDown] = "Fly down",
		
		[Constants.KeyBindings.MoveFaster] = "Fly faster",
		
		[Constants.KeyBindings.OpenConsole] = "Open console",
		
		[Constants.KeyBindings.OpenSpawnMenu] = "Spawn object",
		[Constants.KeyBindings.EditMode] = "Edit mode",
		
		[Constants.KeyBindings.ShowDebug] = "Show debug",
		[Constants.KeyBindings.FreezeTime] = "Freeze time",
		
		[Constants.KeyBindings.JumpToObject] = "Open jump to object menu"
	};
	
	
	
	public override void _Ready()
	{
		var keyDialog = GetNode<PressKeyDialog>("../../../../../../PressKeyDialog");
		
		using var enumerator = InputMap.GetActions()
			.Where(x => NameMap.ContainsKey(x))
			.GetEnumerator();
		bool hasNext = enumerator.MoveNext();
		while (hasNext) {
			var action = enumerator.Current;
			
			AddChild(KeybindGui.New(action, NameMap[action], keyDialog));
			hasNext = enumerator.MoveNext();
			if (hasNext) {
				AddChild(new HSeparator());
			}
		}
	}
}
