using Godot;

public class WindowHelper
{
	public static bool WindowHasMouse(Window wnd, Vector2 mpos)
	{
		return wnd.Visible
			&& new Rect2(wnd.GetPositionWithDecorations(), wnd.GetSizeWithDecorations())
				.HasPoint(mpos)
			|| wnd.HasFocus();
	}
}
