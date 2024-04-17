using Godot;
using System;
using System.Linq;

public partial class EditGui : PanelContainer	
{
	private Workspace workspace;

	private VBoxContainer Container;
	
	private Vector3DControl PositionControl;
	//private Vector3DControl VelocityControl;
	private VelocityControl VelocityControl_;

	private DoubleInput MassControl;
	private DoubleInput DensityControl;
	private DoubleInput EnergyControl;

	private LineEdit CurrentTexture;
	private Button SetTexture;
	private Button RemoveTexture;
	
	private Window TextureSelectWindow;
	private ItemList TextureSelectList;
	private Button TextureSelectButton;

	private LineEdit _nameInput;
	
	public SpaceObject Object;
	public event Action ValueChanged;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		workspace = GetParent<Workspace>();

		Container = GetNode<VBoxContainer>("VBoxContainer");
					
		PositionControl = Container.GetNode<Vector3DControl>("PositionControl/Position");
		//VelocityControl = Container.GetNode<Vector3DControl>("VelocityControl/Velocity");
		VelocityControl_ = Container.GetNode<VelocityControl>("VelocityControl");
		
		MassControl = Container.GetNode<DoubleInput>("MassControl/Mass");
		DensityControl = Container.GetNode<DoubleInput>("DensityControl/Density");
		EnergyControl = Container.GetNode<DoubleInput>("EnergyControl/Energy");

		CurrentTexture = Container.GetNode<LineEdit>("TextureControl/CurrentTexture");
		SetTexture = Container.GetNode<Button>("TextureControl/SetTexture");
		RemoveTexture = Container.GetNode<Button>("TextureControl/RemoveTexture");
		
		TextureSelectWindow = GetNode<Window>("TextureSelect");
		TextureSelectList = GetNode<ItemList>("TextureSelect/TextureList");
		TextureSelectButton = GetNode<Button>("TextureSelect/SelectTexture");

		_nameInput = GetNode<LineEdit>("VBoxContainer/NameControl/LineEdit");
		
		TextureSelectWindow.CloseRequested += TextureSelectWindowCloseRequested;
		TextureSelectButton.Pressed += TextureSelectSelected;
			
		PositionControl.ValueChanged += pos => Object.SimBody.Position = pos;
		VelocityControl_.ValueChanged += vel => Object.SimBody.Velocity = vel;
		MassControl.ValueChanged += mass => Object.SimBody.Mass = mass;
		DensityControl.ValueChanged += dens => Object.SimBody.Density = dens;
		EnergyControl.ValueChanged += lum => Object.SimBody.EnergyLumens = lum;

		RemoveTexture.Pressed += RemoveTextureClicked;
		SetTexture.Pressed += SetTextureClicked;
		
		PositionControl.ValueChanged += _ => ValueChanged?.Invoke();
		VelocityControl_.ValueChanged += _ => ValueChanged?.Invoke();
		MassControl.ValueChanged += _ => ValueChanged?.Invoke();
		DensityControl.ValueChanged += _ => ValueChanged?.Invoke();
		EnergyControl.ValueChanged += _ => ValueChanged?.Invoke();
		
		_nameInput.TextChanged += NameInputOnTextChanged;
		
		if (Object != null)
			ShowGui(Object);
		else
			HideGui();
	}

	private void NameInputOnTextChanged(string newtext)
	{
		Object.SimBody.Name = newtext;
	}

	private void RemoveTextureClicked()
	{
		Object.RemoveTexture();
		Refresh();
	}

	private void SetTextureClicked()
	{
		TextureSelectList.Clear();
		foreach (var img in workspace.textureManager.Images) {
			TextureSelectList.AddItem(img.Name);
		}
		TextureSelectWindow.PopupCentered();
	}

	private void TextureSelectWindowCloseRequested()
	{
		TextureSelectWindow.Hide();
	}

	private void TextureSelectSelected()
	{
		var items = TextureSelectList.GetSelectedItems();
		if (items.Length < 1) {
			Object.RemoveTexture();
		} else {
			var idx = items.First();
			var name = TextureSelectList.GetItemText(idx);
			var img = workspace.textureManager.Images.FirstOrDefault(x => x.Name == name);
			if (img != null) {
				Object.SetTexture(img);	
			} else {
				Object.RemoveTexture();
			}
		}
		TextureSelectWindow.Hide();
		Refresh();
	}
	
	public void Refresh()
	{
		if (Object == null)
			return;
		
		PositionControl.SetValue(Object.SimBody.Position, false);
		VelocityControl_.SetValue(Object.SimBody.Velocity, false);
		MassControl.SetValue(Object.SimBody.Mass, false);
		DensityControl.SetValue(Object.SimBody.Density, false);
		EnergyControl.SetValue(Object.SimBody.EnergyLumens, false);

		CurrentTexture.Text = Object.Image?.Name ?? "None";
	}
	
	public void ShowGui(SpaceObject obj)
	{
		Object = obj;
		_nameInput.Text = obj.SimBody.Name;
		Visible = true;
		Refresh();
	}

	public void HideGui()
	{
		Object = null;
		Visible = false;
	}
	
	public bool CheckMouse(Vector2 point)
	{
		return Visible
		       && GetGlobalRect().HasPoint(point)
		       || WindowHelper.WindowHasMouse(TextureSelectWindow, point);
	}
}
