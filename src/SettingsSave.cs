using System;
using System.IO;
using Godot;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class SettingsSave : Node
{
	private JsonSerializerOptions _serializerOptions = new() {
		WriteIndented = true,
		Converters = { new GodotStringNameJsonConverter() }
	};
	
	public override void _Ready()
	{
		SettingsManager.Settings = LoadSettings();
		GetTree().AutoAcceptQuit = false;
	}

	public SettingsManager._Settings LoadSettings()
	{
		if (File.Exists(Constants.SettingsFile)) {
			try {
				var contents = File.ReadAllText(Constants.SettingsFile);
				var settings = JsonSerializer.Deserialize<SettingsManager._Settings>(contents, _serializerOptions);
				return SetKeybinds(settings);
			} catch (Exception e) {
				GD.PrintErr("settings load error", e);
			}
		}
		return SetKeybinds(new SettingsManager._Settings());
	}
	
	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			string contents = JsonSerializer.Serialize(SettingsManager.Settings, _serializerOptions);
			File.WriteAllText(Constants.SettingsFile, contents);
			GetTree().Quit();
		}
	}

	private SettingsManager._Settings SetKeybinds(SettingsManager._Settings settings)
	{
		if (settings.Keybinds == null) {
			settings.Keybinds = new();
			foreach ((var name, _) in KeybindsSettings.NameMap) {
				var evts = InputMap.ActionGetEvents(name);
				settings.Keybinds[name] = ((InputEventKey)evts[0]).GetPhysicalKeycodeWithModifiers();
			}
		} else {
			foreach ((var name, var key) in settings.Keybinds) {
				var evt = new InputEventKey();
				evt.PhysicalKeycode = key;
				
				InputMap.ActionEraseEvents(name);
				InputMap.ActionAddEvent(name, evt);
			}
		}

		return settings;
	}
	
	public class GodotStringNameJsonConverter : JsonConverter<StringName>
	{
		public override StringName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> reader.GetString();

		public override void Write(Utf8JsonWriter writer, StringName value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value);

		public override StringName ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> reader.GetString();

		public override void WriteAsPropertyName(Utf8JsonWriter writer, StringName value, JsonSerializerOptions options)
			=> writer.WritePropertyName(value);
	}
}
