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
	
	public EditGui EditGuiO;
	public ObjectDrag ObjectDragger;
	public bool EditMode = false;
	public SpaceObject EditingSpaceObject = null;

	public EscMenu escMenu;

	public string SaveFile;

	private SavesManager savesManager;
	private UIFocus uiFocus;

	public TextureManager textureManager;

	public Marker3D spawnMarker;
	private Console console;

	public WorkspaceSettings Settings = new();

	private JumpToObject _jumpToObject;
	private LoadError _loadError;
	
	public struct WorkspaceSettings
	{
		private bool __TimeFrozen = false;
		private double __Timescale = 1;
		private double __PhysicsRate = 0.01f;
		public double PhysicsRateRemainder = 0;
		private double __VisualScale = 1;
		private double __planetVisualScaleMultiplier = 1;
		
		private bool __ShowDebug = false;

		public WorkspaceSettings() {}

		public bool TimeFrozen {
			get => __TimeFrozen;
			set {
				var old = __TimeFrozen;
				__TimeFrozen = value;
				PropertyChanged?.Invoke("TimeFrozen", old);
			}
		}
		
		public double Timescale {
			get => __Timescale;
			set {
				var old = __Timescale;
				__Timescale = value;
				PropertyChanged?.Invoke("Timescale", old);
			}
		}
		
		public double PhysicsRate {
			get => __PhysicsRate;
			set {
				var old = __PhysicsRate;
				__PhysicsRate = value;
				PropertyChanged?.Invoke("PhysicsRate", old);
			}
		}
		
		public double VisualScale {
			get => __VisualScale;
			set {
				var old = __VisualScale;
				__VisualScale = value;
				PropertyChanged?.Invoke("VisualScale", old);
			}
		}
		
		public bool ShowDebug {
			get => __ShowDebug;
			set {
				var old = __ShowDebug; 
				__ShowDebug = value;
				PropertyChanged?.Invoke("ShowDebug", old);
			}
		}
		
		public double PlanetVisualScaleMultiplier {
			get => __planetVisualScaleMultiplier;
			set {
				var old = __planetVisualScaleMultiplier; 
				__planetVisualScaleMultiplier = value;
				PropertyChanged?.Invoke("PlanetVisualScaleMultiplier", old);
			}
		}
		
		public event Action<string, object> PropertyChanged;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		savesManager = GetNode<SavesManager>(Constants.Singletons.SavesManager);
		uiFocus = GetNode<UIFocus>(Constants.Singletons.UIFocus);
		
		console = GetNode<Console>("console");
		
		simulation = new();
		bodyMap = new();
		bodiesContainer = GetNode<Node>("Bodies");
		camera = GetNode<Camera>("Camera3D");
		ObjectDragger = GetNode<ObjectDrag>("drag");
		EditGuiO = GetNode<EditGui>("EditGui");
		textureManager = GetNode<TextureManager>("TextureManager");
		spawnMarker = camera.GetNode<Marker3D>("Marker3D");
		escMenu = GetNode<EscMenu>("Menu");
		_jumpToObject = GetNode<JumpToObject>("JumpToObject");
		_loadError = GetNode<LoadError>("LoadError");
		
		simulation.OnBodyConsumed += OnBodyConsumed;
		
		EditGuiO.ValueChanged += EditGuiValueChanged;
		ObjectDragger.Dragging += DraggerDragging;
		textureManager.ImageRemoved += OnImageRemoved;
		
		Settings.PropertyChanged += SettingsOnPropertyChanged;
		
		RegisterCommands();

		if (SaveFile != null) {
			try {
				savesManager.Load(this, SaveFile);
			} catch (Exception e) {
				_loadError.ShowError(SaveFile, e);	
			}
		}
			
	}

	public bool CheckMouse(Vector2 mpos)
	{
		return EditGuiO.CheckMouse(mpos)
		       || escMenu.CheckMouse(mpos)
		       || textureManager.CheckMouse(mpos)
		       || console.CheckMouse(mpos)
		       || _jumpToObject.CheckMouse(mpos);
	}

	public void ResyncBodies()
	{
		foreach ((var body, var node) in bodyMap) {
			node.Sync(in Settings);
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (!Settings.TimeFrozen && !EditMode) {
			var d = delta * Settings.Timescale + Settings.PhysicsRateRemainder;
			Settings.PhysicsRateRemainder = d % Settings.PhysicsRate;

			for (int i = 0; i < d/Settings.PhysicsRate; i++) {
				simulation.Step(Settings.PhysicsRate);
			}

			foreach ((var body, var node) in bodyMap) {
				node.Sync(in Settings);
			}
			
			EditGuiO.Refresh();
		}

		/* Body selector for orbit cam */

		var window = GetViewport();
		var world3d = window.World3D;
		var space_state = world3d.DirectSpaceState;
		if (space_state != null) {
			var mpos = window.GetMousePosition();

			if (CheckMouse(mpos))
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
						FocusSpaceObject(so);
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
	}

	public void FocusSpaceObject(SpaceObject so, float scrollSpeed = -1, float distance = -1, bool adjustAngle = true)
	{
		if (scrollSpeed == -1) {
			scrollSpeed = camera.CalculateScrollSpeed(so.Mesh.Radius);
		}
		camera.SetToOrbit(so.MeshInstance, scrollSpeed, distance, adjustAngle);
		EditGuiO.ShowGui(so);
	}
	
	private void SettingsOnPropertyChanged(string prop, object old)
	{
		if (prop is "VisualScale" or "PlanetVisualScaleMultiplier") {
			ResyncBodies();
			
			if (camera.Behavior == Camera.CameraBehavior.Orbit) {
				var dist = camera.orbitDistance * prop switch {
					"VisualScale" => (float)((double)old/Settings.VisualScale),
					"PlanetVisualScaleMultiplier" => (float)(Settings.PlanetVisualScaleMultiplier/(double)old),
					_ => throw new Exception("unreachable!")
				};
				
				FocusSpaceObject(
					EditGuiO.Object,
					distance: dist,
					adjustAngle: false
				);
			}
		}
	}
	
	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event is not InputEventKey evt)
			return;

		if (evt.IsActionPressed(Constants.KeyBindings.OpenSpawnMenu)) {
			GetViewport().SetInputAsHandled();
			var body = AddBody($"Object {bodyMap.Count+1}",Vector3D.FromVector3(spawnMarker.GlobalPosition) * Settings.VisualScale, new Vector3D(), 1, 1, 0);
			var so = bodyMap[body];
			EnterEditMode(so);
		} else if (evt.IsActionPressed(Constants.KeyBindings.ShowDebug)) {
			GetViewport().SetInputAsHandled();
			Settings.ShowDebug = !Settings.ShowDebug;
			ResyncBodies();
		} else if (evt.IsActionPressed(Constants.KeyBindings.FreezeTime)) {
			GetViewport().SetInputAsHandled();
			Settings.TimeFrozen = !Settings.TimeFrozen;
		} else if (evt.IsActionPressed(Constants.KeyBindings.JumpToObject)) {
			GetViewport().SetInputAsHandled();
			_jumpToObject.ShowGui();
		}
	}

	public void OnBodyConsumed(Simulation.Body body1, Simulation.Body body2)
	{
		var so1 = bodyMap[body1];
		var so2 = bodyMap[body2];
		
		if (camera.OrbitSubject == so1.MeshInstance) {
			var dist = Math.Max(camera.orbitDistance, camera.CalculateOrbitDistance(so2.Mesh.Radius));
			
			FocusSpaceObject(so2, distance: dist);
		}
		
		bodyMap.Remove(body1);
		so1.Free();
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
		EditGuiO.Object.Sync(in Settings);
		camera.ScrollSpeed = camera.CalculateScrollSpeed(EditGuiO.Object.Mesh.Radius);
	}

	public void DraggerDragging()
	{
		EditingSpaceObject.SimBody.Position = Vector3D.FromVector3(EditingSpaceObject.MeshInstance.Position)*Settings.VisualScale;
		EditGuiO.Refresh();
	}
	
	public void EnterEditMode(SpaceObject so) {
		if (EditMode)
			QuitEditMode();
		EditMode = true;
		EditingSpaceObject = so;
		ObjectDragger.SetDrag(so.MeshInstance);
		camera.SetToFreecam();
		EditGuiO.ShowGui(so);
	}

	public void QuitEditMode() {
		if (!EditMode)
			return;
		var newPos = Vector3D.FromVector3(EditingSpaceObject.MeshInstance.Position)*Settings.VisualScale;
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

		console.AddCommand("freeze", (bool s) => Settings.TimeFrozen = s,
			new Console.CommandArgument[] {
				new(
					"frozen",
					typeof(bool),
					s => bool.Parse(s),
					(_, argv, argi) => new(bool.TryParse(argv[argi], out var res) ? Console.HinterInputResult.Ok : Console.HinterInputResult.Error)
				)
			}
		);

		console.AddCommand("timescale", (float s) => Settings.Timescale = s,
			new Console.CommandArgument[] {
				new(
					"scale",
					typeof(float),
					s => float.Parse(s),
					(_, argv, argi) => new(float.TryParse(argv[argi], out var res) ? Console.HinterInputResult.Ok : Console.HinterInputResult.Error)
				)
			}
		);

		console.AddCommand("physicsrate", (double s) => Settings.PhysicsRate = s,
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
				Settings.VisualScale = s;
				ResyncBodies();
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
		
		console.AddCommand("planetscale",
			(double s) => {
				Settings.PlanetVisualScaleMultiplier = s;
				ResyncBodies();
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

	public Simulation.Body AddBody(string name, Vector3D position, Vector3D velocity, double mass, double density, double energy) {
		Simulation.Body body = new Simulation.Body() {
			Name = name,
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
		var obj = new SpaceObject(body, in Settings);

		bodiesContainer.AddChild(obj.MeshInstance);

		bodyMap.Add(body, obj);
		simulation.AddBody(body);
	}
}
