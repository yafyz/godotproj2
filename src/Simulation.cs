using Godot;
using System;
using System.Collections.Generic;

public class Simulation
{
	public class Body {
		public Vector3D Position;
		public Vector3D Velocity;
		public double Mass;
	}

	public List<Body> BodyList;

	public Simulation() {
		BodyList = new();
	}

	public void AddBody(Body body) {
		BodyList.Add(body);
	}

	public void Step(double delta) {
		foreach (var body in BodyList) {
			body.Position.X += body.Velocity.X*delta;
			body.Position.Y += body.Velocity.Y*delta;
			body.Position.Z += body.Velocity.Z*delta;
		}
	}
}

public struct Vector3D {
	public double X,Y,Z;

	public Vector3D(double X, double Y, double Z) {
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}

	public Vector3 ToVector3F() {
		return new Vector3((float)X,(float)Y,(float)Z);
	}
}