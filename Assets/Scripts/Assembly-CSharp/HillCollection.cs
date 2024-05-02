using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HillCollection
{
	public List<HillData> Hills { get; private set; }

	public bool AllDefeated
	{
		get
		{
			foreach (HillData hill in Hills)
			{
				if (hill._BeastDefeatedCount == 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	public void CreateHillData()
	{
		int numHills = HillDatabase.NumHills;
		Hills = new List<HillData>();
		for (int i = 0; i < numHills; i++)
		{
			HillDefinition defintionFromIndex = HillDatabase.Instance.GetDefintionFromIndex(i);
			HillData item = new HillData(defintionFromIndex);
			Hills.Add(item);
		}
	}

	public HillData GetHillByID(int id)
	{
		for (int i = 0; i < Hills.Count; i++)
		{
			if (Hills[i]._ID == id)
			{
				return Hills[i];
			}
		}
		return null;
	}

	public void RunCompleteForHill(int hillID, float time, float beans)
	{
		HillData hillByID = GetHillByID(hillID);
		if (hillByID._BestTime > time || hillByID._BestTime < 0f)
		{
			hillByID._BestTime = time;
			Debug.Log(string.Format("new best time {0}", time));
		}
		if (hillByID._BestScore < beans)
		{
			hillByID._BestScore = beans;
		}
	}

	public void RunCompleteForHill_MonsterLove(int hillID, float distance)
	{
		HillData hillByID = GetHillByID(hillID);
		if (hillByID._BestScoreMonsterLove < distance)
		{
			hillByID._BestScoreMonsterLove = distance;
		}
	}

	public void UpgradeHill(int hillID, int upgradeLevel)
	{
		HillData hillByID = GetHillByID(hillID);
		if (hillByID._UpgradeLevel != upgradeLevel - 1)
		{
			Debug.LogError(string.Format("Upgrading hill from {0} to {1} is wrongness", hillByID._UpgradeLevel, upgradeLevel));
		}
		hillByID._UpgradeLevel = upgradeLevel;
	}

	public Hashtable GetHash()
	{
		Dictionary<int, Hashtable> dictionary = new Dictionary<int, Hashtable>(Hills.Count);
		for (int i = 0; i < Hills.Count; i++)
		{
			dictionary.Add(Hills[i]._ID, Hills[i].Encode());
		}
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hash, Version version, bool adding)
	{
		foreach (DictionaryEntry item in hash)
		{
			DecodeHill(item.Key as string, item.Value as Hashtable, version, adding);
		}
	}

	private void DecodeHill(string idString, Hashtable hash, Version version, bool adding)
	{
		int id = int.Parse(idString);
		HillData hillByID = GetHillByID(id);
		hillByID.Decode(hash, version, adding);
	}
}
