using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class GameModeData
{
	private const string GameModeKey = "current";

	public GameModeType Current { get; set; }

	public void SetInitialState()
	{
		Current = GameModeType.Quest;
	}

	public Hashtable GetHash()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("current", ((int)Current).ToString());
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hash, Version version, bool adding)
	{
		if (hash.ContainsKey("current"))
		{
			string s = (string)hash["current"];
			Current = (GameModeType)int.Parse(s);
		}
	}
}
