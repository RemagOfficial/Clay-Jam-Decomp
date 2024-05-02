using System;

[Serializable]
public class PrizeDefinition
{
	public PrizeDefinitionData _Data;

	public int _HillUpgrade;

	public int Beans { get; set; }

	public string GetDebugString()
	{
		return string.Format("{0} Beans. Awards : {1}", Beans, _Data.GetDebugString());
	}

	public void AwardPrize()
	{
		_Data.Award();
	}
}
