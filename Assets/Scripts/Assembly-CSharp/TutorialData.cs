using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialData
{
	public string Name { get; private set; }

	public int NumPlays { get; set; }

	public TutorialData(string name)
	{
		Name = name;
		NumPlays = 0;
	}

	public Hashtable Encode()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("NP", NumPlays.ToString());
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hash, bool adding)
	{
		string s = hash["NP"] as string;
		int num = int.Parse(s);
		if (adding)
		{
			num = Mathf.Max(num, NumPlays);
		}
		else
		{
			NumPlays = num;
		}
	}

	public void CopyFromLoadedData(TutorialData loadedData)
	{
		NumPlays = loadedData.NumPlays;
	}

	public void Reset()
	{
		NumPlays = 0;
	}
}
