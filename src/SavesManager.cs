using System;
using System.IO;
using System.Linq;
using System.Threading;
using Godot;
using Serialization;

public partial class SavesManager : Node {
    public const string SavesFolder = "saves";
    public string[] Files = Array.Empty<string>();

    public event Action OnFilesChanged;

    public override void _Ready()
    {
        if (!Directory.Exists(SavesFolder))
            Directory.CreateDirectory(SavesFolder);

        new Thread(() => {
            while (true) {
                var oldFiles = Files;

                Files = Directory.GetFiles(SavesFolder)
                    .Select(x => Path.GetFileName(x))
                    .ToArray();

                if (!oldFiles.All(x => Files.Contains(x))) {
                    OnFilesChanged?.Invoke();
                }

                Thread.Sleep(1000);
            }
        }).Start();
    }

    public void Save(Workspace workspace, string filename) {
        using var s = File.Open($"{SavesFolder}/{filename}", FileMode.OpenOrCreate);
        WorkspaceSerializer.Serialize(workspace, new Writer(s));
    }

    public void Load(Workspace workspace, string filename) {
        if (!File.Exists($"{SavesFolder}/{filename}"))
            return;
        using var s = File.Open($"{SavesFolder}/{filename}", FileMode.Open);
        WorkspaceSerializer.Deserialize(workspace, new Reader(s));
    }
}