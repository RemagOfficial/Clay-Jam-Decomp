public class NextPrizeSignPost : GameStatTextObject
{
	protected override string GetFormatString()
	{
		return Localization.instance.Get("PRIZE_Distance");
	}

	protected override int GetStat()
	{
		HillPrizeData hillPrizeData = SaveData.Instance.Prizes.DataForHill(CurrentHill.Instance.ID);
		return hillPrizeData.MetersToNextPrize();
	}
}
