using Godot;

public partial class GlobalCommands : Node {
    public void Register(Console console) {
        console.AddCommand("wsnew", NewWorkspace);
        console.AddCommand("menu", GotoMenu);
        console.AddCommand("exit", Exit);
    }

    void NewWorkspace() {
        GetTree().ChangeSceneToFile(Constants.Scenes.Workspace);
    }

    void GotoMenu() {
        GetTree().ChangeSceneToFile(Constants.Scenes.MainMenu);
    }

    void Exit() {
        GetTree().Quit();
    }
}