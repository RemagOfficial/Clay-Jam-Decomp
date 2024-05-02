using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClayCollection
{
	public List<ClayData> AllClayColours { get; set; }

	public float TotalAmount
	{
		get
		{
			float num = 0f;
			foreach (ClayData allClayColour in AllClayColours)
			{
				num += allClayColour._Amount;
			}
			return num;
		}
	}

	public ClayCollection(int numColours)
	{
		AllClayColours = new List<ClayData>(numColours);
		for (int i = 0; i < numColours; i++)
		{
			AllClayColours.Add(new ClayData(i, 0f));
		}
	}

	public void AddSingleColour(ClayData clayToAdd)
	{
		AddSingleColour(clayToAdd._ColourIndex, clayToAdd._Amount);
	}

	public void AddSingleColour(int colourIndex, float Amount)
	{
		ClayData clayData = AllClayColours.Find((ClayData x) => x._ColourIndex == colourIndex);
		clayData._Amount += Amount;
	}

	public void AddCollection(ClayCollection collectionToAdd)
	{
		foreach (ClayData allClayColour in collectionToAdd.AllClayColours)
		{
			AddSingleColour(allClayColour);
		}
	}

	public float Amount(int colorIndex)
	{
		ClayData clayData = AllClayColours.Find((ClayData x) => x._ColourIndex == colorIndex);
		return clayData._Amount;
	}

	public bool CanSubtract(ClayData clayToSubtract)
	{
		if (clayToSubtract._Amount <= AllClayColours[clayToSubtract._ColourIndex]._Amount)
		{
			return true;
		}
		return false;
	}

	public bool CanSubtract(int colourIndex, float amount)
	{
		if (amount <= AllClayColours[colourIndex]._Amount)
		{
			return true;
		}
		return false;
	}

	public bool CanSubtract(ClayCollection clayToSubtract)
	{
		for (int i = 0; i < clayToSubtract.AllClayColours.Count; i++)
		{
			if (!CanSubtract(clayToSubtract.AllClayColours[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void Subtract(ClayData clayToSubtract)
	{
		AllClayColours[clayToSubtract._ColourIndex]._Amount -= clayToSubtract._Amount;
	}

	public void Subtract(int colourIndex, float amount)
	{
		AllClayColours[colourIndex]._Amount -= amount;
		if (AllClayColours[colourIndex]._Amount < 0f)
		{
			AllClayColours[colourIndex]._Amount = 0f;
		}
	}

	public void Subtract(ClayCollection clayToSubtract)
	{
		for (int i = 0; i < clayToSubtract.AllClayColours.Count; i++)
		{
			Subtract(clayToSubtract.AllClayColours[i]);
		}
	}

	public void ClearColour(int colourIndex)
	{
		foreach (ClayData allClayColour in AllClayColours)
		{
			if (allClayColour._ColourIndex == colourIndex)
			{
				allClayColour._Amount = 0f;
			}
		}
	}

	public void Clear()
	{
		for (int i = 0; i < AllClayColours.Count; i++)
		{
			AllClayColours[i]._ColourIndex = i;
			AllClayColours[i]._Amount = 0f;
		}
	}

	public override string ToString()
	{
		string text = string.Empty;
		foreach (ClayData allClayColour in AllClayColours)
		{
			text = string.Format("{0}({1}:{2})", text, allClayColour._ColourIndex, allClayColour._Amount);
		}
		return text;
	}

	public Hashtable GetHash()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(AllClayColours.Count);
		for (int i = 0; i < AllClayColours.Count; i++)
		{
			dictionary.Add(i.ToString(), AllClayColours[i].Encode());
		}
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hashtable, Version version, bool adding)
	{
		if (!adding)
		{
			AllClayColours.Clear();
			AllClayColours.Capacity = hashtable.Count;
		}
		else
		{
			AllClayColours.Capacity = Mathf.Max(AllClayColours.Capacity, hashtable.Count);
		}
		foreach (DictionaryEntry item in hashtable)
		{
			ClayData clayData = new ClayData();
			clayData.Decode((string)item.Value);
			if (adding)
			{
				AddSingleColour(clayData);
			}
			else
			{
				AllClayColours.Add(clayData);
			}
		}
	}
}
