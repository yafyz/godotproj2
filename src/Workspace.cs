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

	public EditGui EditGuiO;
	public ObjectDrag ObjectDragger;
	public bool EditMode = false;
	public SpaceObject EditingSpaceObject = null;

	public string SaveFile;

	private SavesManager savesManager;
	private UIFocus uiFocus;

	public TextureManager textureManager;

	public Marker3D spawnMarker;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		savesManager = GetNode<SavesManager>(Constants.Singletons.SavesManager);
		uiFocus = GetNode<UIFocus>(Constants.Singletons.UIFocus);
		
		simulation = new();
		bodyMap = new();
		bodiesContainer = GetNode<Node>("Bodies");
		camera = GetNode<Camera>("Camera3D");
		ObjectDragger = GetNode<ObjectDrag>("drag");
		EditGuiO = GetNode<EditGui>("EditGui");
		textureManager = GetNode<TextureManager>("TextureManager");
		spawnMarker = camera.GetNode<Marker3D>("Marker3D");
		
		EditGuiO.ValueChanged += EditGuiValueChanged;
		ObjectDragger.Dragging += DraggerDragging;
		textureManager.ImageRemoved += OnImageRemoved;
		
		RegisterCommands();

		if (SaveFile != null) {
			savesManager.Load(this, SaveFile);
		}

			foreach ((var body, var node) in bodyMap) {
				node.Sync(VisualScale);
			}
			
			camera.SetToOrbit(bodyMap[earth].Body3D);
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (!TimeFrozen && !EditMode) {
			var d = delta * Timescale + PhysicsRateRemainder;
			PhysicsRateRemainder = d % PhysicsRate;

			for (int i = 0; i < d/PhysicsRate; i++) {
				simulation.Step(PhysicsRate);
			}

			foreach ((var body, var node) in bodyMap) {
				node.Sync(VisualScale);
			}
			
			EditGuiO.Refresh();
		}

		/* Body selector for orbit cam */

		var window = GetViewport();
		var world3d = window.World3D;
		var space_state = world3d.DirectSpaceState;
		if (space_state != null) {
			var mpos = window.GetMousePosition();

			if (EditGuiO.CheckMouse(mpos))
				return;
			
			var ray_start = camera.ProjectRayOrigin(mpos);
			var ray_end = ray_start + camera.ProjectRayNormal(mpos) * 1000;

			var query = PhysicsRayQueryParameters3D.Create(ray_start, ray_end, 1);
			var result = space_state.IntersectRay(query);

			if (!EditMode && Input.IsActionJustPressed(Constants.KeyBindings.LMB)) {
				if (result.Any()) {
					var obj = result["collider"].As<StaticBody3D>();
					var so = bodyMap
						.Select(kv => kv.Value)
						.FirstOrDefault(so => so.Body3D == obj);
					
					if (so != null) {
						camera.SetToOrbit(obj.Owner as Node3D);
						EditGuiO.ShowGui(so);
					}
				} else {
					camera.SetToFreecam();
					EditGuiO.HideGui();
				}
			} else if (Input.IsActionJustPressed(Constants.KeyBindings.EditMode) && !uiFocus.IsFocused) {
				if (result.Any()) {
					var obj = result["collider"].As<StaticBody3D>();
					var so = bodyMap
						.Select(kv => kv.Value)
						.FirstOrDefault(x => x.Body3D == obj);
					if (so != null) {
						EnterEditMode(so);
					} else {
						QuitEditMode();
					}
				} else {
					QuitEditMode();
					EditGuiO.HideGui();
				}
			}
		}

		if (Input.IsActionJustPressed(Constants.KeyBindings.OpenSpawnMenu)) {
			var body = AddBody(Vector3D.FromVector3(spawnMarker.GlobalPosition) * VisualScale, new Vector3D(), 1, 1, 0);
			var so = bodyMap[body];
			EnterEditMode(so);
		}
	}

	public void OnImageRemoved(StoredImage image)
	{
		foreach ((_, var so) in bodyMap) {
			if (so.Image == image) {
				so.RemoveTexture();
				if (EditGuiO.Object == so) {
					EditGuiO.Refresh();
				}
			}
		}
	}
	
	public void EditGuiValueChanged()
	{
		EditGuiO.Object.Sync(VisualScale);
	}

	public void DraggerDragging()
	{
		EditingSpaceObject.SimBody.Position = Vector3D.FromVector3(EditingSpaceObject.Mesh.Position)*VisualScale;
		EditGuiO.Refresh();
	}
	
	public void EnterEditMode(SpaceObject so) {
		if (EditMode)
			QuitEditMode();
		EditMode = true;
		EditingSpaceObject = so;
		ObjectDragger.SetDrag(so.Mesh);
		camera.SetToFreecam();
		EditGuiO.ShowGui(so);
	}

	public void QuitEditMode() {
		if (!EditMode)
			return;
		var newPos = Vector3D.FromVector3(EditingSpaceObject.Mesh.Position)*VisualScale;
		EditingSpaceObject.SimBody.Position = newPos;
		EditMode = false;
		EditingSpaceObject = null;
		ObjectDragger.SetDrag(null);
		EditGuiO.HideGui();
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
		
		console.AddCommand("addtexture", (string name) => {
				var img = textureManager.Images.FirstOrDefault(x => x.Name == name);
				if (img == null || EditingSpaceObject == null) return;
				EditingSpaceObject.SetTexture(img);
			}, [new("image_name", typeof(string), null, ((cmd, argv, argi) => {
					if (!EditMode)
						return new Console.HinterResult(Console.HinterInputResult.Error, ["you must be in edit mode"]);
					var matches = textureManager.Images
						.Where(x => x.Name.StartsWith(argv[argi]))
						.Select(x => x.Name[argv[argi].Length..]);
					return matches.Any()
						? new Console.HinterResult(Console.HinterInputResult.Hint, matches)
						: new Console.HinterResult(Console.HinterInputResult.Error, ["no texture with such name found"]);
				}
		))]);
		
		console.AddCommand("removetexture", () => {
			if (EditingSpaceObject == null) return;
			EditingSpaceObject.RemoveTexture();
		});
	}

	public Simulation.Body AddBody(Vector3D position, Vector3D velocity, double mass, double density, double energy) {
		Simulation.Body body = new Simulation.Body() {
			Position = position,
			Velocity = velocity,
			Mass = mass,
			Density = density,
			EnergyLumens = energy
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
