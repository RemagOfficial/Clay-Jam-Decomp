public class QuestSquashWithShrink : Quest
{
	public string _DescriptionKey = "QUEST_06";

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
		string arg = Localization.instance.Get("POWERUP_Shrink");
		string format = Localization.instance.Get(_DescriptionKey);
		return string.Format(format, _Count.ValueRequired(questIteration), arg);
	}

	public override bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration)
	{
		return Progress(currentRunStats, questIteration) >= TargetCount(questIteration);
	}

	public override int Progress(GameStats currentRunStats, int questIteration)
	{
		return currentRunStats.SquashedWithShrink;
	}

	public override int TargetCount(int questIteration)
	{
		return _Count.ValueRequired(questIteration);
	}
}
