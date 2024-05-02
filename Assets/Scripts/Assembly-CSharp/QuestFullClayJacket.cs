public class QuestFullClayJacket : Quest
{
	public string _DescriptionKey = "QUEST_FullClayJacket";

	public QuestRequirementInt _Count;

	public override bool HasProgressCounter
	{
		get
		{
			return true;
		}
	}

	public override string Description(int questIteration)
	{
		string format = Localization.instance.Get(_DescriptionKey);
		return string.Format(format, _Count.ValueRequired(questIteration));
	}

	public override bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration)
	{
		return Progress(currentRunStats, questIteration) >= TargetCount(questIteration);
	}

	public override int Progress(GameStats currentRunStats, int questIteration)
	{
		return currentRunStats.PowerUpsCollected;
	}

	public override int TargetCount(int questIteration)
	{
		return _Count.ValueRequired(questIteration);
	}
}
