using System.Linq;
using Godot;

public partial class GlobalCommands : Node
{
    private Console _console;
    
    public void Register(Console console)
    {
        _console = console;
        
        console.AddCommand("wsnew", NewWorkspace);
        console.AddCommand("menu", GotoMenu);
        console.AddCommand("exit", Exit);
        console.AddCommand("logs", OpenLogs);
        console.AddCommand("help", PrintHelp);
    }

    void NewWorkspace() {
        GetTree().ChangeSceneToFile(Constants.Scenes.Workspace);
    }

    void GotoMenu() {
        GetTree().ChangeSceneToFile(Constants.Scenes.MainMenu);
    }

    void Exit() {
        QuitHelper.Quit(GetTree());
    }

    void OpenLogs() {
        _console.ShowLogs();
    }

    string PrintHelp()
    {
        OpenLogs();
        var e = _console.Commands.Select(x => $"[color=#FFFFFF]{x.Key}[/color] {
            string.Join(" ", x.Value.Arguments.Select(y => $"[color=#CCCCCC]{y.Name}[/color]: [color=#AAAAAA]{y.ValueType}[/color]"))
        }");
        return string.Join("\n",e);
    }
}