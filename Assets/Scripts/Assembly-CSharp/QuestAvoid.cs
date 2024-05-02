public class QuestAvoid : Quest
{
	public string _DescriptionKey = "QUEST_04";

	public override bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration)
	{
		if (currentRunStats.TotalSquashed() > 0)
		{
			return false;
		}
		if (currentRunStats.TotalBounced() > 0)
		{
			return false;
		}
		return true;
	}

	public override string Description(int questIteration)
	{
		return Localization.instance.Get(_DescriptionKey);
	}

	public override bool CannotComplete(GameStats currentRunStats, int questIteration)
	{
		return !CompletedByCurrentRun(currentRunStats, questIteration);
	}

	public override string CounterText(GameStats currentRunStats, int questIteration)
	{
		return Localization.instance.Get("QUESTS_Avoid");
	}

	public override bool SplatFingerAffectsStats(ObstacleMould obstacle)
	{
		return false;
	}
}
