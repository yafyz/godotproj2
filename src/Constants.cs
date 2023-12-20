using Godot;
using System;

public class Constants
{
	public class Scenes {
		public static StringName MainMenu = "res://main_menu.tscn";
		public static StringName Workspace = "res://workspace.tscn";
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
	}
}
