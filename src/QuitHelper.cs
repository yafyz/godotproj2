using Godot;
using System;

public static class QuitHelper
{
	public static void Quit(SceneTree tree)
	{
		tree.Root.PropagateNotification((int)Node.NotificationWMCloseRequest);
	}
}
