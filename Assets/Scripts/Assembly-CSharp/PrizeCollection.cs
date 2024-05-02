using System;
using System.Collections;
using System.Collections.Generic;

public class PrizeCollection
{
	public List<HillPrizeData> ProgressData;

	public void CreatePrizeData()
	{
		ProgressData = new List<HillPrizeData>(5);
		ProgressData.Add(new HillPrizeData(1));
		ProgressData.Add(new HillPrizeData(2));
		ProgressData.Add(new HillPrizeData(3));
		ProgressData.Add(new HillPrizeData(4));
		ProgressData.Add(new HillPrizeData(5));
	}

	public HillPrizeData DataForHill(int hillID)
	{
		return ProgressData.Find((HillPrizeData pd) => pd._HillID == hillID);
	}

	public int CurrentPrizeIndexForHill(int hillID)
	{
		return DataForHill(hillID)._PrizeIndex;
	}

	public int MetersToNextPrize(int hillID)
	{
		return DataForHill(hillID).MetersToNextPrize();
	}

	public PrizeDefinition NextPrize(int hillID)
	{
		return DataForHill(hillID).NextPrize;
	}

	public Hashtable GetHash()
	{
		Dictionary<int, Hashtable> dictionary = new Dictionary<int, Hashtable>(ProgressData.Count);
		for (int i = 0; i < ProgressData.Count; i++)
		{
			dictionary.Add(i, ProgressData[i].Encode());
		}
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hash, Version version, bool adding)
	{
		int num = 0;
		foreach (DictionaryEntry item in hash)
		{
			DecodePrize(item.Value as Hashtable, num++, adding);
		}
	}

	private void DecodePrize(Hashtable hash, int dataIndex, bool adding)
	{
		HillPrizeData hillPrizeData = ProgressData[dataIndex];
		hillPrizeData.Decode(hash, adding);
	}
}
