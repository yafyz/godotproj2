using Godot;
using System;
using System.Collections.Generic;

public partial class Workspace : Node3D
{
	Simulation simulation;
	Dictionary<Simulation.Body, MeshInstance3D> bodyMap;
	Node bodiesContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		simulation = new();
		bodyMap = new();
		bodiesContainer = GetNode<Node>("Bodies");

		(GetNode<MeshInstance3D>("MeshInstance3D").Mesh as TextMesh)
			.Text = "balls";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		simulation.Step(delta);

		foreach ((var body, var node) in bodyMap) {
			node.Position = body.Position.ToVector3F();
		}
	}

	public void AddBody(Vector3D position, Vector3D velocity, double mass) {
		Simulation.Body body = new Simulation.Body() {
			Position = position,
			Velocity = velocity,
			Mass = mass
		};

		MeshInstance3D body_node = new();
		body_node.Mesh = new SphereMesh();

		bodiesContainer.AddChild(body_node);

		bodyMap.Add(body, body_node);
		simulation.AddBody(body);
	}
}
