public class QuestObstacleBounceRow : Quest
{
	public string _DescriptionKey = "QUEST_08";

	public string _ObstacleName;

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
		string key = string.Format("MONSTER_PL_{0}", _ObstacleName);
		string arg = Localization.instance.Get(key);
		string format = Localization.instance.Get(_DescriptionKey);
		return string.Format(format, _Count.ValueRequired(questIteration), arg);
	}

	public override bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration)
	{
		return currentRunStats.LongestRowBounced(_ObstacleName) >= TargetCount(questIteration);
	}

	public override int Progress(GameStats currentRunStats, int questIteration)
	{
		return currentRunStats.CurrentBounceRow(_ObstacleName);
	}

	public override int TargetCount(int questIteration)
	{
		return _Count.ValueRequired(questIteration);
	}
}
