using Godot;
using System;

public class SceneHelper
{
    public static void SwitchToScene(SceneTree tree, Node scene)
    {
        tree.Root.AddChild(scene);
        tree.CurrentScene.QueueFree();
        tree.CurrentScene = scene;
    }
}
