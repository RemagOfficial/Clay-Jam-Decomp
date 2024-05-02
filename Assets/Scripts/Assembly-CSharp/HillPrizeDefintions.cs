using System;
using System.Collections.Generic;

[Serializable]
public class HillPrizeDefintions
{
	public int _HillID;

	public PrizeBeanFormula _Formula;

	public PrizeClayFormula _ProceduralClay;

	public List<PrizeDefinition> _PrizeList;

	public int NumPrizes
	{
		get
		{
			return _PrizeList.Count;
		}
	}

	public PrizeDefinition GetPrize(int prizeIndex)
	{
		return _PrizeList[prizeIndex];
	}

	public void CalculateBeansNeededForEachPrize()
	{
		int num = 0;
		foreach (PrizeDefinition prize in _PrizeList)
		{
			prize.Beans = CalculateBeansNeededForPrize(num++);
		}
	}

	public int CalculateBeansNeededForPrize(int prizeIndex)
	{
		return _Formula.BeansForPrize(prizeIndex);
	}
}
