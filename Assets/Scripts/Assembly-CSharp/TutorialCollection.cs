using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TutorialCollection
{
	private List<TutorialData> _tutorials;

	public void CreateTutorialData()
	{
		_tutorials = new List<TutorialData>(TutorialDatabase.Instance.Tutorials.Count);
		foreach (Tutorial tutorial in TutorialDatabase.Instance.Tutorials)
		{
			TutorialData item = new TutorialData(tutorial.name);
			_tutorials.Add(item);
		}
	}

	public TutorialData GetTutorialData(string name)
	{
		return _tutorials.Find((TutorialData t) => t.Name == name);
	}

	public Hashtable GetHash()
	{
		Dictionary<string, Hashtable> dictionary = new Dictionary<string, Hashtable>(_tutorials.Count);
		foreach (TutorialData tutorial in _tutorials)
		{
			dictionary.Add(tutorial.Name, tutorial.Encode());
		}
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hash, Version version, bool adding)
	{
		foreach (DictionaryEntry item in hash)
		{
			DecodeTutorial(item.Key as string, item.Value as Hashtable, adding);
		}
	}

	private void DecodeTutorial(string name, Hashtable hash, bool adding)
	{
		TutorialData tutorialData = GetTutorialData(name);
		if (tutorialData != null)
		{
			tutorialData.Decode(hash, adding);
		}
	}

	public void Reset()
	{
		foreach (TutorialData tutorial in _tutorials)
		{
			tutorial.Reset();
		}
		SaveData.Instance.Progress.ResetTutorialRelatedFlags();
	}
}
