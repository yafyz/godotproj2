using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.NativeInterop;

namespace Serialization;

public class WorkspaceSerializer {
    public static byte[] Magic = {0x7F, (byte)'b', (byte)'a', (byte)'l', (byte)'l', (byte)'s'};
    public static int Version = 5;

    public enum CameraMode
    {
        Freecam,
        Orbit,
        Edit
    }
    
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
        writer.WriteBool(workspace.Settings.TimeFrozen);
        writer.WriteDouble(workspace.Settings.Timescale);
        writer.WriteDouble(workspace.Settings.PhysicsRate);
        writer.WriteDouble(workspace.Settings.PhysicsRateRemainder);
        writer.WriteDouble(workspace.Settings.VisualScale);
        writer.WriteDouble(workspace.Settings.PlanetVisualScaleMultiplier);

        // camera
        writer.WriteVector3(workspace.camera.Position);
        writer.WriteVector3(workspace.camera.Rotation);

        if (workspace.camera.Behavior == Camera.CameraBehavior.Orbit) {
            var index = workspace.simulation.BodyList
                .FindIndex(x => workspace.bodyMap[x].MeshInstance == workspace.camera.OrbitSubject);
            writer.WriteInt32((int)CameraMode.Orbit);
            writer.WriteInt32(index);
        } else if (workspace.EditingSpaceObject != null) {
            var index = workspace.simulation.BodyList
                .FindIndex(x => workspace.bodyMap[x] == workspace.EditingSpaceObject);
            writer.WriteInt32((int)CameraMode.Edit);
            writer.WriteInt32(index);
        } else {
            writer.WriteInt32((int)CameraMode.Freecam);
        }

        // bodies
        writer.WriteInt32(workspace.bodyMap.Count);
        foreach (var body in workspace.simulation.BodyList) {
            writer.WriteString(body.Name);
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
        workspace.Settings.TimeFrozen = reader.ReadBool();
        workspace.Settings.Timescale = reader.ReadDouble();
        workspace.Settings.PhysicsRate = reader.ReadDouble();
        workspace.Settings.PhysicsRateRemainder = reader.ReadDouble();
        workspace.Settings.VisualScale = reader.ReadDouble();
        workspace.Settings.PlanetVisualScaleMultiplier = reader.ReadDouble();

        // camera
        workspace.camera.SetToFreecam();
        workspace.camera.Position = reader.ReadVector3();
        workspace.camera.Rotation = reader.ReadVector3();

        CameraMode mode = (CameraMode)reader.ReadInt32();
        int orbit_index = mode != CameraMode.Freecam ? reader.ReadInt32() : -1;

        // bodies
        foreach ((_, var spaceobject) in workspace.bodyMap) {
            workspace.RemoveChild(spaceobject.Body3D);
            spaceobject.MeshInstance.QueueFree();
        }

        workspace.bodyMap = new();
        workspace.simulation = new();
        workspace.simulation.OnBodyConsumed += workspace.OnBodyConsumed;

        var count = reader.ReadInt32();
        for (int i = 0; i < count; i++) {
            var body = new Simulation.Body {
                Name = reader.ReadString(),
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

            workspace.bodyMap[body].Sync(in workspace.Settings);

            if (mode != CameraMode.Freecam && i == orbit_index) {
                if (mode == CameraMode.Orbit) {
                    workspace.FocusSpaceObject(workspace.bodyMap[body]);
                } else if (mode == CameraMode.Edit) {
                    workspace.EnterEditMode(workspace.bodyMap[body]);
                }
            }
        }
    }
}