using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.NativeInterop;

namespace Serialization;

public class WorkspaceSerializer {
    public static byte[] Magic = {0x7F, (byte)'b', (byte)'a', (byte)'l', (byte)'l', (byte)'s'};
    public static int Version = 3;

    public static void Serialize(Workspace workspace, Writer writer) {
        // header
        writer.WriteBytes(Magic);
        writer.WriteInt32(Version);

        // textures
        writer.WriteInt32(workspace.textureManager.Images.Count);
        foreach (var image in workspace.textureManager.Images) {
            writer.WriteImage(image);
        }
        
        // workspace settings
        writer.WriteDouble(workspace.VisualScale);

        // camera
        writer.WriteVector3(workspace.camera.Position);
        writer.WriteVector3(workspace.camera.Rotation);
        writer.WriteBool(workspace.camera.Behavior == Camera.CameraBehavior.Orbit);

        if (workspace.camera.Behavior == Camera.CameraBehavior.Orbit) {
            var index = workspace.simulation.BodyList
                .FindIndex(x => workspace.bodyMap[x].Mesh == workspace.camera.OrbitSubject);
            writer.WriteInt32(index);
        }

        // bodies
        writer.WriteInt32(workspace.bodyMap.Count);
        foreach (var body in workspace.simulation.BodyList) {
            writer.WriteVector3D(body.Position);
            writer.WriteVector3D(body.Velocity);
            writer.WriteVector3D(body.OldAcceleration);
            writer.WriteVector3D(body.Acceleration);
            writer.WriteDouble(body.Mass);
            writer.WriteDouble(body.Density);
            writer.WriteDouble(body.EnergyLumens);
            writer.WriteString(workspace.bodyMap[body].Image?.Name ?? "");
        }
    }

    public static void Deserialize(Workspace workspace, Reader reader) {
        // header
        if (!reader.ReadBytes(Magic.Length).SequenceEqual(Magic)) {
            throw new Exception("Invalid magic");
        }

        if (reader.ReadInt32() != Version) {
            throw new Exception("Version not matching");
        }

        // textures
        workspace.textureManager.Clear();
        
        int texture_count = reader.ReadInt32();
        for (int i = 0; i < texture_count; i++) {
            var img = reader.ReadImage();
            workspace.textureManager.AddItem(img);
        }
        
        // workspace settings
        workspace.VisualScale = reader.ReadDouble();

        // camera
        workspace.camera.SetToFreecam();
        workspace.camera.Position = reader.ReadVector3();
        workspace.camera.Rotation = reader.ReadVector3();

        bool is_orbit = reader.ReadBool();
        int orbit_index = is_orbit ? reader.ReadInt32() : -1;

        // bodies
        foreach ((_, var spaceobject) in workspace.bodyMap) {
            workspace.RemoveChild(spaceobject.Mesh);
            spaceobject.Mesh.QueueFree();
        }

        workspace.bodyMap = new();
        workspace.simulation = new();

        var count = reader.ReadInt32();
        for (int i = 0; i < count; i++) {
            var body = new Simulation.Body {
                Position = reader.ReadVector3D(),
                Velocity = reader.ReadVector3D(),
                OldAcceleration = reader.ReadVector3D(),
                Acceleration = reader.ReadVector3D(),
                Mass = reader.ReadDouble(),
                Density = reader.ReadDouble(),
                EnergyLumens = reader.ReadDouble()
            };

            workspace.AddBody(body);
            
            var textureName = reader.ReadString();
            if (!string.IsNullOrEmpty(textureName)) {
                var img = workspace.textureManager.Images.First(x => x.Name == textureName);
                workspace.bodyMap[body].SetTexture(img);
            }

            workspace.bodyMap[body].Sync(workspace.VisualScale);

            if (is_orbit && i == orbit_index) {
                workspace.camera.SetToOrbit(workspace.bodyMap[body].Mesh);
            }
        }
    }
}