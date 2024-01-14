using Godot;
using System;
using System.Collections.Generic;

public class Simulation
{
	public static double NewtonGravConstant = 6.67430*Math.Pow(10, -11);

	public List<Body> BodyList;

	public Simulation() {
		BodyList = new();
	}

	public void RemoveBody(Body body)  {
		BodyList.Remove(body);
	}

	public void AddBody(Body body) {
		BodyList.Add(body);
	}

	public void Step(double delta) {
		foreach (var body in BodyList) {
			Vector3D acc = new();

			foreach (var body2 in BodyList) {
				if (body == body2) continue;

				Vector3D off = body.Position-body2.Position;
				double force = NewtonGravConstant*(body2.Mass/off.Length());
				acc += -off.Normalize()*force;
			}

			body.OldAcceleration = body.Acceleration;
			body.Acceleration = acc;
		}

		foreach (var body in BodyList) {
			body.Position += body.Velocity*delta + body.OldAcceleration*(delta*delta)*0.5;
			body.Velocity += (body.Acceleration+body.OldAcceleration)*(delta*0.5);
		}
	}

	public class Body {
		public Vector3D Position;
		public Vector3D Velocity;
		public double Mass;
		public Vector3D OldAcceleration;
		public Vector3D Acceleration;
	}
}

public class SpaceObject {
	public MeshInstance3D Mesh;
	public StaticBody3D Body3D;
	public CollisionShape3D Collider;
	public Simulation.Body SimBody;

	public MeshInstance3D AccelerationArrow;
	public MeshInstance3D VelocityArrow;

	public SpaceObject(Simulation.Body simbody) {
		SimBody = simbody;

		Mesh = new MeshInstance3D();
		Mesh.Mesh = new SphereMesh();

		Body3D = new StaticBody3D();
		Mesh.AddChild(Body3D);
		Body3D.Owner = Mesh;
		
		Collider = new CollisionShape3D();
		Collider.Shape = new SphereShape3D();

		Body3D.AddChild(Collider);
		Collider.Owner = Mesh;

		AccelerationArrow = new MeshInstance3D();
		AccelerationArrow.Mesh = new BoxMesh();
		AccelerationArrow.MaterialOverride = new StandardMaterial3D() {
			AlbedoColor = new(0x00_FF_00_00)
		};

		VelocityArrow = new MeshInstance3D();
		VelocityArrow.Mesh = new BoxMesh();
		VelocityArrow.MaterialOverride = new StandardMaterial3D() {
			AlbedoColor = new(0xFF_FF_00_00)
		};

		Mesh.AddChild(AccelerationArrow);
		Mesh.AddChild(VelocityArrow);

		Sync();
	}

	public void Sync() {
		var pos = SimBody.Position.ToVector3F();
		Mesh.Position = pos;
		Body3D.Scale = Mesh.Scale;

		var accel = SimBody.Acceleration.ToVector3F();
		AccelerationArrow.Scale = new(0.05f, 0.05f, 1f);
		AccelerationArrow.LookAtFromPosition(Mesh.Position+accel.Normalized(), Mesh.Position+accel);
	
		var vel = SimBody.Velocity.ToVector3F();
		VelocityArrow.Scale = new(0.05f, 0.05f, 1f);
		VelocityArrow.LookAtFromPosition(Mesh.Position+vel.Normalized(), Mesh.Position+vel);
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

	public double Length() {
		return Math.Sqrt(X*X+Y*Y+Z*Z);
	}

	public double LengthSquared() {
		return X*X+Y*Y+Z*Z;
	}

	public Vector3D Normalize() {
		var res = this;
		var ls = res.LengthSquared();

		res.X /= ls;
		res.Y /= ls;
		res.Z /= ls;

		return res;
	}

	public static Vector3D operator +(Vector3D a, Vector3D b)
		=> new(a.X+b.X, a.Y+b.Y, a.Z+b.Z);

	public static Vector3D operator -(Vector3D a, Vector3D b)
		=> new(a.X-b.X, a.Y-b.Y, a.Z-b.Z);

	public static Vector3D operator *(Vector3D vec, double scale)
		=> new(vec.X*scale, vec.Y*scale, vec.Z*scale);

	public static Vector3D operator -(Vector3D a)
		=> new(-a.X, -a.Y, -a.Z);

    public override string ToString()
		=> $"({X}, {Y}, {Z})";
}