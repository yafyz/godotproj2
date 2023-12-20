using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Workspace : Node3D
{
	Simulation simulation;
	Dictionary<Simulation.Body, SpaceObject> bodyMap;
	Node bodiesContainer;
	Camera camera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		simulation = new();
		bodyMap = new();
		bodiesContainer = GetNode<Node>("Bodies");
		camera = GetNode<Camera>("Camera3D");

		(GetNode<MeshInstance3D>("MeshInstance3D").Mesh as TextMesh)
			.Text = "balls";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		simulation.Step(delta);

		foreach ((var body, var node) in bodyMap) {
			node.Sync();
		}

		/* Body selector for orbit cam */

		var window = GetViewport();
		var world3d = window.World3D;
		var space_state = world3d.DirectSpaceState;
		if (space_state != null) {
			var mpos = window.GetMousePosition();

			var ray_start = camera.ProjectRayOrigin(mpos);
			var ray_end = ray_start + camera.ProjectRayNormal(mpos) * 1000;

			var query = PhysicsRayQueryParameters3D.Create(ray_start, ray_end, 1);
			var result = space_state.IntersectRay(query);

			if (Input.IsActionJustPressed(Constants.KeyBindings.LMB)) {
				if (result.Any()) {
					var obj = result["collider"].As<StaticBody3D>();
					if (bodyMap.Any(kv => kv.Value.Body3D == obj)) {
						camera.SetToOrbit(obj.Owner as Node3D);
					}
				} else {
					camera.SetToFreecam();
				}
			}
		}
	}

	public Simulation.Body AddBody(Vector3D position, Vector3D velocity, double mass) {
		Simulation.Body body = new Simulation.Body() {
			Position = position,
			Velocity = velocity,
			Mass = mass
		};

		var obj = new SpaceObject(body);

		bodiesContainer.AddChild(obj.Mesh);

		bodyMap.Add(body, obj);
		simulation.AddBody(body);

		return body;
	}
}
