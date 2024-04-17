using Godot;
using System;

public partial class SpinScript : MeshInstance3D
{
	[Export]
	public float FullRotationTimeSeconds { get; set; }

	[Export] public float SpinMult { get; set; } = 1;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var rotation = Rotation;
		Rotation = rotation with { Y = rotation.Y + (float)(Math.PI * 2 * delta/FullRotationTimeSeconds)*SpinMult };
	}
}
