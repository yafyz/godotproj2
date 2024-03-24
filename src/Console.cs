using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

public partial class Console : Control
{
	// Called when the node enters the scene tree for the first time.

	RichTextLabel Label;

	bool isOpen;
	string input;

	Dictionary<string, Command> commands = new();

	private UIFocus uiFocus;
	
	public override void _Ready()
	{
		uiFocus = GetNode<UIFocus>(Constants.Singletons.UIFocus);

		Label = GetNode("Panel").GetNode<RichTextLabel>("RichTextLabel");
		Label.BbcodeEnabled = true;
		SetVisible(false);

		var gcmd = new GlobalCommands();
		AddChild(gcmd);
		gcmd.Register(this);
	}

	public void SetVisible(bool visible) {
		Visible = isOpen = visible;
		uiFocus.ConsoleFocusOverride = visible;
	}

	public void AddCommand(string name, Delegate function, CommandArgument[] arguments = null) {
		AddCommand(new Command(name, function, arguments));
	}

	public void AddCommand(Command command) {
		commands[command.Name] = command;
	}

	public void OpenConsole() {
		SetVisible(true);
		input = "";
		UpdateConsole();
	}

	public void CloseConsole() {
		SetVisible(false);
	}

	// this was written without looking back, how could you tell?
	void UpdateConsole() {
		const string ERR_COLOR = "#FF0000";
		const string OK_COLOR = "#00FF00";
		const string HINTING_COLOR = "#0000FF";
		const string HINT_COLOR = "#999999";
		const string CURRENT_POSITION_INDICATOR = "_";

		string[] argv = input.Split(" ");

		StringBuilder stringBuilder = new();

		stringBuilder.Append('>');

		if (commands.TryGetValue(argv[0], out var cmd)) {
			stringBuilder.Append($"[color={OK_COLOR}]");
			stringBuilder.Append(argv[0]);
			stringBuilder.Append("[/color]");
			if (argv.Length > 1)
				stringBuilder.Append(' ');

			var arg_last = argv.Length-1;
			for (int i = 1; i < arg_last; i++) {
				if (i-1 >= cmd.Arguments.Length) {
					// too many arguments
					stringBuilder.Append($"[color={ERR_COLOR}]");
					stringBuilder.Append(argv[i]);
					stringBuilder.Append("[/color]");
				} else {
					CommandArgument arg = cmd.Arguments[i-1];
					CommandArgument.Hinter hinter = arg.HinterFunc;

					// argument name
					stringBuilder.Append($"[i][color={HINT_COLOR}]");
					stringBuilder.Append(arg.Name);
					stringBuilder.Append(": [/color][/i]");

					// hinter check
					if(hinter != null) {
						if (hinter(cmd, argv, i).InputResult == HinterInputResult.Error) {
							stringBuilder.Append($"[color={ERR_COLOR}]");
							stringBuilder.Append(argv[i]);
							stringBuilder.Append("[/color]");
						} else {
							// argument ok
							stringBuilder.Append(argv[i]);
						}
					} else {
						// argument doesnt have a hinter func
						stringBuilder.Append(argv[i]);
					}
				}
				stringBuilder.Append(' ');
			}

			if (arg_last > 0 && arg_last-1 < cmd.Arguments.Length) {
				// argument name
				stringBuilder.Append($"[i][color={HINT_COLOR}]");
				stringBuilder.Append(cmd.Arguments[arg_last-1].Name);
				stringBuilder.Append(": [/color][/i]");
			}

			// argument is not out of bounds or a space
			if (argv[arg_last] != ""
				&& arg_last > 0
				&& arg_last-1 < cmd.Arguments.Length)
			{
				var hinter = cmd.Arguments[arg_last-1].HinterFunc;
				if (hinter != null) {
					var res = hinter(cmd, argv, arg_last);
					var color = res.InputResult switch {
						HinterInputResult.Ok => OK_COLOR,
						HinterInputResult.Error => ERR_COLOR,
						HinterInputResult.Hint => HINTING_COLOR,
						_ => throw new Exception()
					};

					// argument value
					stringBuilder.Append($"[color={color}]");
					stringBuilder.Append(argv[arg_last]);
					stringBuilder.Append("[/color]");

					// add position arrow
					stringBuilder.Append(CURRENT_POSITION_INDICATOR);

					// hints
					if ((res.InputResult == HinterInputResult.Hint
						|| res.InputResult == HinterInputResult.Error)
						&& res.Hints != null && res.Hints.Any())
					{
						stringBuilder.Append($"[i][color={HINT_COLOR}]");

						// add a space before a hint message for errors
						if (res.InputResult == HinterInputResult.Error)
							stringBuilder.Append(' ');

						stringBuilder.Append(res.Hints.First());
						stringBuilder.Append("[/color][/i]");
					}
				} else {
					// argument doesnt have a hinter func
					stringBuilder.Append(argv[arg_last]);

					stringBuilder.Append(CURRENT_POSITION_INDICATOR);
				}
			} else if (arg_last > 0 && argv[arg_last] != "") {
				// too many arguments, highlight in red
				stringBuilder.Append($"[color={ERR_COLOR}]");
				stringBuilder.Append(argv[arg_last]);
				stringBuilder.Append("[/color]");

				stringBuilder.Append(CURRENT_POSITION_INDICATOR);
			} else if (arg_last > 0) {
				// useless fallback for if last argument is a space
				stringBuilder.Append(argv[arg_last]);

				stringBuilder.Append(CURRENT_POSITION_INDICATOR);
			}

			if (argv.Length-1 > cmd.Arguments.Length) {
				// too many arguments, add a hint message
				stringBuilder.Append($" [i][color={HINT_COLOR}]too many arguments (expected {cmd.Arguments.Length}, got {argv.Length-1})[/color][/i]");
			}
		} else {
			// no command found
			var hint =
				(input != "") ?
					commands
						.Select(x => x.Key)
						.FirstOrDefault(x => x.StartsWith(argv[0]))
					: null;

			if (hint != null) {
				stringBuilder.Append($"[color={HINTING_COLOR}]");
				stringBuilder.Append(input);
				stringBuilder.Append("[/color]");

				stringBuilder.Append(CURRENT_POSITION_INDICATOR);

				stringBuilder.Append($"[i][color={HINT_COLOR}]");
				stringBuilder.Append(hint[input.Length..]);
				stringBuilder.Append("[/color][/i]");
			} else {
				stringBuilder.Append($"[color={ERR_COLOR}]");
				stringBuilder.Append(input);
				stringBuilder.Append("[/color]");

				stringBuilder.Append(CURRENT_POSITION_INDICATOR);
			}
		}

		Label.Text = stringBuilder.ToString();
	}

	void ExecuteCommand(string command) {
		var argv = command.Split(" ");

		if (!commands.TryGetValue(argv[0], out var cmd))
			return;

		if (argv.Length-1 != cmd.Arguments.Length)
			return;

		var args = new object[cmd.Arguments.Length];
		for (int i = 0; i < cmd.Arguments.Length; i++)
			args[i] = cmd.Arguments[i].ParserFunc(argv[i+1]);

		cmd.Call(args);
	}

	void Autocomplete() {
		var argv = input.Split(" ");

		if (argv.Length == 1 && argv[0].Trim() != "") {
			var hint =
					commands
						.Select(x => x.Key)
						.FirstOrDefault(x => x.StartsWith(argv[0]));

			if (hint != null) {
				argv[0] = hint;
				input = string.Join(" ", argv);
			}
		} else if (argv.Length > 1 && commands.TryGetValue(argv[0], out var cmd)) {
			if (argv.Length-1 > cmd.Arguments.Length)
				return;

			var argi = argv.Length-1;

			if (argv[argi].Trim() == "")
				return;

			var arg = cmd.Arguments[argi-1];
			if (arg.HinterFunc == null)
				return;

			var res = arg.HinterFunc(cmd, argv, argi);

			if (res.InputResult == HinterInputResult.Hint && res.Hints.Any()) {
				argv[argi] += res.Hints.First();
				input = string.Join(" ", argv);
			}
		}
	}

	void HandleInput(Key keycode, bool caps) {
		switch (keycode) {
			case Key.Backspace:
				if (input.Length > 0)
					input = input[..^1];
				break;
			case Key.Enter:
				ExecuteCommand(input);
				CloseConsole();
				break;
			case Key.Space:
				input += " ";
				break;
			case Key.Tab:
				Autocomplete();
				break;
			default:
				var key = (char)keycode;

				if (!(key > 0x20 && key < 0x7F))
					break;

				if (!caps)
					key = char.ToLower(key);

				input += key;
				break;
		}
	}

    public override void _UnhandledKeyInput(InputEvent @event)
    {
		if (@event is not InputEventKey evt)
			return;

		if (isOpen) {
			if (evt.IsActionPressed(Constants.KeyBindings.CloseConsole)) {
				CloseConsole();
			} else if (evt.IsPressed()) {
				HandleInput(evt.Keycode, (evt.GetModifiersMask() & KeyModifierMask.MaskShift) != 0);
				UpdateConsole();
			}

			GetViewport().SetInputAsHandled();
		} else if (evt.IsActionPressed(Constants.KeyBindings.OpenConsole)) {
			OpenConsole();
			GetViewport().SetInputAsHandled();
		}
    }


	public class Command {
		public string Name;
		public CommandArgument[] Arguments;
		Delegate function;

		public Command(string name, Delegate function, CommandArgument[] arguments = null) {
			Name = name;
			Arguments = arguments ?? Array.Empty<CommandArgument>();
			this.function = function;
		}

		public void Call(object[] arguments) {
			function.DynamicInvoke(arguments);
		}
	}

	public enum HinterInputResult {
		Ok,
		Error,
		Hint,
	}

	public class HinterResult {
		public HinterInputResult InputResult;
		public IEnumerable<string> Hints;

		public HinterResult(HinterInputResult result, IEnumerable<string> hints = null) {
			InputResult = result;
			Hints = hints;
		}
	}

	public class CommandArgument {
		public delegate HinterResult Hinter(Command cmd, string[] argv, int argi);
		public delegate object Parser(string input);

		public string Name;
		public Type ValueType;
		public Parser ParserFunc;
		public Hinter HinterFunc;

		public CommandArgument(string name, Type valueType = null, Parser parser = null, Hinter hinter = null) {
			Name = name;

			ParserFunc = parser ?? (x => x);
			HinterFunc = hinter;

			ValueType = valueType;
		}
	}
}