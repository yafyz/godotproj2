using Godot;
using System;

public partial class UIFocus : Node
{
	public bool IsFocused => ConsoleFocusOverride || GetWindow().GuiGetFocusOwner() != null;
	public bool ConsoleFocusOverride = false;
	
	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton evt)
			return;
		
		var focus = GetWindow().GuiGetFocusOwner();
		
		if (focus == null)
			return;
		
		if (!focus.GetGlobalRect().HasPoint(evt.Position)) {
			focus.ReleaseFocus();
		}
	}
}