using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class SettingsManager
{
	public static _Settings Settings;
	public struct _Settings
	{
		public _Settings() {}
		
		// Pixels per 1 rotation
		[JsonIgnore]
		public float MouseSensitivity => (float)(2*Math.PI/1000)*MouseSensitivityRatio;
	
		public float MouseSensitivityRatio { get; set; } = 1;
	
		public float CameraScrollSpeed { get; set; } = 0.5f;
	
		public float CameraFlySpeed { get; set; } = 4f;
		public float CameraFlySpeedMultiplier { get; set; } = 4f;

		public Dictionary<StringName, Key> Keybinds { get; set; }
	}
}
