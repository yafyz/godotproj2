using Godot;
using System;
using System.Collections.Generic;

public class Simulation
{
	public static double NewtonGravConstant = 6.67430*Math.Pow(10, -11);

	public List<Body> BodyList;


	public delegate void DOnBodyConsumed(Body consumed, Body consumee);

	public event DOnBodyConsumed OnBodyConsumed;
	
	public Simulation() {
		BodyList = new();
	}

	public void RemoveBody(Body body)  {
		BodyList.Remove(body);
	}

	public void AddBody(Body body) {
		BodyList.Add(body);
	}
	
	public void Step(double delta)
	{
		Dictionary<Body, Body> toKill = [];
		
		foreach (var body in BodyList) {
			if (toKill.ContainsKey(body)) continue;
			
			Vector3D acc = new();
			
			foreach (var body2 in BodyList) {
				if (body == body2) continue;
				if (toKill.ContainsKey(body2)) continue;
				
				Vector3D off = body.Position-body2.Position;

				if (off.Length() < body.Radius
				    && body.Radius > body2.Radius)
				{
					body.Mass += body2.Mass;
					body.Velocity += (body2.Velocity * body2.Mass) / body.Mass;
					body.Density += (body2.Mass / (body.Mass + body2.Mass)) * body2.Density;

					toKill[body2] = body;
					
					continue;
				}
				
				double accel = (NewtonGravConstant*body2.Mass)/off.LengthSquared();

				acc += -off.Normalized()*accel;
			}

			body.OldAcceleration = body.Acceleration;
			body.Acceleration = acc;
		}

		foreach (var body in BodyList)
		{
			body.Position += body.Velocity * delta + body.OldAcceleration*(delta*delta)*0.5;
			body.Velocity += (body.Acceleration+body.OldAcceleration)*(delta*0.5);
		}

		foreach (var (body1, body2) in toKill) {
			BodyList.Remove(body1);
			OnBodyConsumed?.Invoke(body1, body2);
		}
	}

	public class Body {
		public Vector3D Position; // meters
		public Vector3D Velocity; // m/s
		public double Mass; // kg
		public double Density; // kg/m^3
		public double EnergyLumens; // lumen
		public double Radius; // meters
		public Vector3D OldAcceleration; // m/s 
		public Vector3D Acceleration; // m/s
		public string Name;
	}
}

public class SpaceObject {
	public MeshInstance3D MeshInstance;
	public SphereMesh Mesh;
	public StaticBody3D Body3D;
	public CollisionShape3D Collider;
	public Simulation.Body SimBody;

	public StandardMaterial3D Material;
	public OmniLight3D Light;
	
	public MeshInstance3D AccelerationArrow;
	public MeshInstance3D VelocityArrow;

	public StoredImage Image;

	public SpaceObject(Simulation.Body simbody, in Workspace.WorkspaceSettings settings) {
		SimBody = simbody;

		Material = new StandardMaterial3D();
		Material.AlbedoColor = new Color(1, 1, 1);
		
		MeshInstance = new MeshInstance3D();
		MeshInstance.Mesh = Mesh = new SphereMesh() {Material = Material};
		
		Body3D = new StaticBody3D();
		MeshInstance.AddChild(Body3D);
		Body3D.Owner = MeshInstance;
		
		Collider = new CollisionShape3D();
		Collider.Shape = new SphereShape3D();

		Body3D.AddChild(Collider);
		Collider.Owner = MeshInstance;

		Light = new OmniLight3D();
		Light.OmniRange = 4096;
		Light.OmniAttenuation = 4;
		MeshInstance.AddChild(Light);
		
		AccelerationArrow = new MeshInstance3D();
		AccelerationArrow.Mesh = new BoxMesh();
		AccelerationArrow.MaterialOverride = new StandardMaterial3D() {
			AlbedoColor = new(0x00_FF_00_00),
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
		};

		VelocityArrow = new MeshInstance3D();
		VelocityArrow.Mesh = new BoxMesh();
		VelocityArrow.MaterialOverride = new StandardMaterial3D() {
			AlbedoColor = new(0xFF_FF_00_00),
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
		};

		MeshInstance.AddChild(AccelerationArrow);
		MeshInstance.AddChild(VelocityArrow);

		Sync(in settings);
	}

	public void Sync(in Workspace.WorkspaceSettings settings) {
		var pos = (SimBody.Position/settings.VisualScale).ToVector3F();
		MeshInstance.Position = pos;

		double V = SimBody.Mass / SimBody.Density;
		double r = Math.Pow(V / (4d / 3d * Math.PI), 1d / 3d);
		double scaled_r = (float)(r / settings.VisualScale)*settings.PlanetVisualScaleMultiplier;

		SimBody.Radius = r;
		
		Mesh.Radius = (float)scaled_r;
		Mesh.Height = (float)scaled_r*2;

		((SphereShape3D)Collider.Shape).Radius = (float)scaled_r;

		if (SimBody.EnergyLumens > 0) {
			Material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
			Light.LightIntensityLumens = (float)(SimBody.EnergyLumens / Math.Pow(settings.VisualScale, 2));
		} else {
			Material.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
			Light.LightIntensityLumens = 0;
		}
		
		var accel = SimBody.Acceleration.ToVector3F();
		if (settings.ShowDebug && accel.Length() >= 0.0001) {
			AccelerationArrow.Scale = new(0.05f, 0.05f, 1f); //new(0.05f, 0.05f, accel.Length()/(float)scale);
			var point_accel = MeshInstance.Position + accel.Normalized()*(AccelerationArrow.Scale.Z/2+MeshInstance.Scale.Z/2);
			AccelerationArrow.LookAtFromPosition(point_accel, point_accel+accel);
		} else {
			// hack instead of setting Visible
			AccelerationArrow.Scale = new(0, 0, 0);
		}

		var vel = SimBody.Velocity.ToVector3F();
		if (settings.ShowDebug && vel.Length() >= 0.0001) {
			VelocityArrow.Scale = new(0.05f, 0.05f, 1f);; //new(0.05f, 0.05f, vel.Length()/(float)scale);
			var point_vel = MeshInstance.Position + vel.Normalized()*(VelocityArrow.Scale.Z/2+MeshInstance.Scale.Z/2);
			VelocityArrow.LookAtFromPosition(point_vel, point_vel+vel);
		} else {
			VelocityArrow.Scale = new(0, 0, 0);
		}
	}

	public void Free()
	{
		Body3D.QueueFree();
		Collider.QueueFree();
		AccelerationArrow.QueueFree();
		Light.QueueFree();
		VelocityArrow.QueueFree();
		MeshInstance.QueueFree();
	}

	public void SetTexture(StoredImage img)
	{
		Image = img;
		Material.AlbedoTexture = img.Texture;
	}

	public void RemoveTexture()
	{
		Image = null;
		Material.AlbedoTexture = null;
	}
}

public struct Vector3D {
	public double X,Y,Z;

	public Vector3D(double X, double Y, double Z) {
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}

	public static Vector3D FromVector3(Vector3 vec)
		=> new(vec.X, vec.Y, vec.Z);

	public Vector3 ToVector3F() {
		return new Vector3((float)X,(float)Y,(float)Z);
	}

	public double Length() {
		return Math.Sqrt(X*X+Y*Y+Z*Z);
	}

	public double LengthSquared() {
		return X*X+Y*Y+Z*Z;
	}

	public Vector3D Normalized() {
		var res = this;
		var ls = res.Length();

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

	public static Vector3D operator /(Vector3D vec, double value)
		=> new(vec.X/value, vec.Y/value, vec.Z/value);

    public override string ToString()
		=> $"({X}, {Y}, {Z})";
}