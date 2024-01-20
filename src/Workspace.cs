using Godot;
using Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Workspace : Node3D
{
	public Simulation simulation;
	public Dictionary<Simulation.Body, SpaceObject> bodyMap;
	public Node bodiesContainer;
	public Camera camera;
	public bool TimeFrozen = false;
	public float Timescale = 1;
	public double PhysicsRate = 0.01f;
	public double PhysicsRateRemainder = 0;
	public double VisualScale = 1;

	public string SaveFile;

	SavesManager savesManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		savesManager = GetNode<SavesManager>(Constants.Singletons.SavesManager);

		simulation = new();
		bodyMap = new();
		bodiesContainer = GetNode<Node>("Bodies");
		camera = GetNode<Camera>("Camera3D");

		(GetNode<MeshInstance3D>("MeshInstance3D").Mesh as TextMesh)
			.Text = "balls";

		RegisterCommands();

		if (SaveFile != null) {
			savesManager.Load(this, SaveFile);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{

		if (!TimeFrozen) {
			var d = delta * Timescale + PhysicsRateRemainder;
			PhysicsRateRemainder = d % PhysicsRate;

			for (int i = 0; i < d/PhysicsRate; i++) {
				simulation.Step(PhysicsRate);
			}

			foreach ((var body, var node) in bodyMap) {
				node.Sync(VisualScale);
			}
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

	public void Save(string filename = null) {
		filename ??= SaveFile;
		savesManager.Save(this, filename);
		SaveFile = filename;
	}

	public void Load(string filename = null) {
		filename ??= SaveFile;
		savesManager.Load(this, filename);
		SaveFile = filename;
	}

	public void RegisterCommands() {
		Console console = GetNode<Console>("console");

		console.AddCommand("save",
			(string filename) => Save(filename),
			new Console.CommandArgument[] {
			new(
				"filename",
				typeof(string),
				null,
				(_, argv, argi) => {
					var hints = savesManager.Files.Where(x => x.StartsWith(argv[argi]));
					if (hints.Any(x => x == argv[argi])) {
						return new(Console.HinterInputResult.Ok);
					} else {
						return new(Console.HinterInputResult.Hint, hints.Select(x=>x[argv[argi].Length..]));
					}
				}
			)
		});

		console.AddCommand("load",
			(string filename) => Load(filename),
			new Console.CommandArgument[] {
			new(
				"filename",
				typeof(string),
				null,
				(_, argv, argi) => {
					var hints = savesManager.Files.Where(x => x.StartsWith(argv[argi]));
					if (hints.Any(x => x == argv[argi])) {
						return new(Console.HinterInputResult.Ok);
					} else if (hints.Any()) {
						return new(Console.HinterInputResult.Hint, hints.Select(x=>x[argv[argi].Length..]));
					} else {
						return new(Console.HinterInputResult.Error, new string[] {"no such file exists"});
					}
				}
			)
		});

		console.AddCommand("freeze", (bool s) => TimeFrozen = s,
			new Console.CommandArgument[] {
				new(
					"frozen",
					typeof(bool),
					s => bool.Parse(s),
					(_, argv, argi) => new(bool.TryParse(argv[argi], out var res) ? Console.HinterInputResult.Ok : Console.HinterInputResult.Error)
				)
			}
		);

		console.AddCommand("timescale", (float s) => Timescale = s,
			new Console.CommandArgument[] {
				new(
					"scale",
					typeof(float),
					s => float.Parse(s),
					(_, argv, argi) => new(float.TryParse(argv[argi], out var res) ? Console.HinterInputResult.Ok : Console.HinterInputResult.Error)
				)
			}
		);

		console.AddCommand("physicsrate", (double s) => PhysicsRate = s,
			new Console.CommandArgument[] {
				new(
					"rate",
					typeof(double),
					s => double.Parse(s),
					(_, argv, argi) => new(double.TryParse(argv[argi], out var res) ? Console.HinterInputResult.Ok : Console.HinterInputResult.Error)
				)
			}
		);

		console.AddCommand("scale",
			(double s) => {
				VisualScale = s;
				foreach ((var body, var node) in bodyMap) {
					node.Sync(VisualScale);
				}
			},
			new Console.CommandArgument[] {
				new(
					"scale",
					typeof(double),
					s => double.Parse(s),
					(_, argv, argi) => new(double.TryParse(argv[argi], out var res) ? Console.HinterInputResult.Ok : Console.HinterInputResult.Error)
				)
			}
		);
	}

	public Simulation.Body AddBody(Vector3D position, Vector3D velocity, double mass) {
		Simulation.Body body = new Simulation.Body() {
			Position = position,
			Velocity = velocity,
			Mass = mass
		};

		AddBody(body);

		return body;
	}

	public void AddBody(Simulation.Body body) {
		var obj = new SpaceObject(body, VisualScale);

		bodiesContainer.AddChild(obj.Mesh);

		bodyMap.Add(body, obj);
		simulation.AddBody(body);
	}
}
