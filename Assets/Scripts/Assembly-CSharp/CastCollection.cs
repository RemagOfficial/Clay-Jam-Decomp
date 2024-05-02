using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CastCollection
{
	public List<CastData> ObjectList { get; private set; }

	public List<CastData> PurchasableList { get; private set; }

	public int Count
	{
		get
		{
			return ObjectList.Count;
		}
	}

	public int PurchasableCount
	{
		get
		{
			return PurchasableList.Count;
		}
	}

	public void CreateCollectionData()
	{
		ObjectList = new List<CastData>();
		PurchasableList = new List<CastData>();
		for (int i = 0; i < ObstacleDatabase.Instance.ObstacleDefinitions.Length; i++)
		{
			ObstacleDefinition defitnionByIndex = ObstacleDatabase.Instance.GetDefitnionByIndex(i);
			int num = defitnionByIndex._Colours.Count;
			if (num == 0)
			{
				if (defitnionByIndex._Type != ObstacleType.Native && defitnionByIndex._Type != ObstacleType.PowerUp)
				{
					Debug.LogWarning("No Colours set for " + defitnionByIndex.name);
				}
				num = 1;
			}
			if (!defitnionByIndex._CanBePurchased)
			{
				num = 1;
			}
			for (int j = 0; j < num; j++)
			{
				CastData castData = new CastData(defitnionByIndex.name, j, defitnionByIndex._InitialState);
				castData.MouldInfo = defitnionByIndex;
				if (defitnionByIndex._CanBePurchased)
				{
					PurchasableList.Add(castData);
				}
				ObjectList.Add(castData);
			}
		}
	}

	public bool UnlockCastsForMould(string mouldName)
	{
		bool result = false;
		for (int i = 0; i < PurchasableCount; i++)
		{
			if (PurchasableList[i].Id.MouldName == mouldName && PurchasableList[i].StateOnCurrentHill == LockState.Locked)
			{
				PurchasableList[i].StateOnCurrentHill = LockState.Unlocked;
				result = true;
			}
		}
		return result;
	}

	public bool UnlockCast(CastId id)
	{
		for (int i = 0; i < PurchasableCount; i++)
		{
			if (PurchasableList[i].Id.Compare(id))
			{
				if (PurchasableList[i].StateOnCurrentHill == LockState.Locked)
				{
					PurchasableList[i].StateOnCurrentHill = LockState.Unlocked;
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public bool CanPurchaseCast(CastId id)
	{
		for (int i = 0; i < PurchasableCount; i++)
		{
			if (PurchasableList[i].Id.Compare(id) && PurchasableList[i].StateOnCurrentHill == LockState.Unlocked)
			{
				ClayCollection clayCollected = SaveData.Instance.ClayCollected;
				return clayCollected.CanSubtract(PurchasableList[i].ColourIndex, PurchasableList[i].Cost);
			}
		}
		return false;
	}

	public bool PurchaseCast(CastId id)
	{
		for (int i = 0; i < PurchasableCount; i++)
		{
			if (!PurchasableList[i].Id.Compare(id))
			{
				continue;
			}
			if (PurchasableList[i].StateOnCurrentHill == LockState.Unlocked)
			{
				ClayCollection clayCollected = SaveData.Instance.ClayCollected;
				if (clayCollected.CanSubtract(0, PurchasableList[i].Cost))
				{
					clayCollected.Subtract(0, PurchasableList[i].Cost);
					PurchasableList[i].StateOnCurrentHill = LockState.Purchased;
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public void PurchaseEveything()
	{
		for (int i = 0; i < PurchasableCount; i++)
		{
			PurchasableList[i].SetStateForAllHills(LockState.Purchased);
		}
	}

	public CastData GetCast(CastId castID)
	{
		return ObjectList.Find((CastData c) => c.Id.Compare(castID));
	}

	public Hashtable GetHash()
	{
		Dictionary<int, Hashtable> dictionary = new Dictionary<int, Hashtable>(PurchasableCount);
		for (int i = 0; i < PurchasableCount; i++)
		{
			dictionary.Add(i, PurchasableList[i].Encode());
		}
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hashtable, Version version, bool adding)
	{
		foreach (DictionaryEntry item in hashtable)
		{
			DecodeCast(item.Value as Hashtable, adding);
		}
	}

	private void DecodeCast(Hashtable hash, bool adding)
	{
		CastData castData = new CastData();
		castData.Decode(hash);
		CastData cast = GetCast(castData.Id);
		if (cast != null)
		{
			cast.CopyStates(castData, adding);
		}
	}
}
