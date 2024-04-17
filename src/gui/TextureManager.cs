using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class TextureManager : Panel
{
	private ItemList TextureList;
	private Button LoadButton;
	private Button DeleteButton;
	private Button CloseButton;
	private FileDialog FileDialog;
	private AcceptDialog ErrorDialog;

	private SphereMesh Mesh;
	private StandardMaterial3D Material;
    
	public List<StoredImage> Images = new();

	public event Action<StoredImage> ImageRemoved;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TextureList = GetNode<ItemList>("TextureList");
		LoadButton = GetNode<Button>("VFlowContainer/Load");
		DeleteButton = GetNode<Button>("VFlowContainer/Delete");
		CloseButton = GetNode<Button>("VFlowContainer/Close");
		FileDialog = GetNode<FileDialog>("FileDialog");
		ErrorDialog = GetNode<AcceptDialog>("AcceptDialog");
		Mesh = (SphereMesh)GetNode<MeshInstance3D>("VFlowContainer/SubViewportContainer/SubViewport/MeshInstance3D").Mesh;
		
		LoadButton.Pressed += LoadButtonClicked;
		DeleteButton.Pressed += DeleteButtonClicked;
		CloseButton.Pressed += CloseButtonPressed;

		FileDialog.Title = "Load a texture";
		FileDialog.Filters = ["*.jpg", "*.png"];
		FileDialog.FileSelected += FileDialogFileSelected;
		
		TextureList.ItemSelected += TextureClicked;

		Mesh.Material = Material = new StandardMaterial3D();
		Material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
	}

	private void TextureClicked(long index)
	{
		Material.AlbedoTexture = null;
		var img = Images[(int)index];
		Material.AlbedoTexture = img.Texture;
	}

	public void LoadButtonClicked()
	{
		FileDialog.PopupCentered();
	}

	public void FileDialogFileSelected(string filePath)
	{
		try {
			string fileName = Path.GetFileName(filePath);

			if (Images.Any(x => x.Name == fileName))
				throw new Exception("texture with such name already exists");
			
			StoredImage.ImageFormat format = Path.GetExtension(filePath).ToLower() switch {
				".jpg" => StoredImage.ImageFormat.JPG,
				".png" => StoredImage.ImageFormat.PNG,
				var ex => throw new Exception($"Unsupported file extension: {ex}")
			};
			
			byte[] content = File.ReadAllBytes(filePath);

			var tx = new StoredImage(fileName, format, content);
			
			AddItem(tx);
		} catch (Exception e) {
			ErrorDialog.DialogText = $"Error while loading texture\n{e.Message}";
			ErrorDialog.PopupCentered();
		}
	}
	
	public void DeleteButtonClicked()
	{
		// hope that they are ordered
		var offset = 0;
		foreach (var idx in TextureList.GetSelectedItems()) {
			var img = Images[idx - offset];
			if (Material.AlbedoTexture == img.Texture)
				Material.AlbedoTexture = null;
			RemoveItem(img);
			offset++;
		}
	}

	public void AddItem(StoredImage img)
	{
		Images.Add(img);
		TextureList.AddItem(img.Name);
	}

	public void RemoveItem(StoredImage img)
	{
		var idx = Images.FindIndex(x => x == img);
		if (idx > -1) {
			Images.RemoveAt(idx);
			TextureList.RemoveItem(idx);
		}
		ImageRemoved?.Invoke(img);
	}

	public void CloseButtonPressed()
	{
		FileDialog.Hide();
		ErrorDialog.Hide();
		Hide();
	}

	public void Clear()
	{
		Images.Clear();
		TextureList.Clear();
	}

	public bool CheckMouse(Vector2 mpos)
	{
		return Visible && GetGlobalRect().HasPoint(mpos)
			|| WindowHelper.WindowHasMouse(FileDialog, mpos)
			|| WindowHelper.WindowHasMouse(ErrorDialog, mpos);
	}
}

public class StoredImage
{
	public enum ImageFormat
	{
		PNG, JPG
	}
	
	public string Name;
	public ImageFormat Format;
	public byte[] RawData;
	
	public Image Image;

	private ImageTexture _texture;
	public ImageTexture Texture => _texture ??= ImageTexture.CreateFromImage(Image);

	public StoredImage(string name, ImageFormat imageFormat, byte[] rawData)
	{
		Name = name;
		RawData = rawData;
		Format = imageFormat;
		
		Image = new Image();

		switch (Format) {
			case ImageFormat.JPG:
				Image.LoadJpgFromBuffer(RawData);
				break;
			case ImageFormat.PNG:
				Image.LoadPngFromBuffer(RawData);
				break;
			default:
				throw new Exception($"Unsupported format: {Format}");
		}
	}
}