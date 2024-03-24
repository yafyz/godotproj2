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
				
				double fg = NewtonGravConstant*((body.Mass*body2.Mass)/off.LengthSquared());
				double force = fg * (body2.Mass / (body.Mass + body2.Mass));
				double accel = force / body.Mass;
				
				acc += -off.Normalize()*accel;
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
		public double Density;
		public double EnergyLumens;
		public Vector3D OldAcceleration;
		public Vector3D Acceleration;
	}
}

public class SpaceObject {
	public MeshInstance3D Mesh;
	public StaticBody3D Body3D;
	public CollisionShape3D Collider;
	public Simulation.Body SimBody;

	public StandardMaterial3D Material;
	public OmniLight3D Light;
	
	public MeshInstance3D AccelerationArrow;
	public MeshInstance3D VelocityArrow;

	public StoredImage Image;

	public SpaceObject(Simulation.Body simbody, double scale = 1) {
		SimBody = simbody;

		Material = new StandardMaterial3D();
		Material.AlbedoColor = new Color(1, 1, 1);
		//Material.AlbedoTexture = GD.Load<CompressedTexture2D>("res://textures/earth.jpg");
		//Material.AlbedoTexture = ImageTexture.CreateFromImage(new Image()).GetImage().save;
		
		Mesh = new MeshInstance3D();
		Mesh.Mesh = new SphereMesh() {Material = Material};
		
		Body3D = new StaticBody3D();
		Mesh.AddChild(Body3D);
		Body3D.Owner = Mesh;
		
		Collider = new CollisionShape3D();
		Collider.Shape = new SphereShape3D();

		Body3D.AddChild(Collider);
		Collider.Owner = Mesh;

		Light = new OmniLight3D();
		Light.OmniRange = 4096;
		Light.OmniAttenuation = 2;
		Mesh.AddChild(Light);
		
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

		Mesh.AddChild(AccelerationArrow);
		Mesh.AddChild(VelocityArrow);

		Sync(scale);
	}

	public void Sync(double scale) {
		var pos = (SimBody.Position/scale).ToVector3F();
		Mesh.Position = pos;

		double V = SimBody.Mass / SimBody.Density;
		double r = Math.Pow(V / (4d / 3d * Math.PI), 1d / 3d) / 1000;
		double scaled_r = (float)(r / scale) * 100;
		
		SphereMesh mesh = (SphereMesh)Mesh.Mesh;
		mesh.Radius = (float)scaled_r / 2;
		mesh.Height = (float)scaled_r;

		((SphereShape3D)Collider.Shape).Radius = (float)scaled_r / 2;
		Body3D.Scale = Mesh.Scale;


		if (SimBody.EnergyLumens > 0) {
			//Material.EmissionEnabled = true;
			//Material.Emission = new Color(1,1,1);
			//Material.EmissionIntensity = 0.01f;//(float)(SimBody.EnergyLumens / Math.Pow(10, 17+Math.Log10(scale)));
			Material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
			Light.LightIntensityLumens = (float)(SimBody.EnergyLumens / Math.Pow(10, 17+Math.Log10(scale)));
			// 100000
		} else {
			//Material.EmissionEnabled = false;
			Material.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
			Light.LightIntensityLumens = 0;
		}
		
		var accel = SimBody.Acceleration.ToVector3F();
		if (accel.Length() >= 0.0001) {
			AccelerationArrow.Scale = new(0.05f, 0.05f, 1f); //new(0.05f, 0.05f, accel.Length()/(float)scale);
			var point_accel = Mesh.Position + accel.Normalized()*(AccelerationArrow.Scale.Z/2+Mesh.Scale.Z/2);
			AccelerationArrow.LookAtFromPosition(point_accel, point_accel+accel);
		} else {
			// hack instead of setting Visible so i dont have to set Visible above
			AccelerationArrow.Scale = new(0.05f, 0.05f, 0);
		}

		var vel = SimBody.Velocity.ToVector3F();
		if (vel.Length() >= 0.0001) {
			VelocityArrow.Scale = new(0.05f, 0.05f, 1f);; //new(0.05f, 0.05f, vel.Length()/(float)scale);
			var point_vel = Mesh.Position + vel.Normalized()*(VelocityArrow.Scale.Z/2+Mesh.Scale.Z/2);
			VelocityArrow.LookAtFromPosition(point_vel, point_vel+vel);
		} else {
			VelocityArrow.Scale = new(0.05f, 0.05f, 0);
		}
	}

	public void SetTexture(StoredImage img)
	{
		Image = img;
		Material.AlbedoTexture = ImageTexture.CreateFromImage(img.Image);
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

	public static Vector3D operator /(Vector3D vec, double value)
		=> new(vec.X/value, vec.Y/value, vec.Z/value);

    public override string ToString()
		=> $"({X}, {Y}, {Z})";
}