using System;
using System.IO;
using UnityEngine;

public class GADebugLogger : IGALogger
{
	private string _pathToLogFile;

	public GADebugLogger()
	{
		string text = ((!Application.isEditor) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Application.dataPath);
		if (!text.EndsWith("Documents"))
		{
			text = Path.Combine(text, "Documents");
		}
		if (Application.isEditor && !Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		_pathToLogFile = Path.Combine(text, "GALog.txt");
	}

	public void log(string data)
	{
		using (StreamWriter streamWriter = new StreamWriter(_pathToLogFile, true))
		{
			streamWriter.Write(string.Format("{0} {1}\r\n", DateTime.Now, data));
		}
	}

	public void logStartRequest(string url)
	{
		log(string.Format("[started] {0}", url));
	}

	public void logSuccessfulRequest(string url)
	{
		log(string.Format("[succeeded] {0}", url));
	}

	public void logFailedRequest(string url, string error)
	{
		log(string.Format("[failed] {0}, {1}", url, error));
	}
}
