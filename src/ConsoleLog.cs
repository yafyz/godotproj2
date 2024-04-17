using Godot;
using System;
using System.Collections.Generic;

public partial class ConsoleLog : Node
{
	public class Log
	{
		public DateTime Time;
		public string Text;

		public Log(DateTime time, string text)
		{
			Time = time;
			Text = text;
		}
	}

	public int LogsMax = 200;
	public List<Log> Logs = [];
	
	public event Action<Log> OnLog;
	
	public void Add(Log log)
	{
		if (Logs.Count + 1 > LogsMax) {
			Logs.RemoveRange(0, Logs.Count + 1 - LogsMax);
		}
		Logs.Add(log);
		OnLog?.Invoke(log);
	}

	public void Add(string str)
	{
		Add(new Log(DateTime.Now, str));
	}
}
