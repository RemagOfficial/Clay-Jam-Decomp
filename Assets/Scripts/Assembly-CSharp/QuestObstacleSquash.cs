public class QuestObstacleSquash : Quest
{
	public string _DescriptionKey = "QUEST_01";

	public QuestRequirementString _ObstacleNames;

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
		string key = string.Format("MONSTER_PL_{0}", _ObstacleNames.ValueRequired(questIteration));
		string arg = Localization.instance.Get(key);
		string format = Localization.instance.Get(_DescriptionKey);
		return string.Format(format, _Count.ValueRequired(questIteration), arg);
	}

	public override bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration)
	{
		return Progress(currentRunStats, questIteration) >= TargetCount(questIteration);
	}

	public override int Progress(GameStats currentRunStats, int questIteration)
	{
		string obstacleName = _ObstacleNames.ValueRequired(questIteration);
		return currentRunStats.TotalSquashedByName(obstacleName);
	}

	public override int TargetCount(int questIteration)
	{
		return _Count.ValueRequired(questIteration);
	}
}
