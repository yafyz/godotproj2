using Godot;
using System;

public partial class CameraSensitivity : HBoxContainer
{
	private Label PercentLabel;
	private HSlider Slider;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PercentLabel = GetNode<Label>("Percent");
		Slider = GetNode<HSlider>("HSlider");

		Slider.ValueChanged += SliderValueChanged;
		
		UpdateLabel();
		UpdateSlider();
	}

	void UpdateLabel() => PercentLabel.Text = string.Format("{0:F2}%",SettingsManager.Settings.MouseSensitivityRatio * 100);
	void UpdateSlider() => Slider.Value = SettingsManager.Settings.MouseSensitivityRatio * 100;

	void SliderValueChanged(double value)
	{
		SettingsManager.Settings.MouseSensitivityRatio = (float)(Slider.Value / 100d);
		UpdateLabel();
	}
}
