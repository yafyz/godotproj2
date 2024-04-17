using Godot;
using System;

public class Constants
{
	public const string SettingsFile = "settings.json";
	
	public class Scenes {
		public static StringName MainMenu = "res://main_menu.tscn";
		public static StringName Workspace = "res://workspace.tscn";

		public static StringName KeybindGui = "res://src/gui/settings/KeybindGui.tscn";
	}

	public class Singletons {
		public static NodePath SettingsSave = "/root/SettingsSave";
		public static NodePath SavesManager = "/root/SavesManager";
		public static NodePath UIFocus = "/root/UiFocus";
		public static NodePath ConsoleLog = "/root/ConsoleLog";
	}

	public class KeyBindings {
		public readonly static StringName MoveForward = "move_forward";
		public readonly static StringName MoveBackward = "move_backward";
		public readonly static StringName MoveLeft = "move_left";
		public readonly static StringName MoveRight = "move_right";
		public readonly static StringName MoveUp = "move_up";
		public readonly static StringName MoveDown = "move_down";
		public readonly static StringName MoveFaster = "move_faster";
		
		public readonly static StringName RMB = "RMB";
		public readonly static StringName LMB = "LMB";

		public readonly static StringName OpenConsole = "open_console";
		public readonly static StringName CloseConsole = "close_console";

		public readonly static StringName OpenSpawnMenu = "open_spawn_menu";
		public readonly static StringName ESC = "ESC";
		public readonly static StringName EditMode = "edit_mode";

		public readonly static StringName ShowDebug = "show_debug";
		public readonly static StringName FreezeTime = "freeze_time";

		public readonly static StringName JumpToObject = "jump_to_object";
	}
}
