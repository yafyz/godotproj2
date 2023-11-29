using Godot;
using System;

public partial class SpawnerGUI : Panel
{
	static double ParseDouble(string str) => double.Parse(str.Replace(".", ","));

	Workspace workspace;

	struct Vector3TextBoxes {
		public LineEdit X,Y,Z;
		public Vector3TextBoxes(Control parent) {
			X = parent.GetNode<LineEdit>("X");
			Y = parent.GetNode<LineEdit>("Y");
			Z = parent.GetNode<LineEdit>("Z");
		}

		public Vector3D GetValue() {
			return new Vector3D(ParseDouble(X.Text), ParseDouble(Y.Text), ParseDouble(Z.Text));
		}

		public void Reset() {
			X.Text = Y.Text = Z.Text = "";
		}
	}

	Vector3TextBoxes Position_input;
	Vector3TextBoxes Velocity_input;
	LineEdit Mass_input;
	Button Spawn_button;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		workspace = GetNode<Workspace>("/root/Workspace");
		Position_input = new(GetNode<Control>("Position"));
		Velocity_input = new(GetNode<Control>("Velocity"));
		Mass_input = GetNode("Mass").GetNode<LineEdit>("Mass");
		Spawn_button = GetNode<Button>("Spawn");

		Spawn_button.Pressed += SpawnButtonClicked;
	}

    public override void _Process(double delta)
    {
		Position = Position with {Y = GetWindow().Size.Y-Size.Y};
    }

    public void SpawnButtonClicked() {
		Vector3D position = Position_input.GetValue();
		Vector3D velocity = Velocity_input.GetValue();
		double mass = ParseDouble(Mass_input.Text);

		workspace.AddBody(position, velocity, mass);

		Position_input.Reset();
		Velocity_input.Reset();
		Mass_input.Text = "";
	}
}
