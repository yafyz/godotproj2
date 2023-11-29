using Godot;
using System;

public class Vector3Helper
{
	public static Vector3 AnglesToUnit(float side, float up) {
		return new Vector3(
				(float)(-Math.Sin(side)),
				(float)(Math.Sin(up)),
				(float)(-Math.Cos(side))
			)
			.Normalized();
	}
}
