using System.Collections.Generic;
using UnityEngine;

public class PrizeDatabase : MonoBehaviour
{
	public static PrizeDatabase Instance;

	public List<HillPrizeDefintions> HillPrizesList;

	private void Awake()
	{
		if (!(Instance != null))
		{
			Instance = this;
			SetupInitialData();
		}
	}

	public HillPrizeDefintions PrizesForHill(int hillID)
	{
		return HillPrizesList.Find((HillPrizeDefintions h) => h._HillID == hillID);
	}

	public PrizeDefinition PrizeForHill(int hillID, int prizeIndex)
	{
		HillPrizeDefintions hillPrizeDefintions = PrizesForHill(hillID);
		return hillPrizeDefintions._PrizeList[prizeIndex];
	}

	public int PrizeCountForHill(int hillID)
	{
		return PrizesForHill(hillID).NumPrizes;
	}

	private void SetupInitialData()
	{
		CalculateBeansNeededForEachPrize();
	}

	private void CalculateBeansNeededForEachPrize()
	{
		foreach (HillPrizeDefintions hillPrizes in HillPrizesList)
		{
			hillPrizes.CalculateBeansNeededForEachPrize();
		}
	}

	public int CalculateBeansNeededForPrize(int hillID, int prizeIndex)
	{
		HillPrizeDefintions hillPrizeDefintions = PrizesForHill(hillID);
		return hillPrizeDefintions.CalculateBeansNeededForPrize(prizeIndex);
	}
}
