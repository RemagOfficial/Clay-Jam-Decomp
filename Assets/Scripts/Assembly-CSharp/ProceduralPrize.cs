public class ProceduralPrize
{
	public PrizeDefinition Definition { get; private set; }

	public int Index { get; private set; }

	public int HillID { get; private set; }

	public ProceduralPrize()
	{
		HillID = -1;
		Definition = new PrizeDefinition();
		Index = -1;
		Definition._Data = new PrizeDefinitionData();
	}

	public void SetHillID(int hillID)
	{
		HillID = hillID;
	}

	public void Generate(int prizeIndex)
	{
		Index = prizeIndex;
		HillDefinition definitionFromID = HillDatabase.Instance.GetDefinitionFromID(HillID);
		Definition._HillUpgrade = definitionFromID.MaxUpgradeLevel;
		SetupForPowerPlayPrize();
		int beans = PrizeDatabase.Instance.CalculateBeansNeededForPrize(HillID, Index);
		Definition.Beans = beans;
	}

	private void SetupForClayPrize()
	{
		Definition._Data._ID = 0;
		Definition._Data._Type = PrizeType.Clay;
		HillPrizeDefintions hillPrizeDefintions = PrizeDatabase.Instance.PrizesForHill(HillID);
		int num = Index - hillPrizeDefintions.NumPrizes;
		num /= 2;
		Definition._Data._Value = hillPrizeDefintions._ProceduralClay.ClayForPrize(num);
	}

	private void SetupForPowerPlayPrize()
	{
		Definition._Data._ID = HillID;
		Definition._Data._Type = PrizeType.PowerPlay;
	}
}
