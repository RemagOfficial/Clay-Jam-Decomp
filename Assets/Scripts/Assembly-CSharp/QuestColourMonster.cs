public class QuestColourMonster : Quest
{
	public string _DescriptionKey = "QUEST_05";

	public int _ColourIndex;

	public string _ColourNameKey;

	public QuestRequirementInt _Count;

	public bool _InARow;

	public override bool HasProgressCounter
	{
		get
		{
			return true;
		}
	}

	public override string Description(int questIteration)
	{
		string arg = Localization.instance.Get(_ColourNameKey);
		string format = Localization.instance.Get(_DescriptionKey);
		return string.Format(format, _Count.ValueRequired(questIteration), arg);
	}

	public override bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration)
	{
		if (_InARow)
		{
			return currentRunStats.LongestColourRow(_ColourIndex) >= TargetCount(questIteration);
		}
		return currentRunStats.TotalColour(_ColourIndex) >= TargetCount(questIteration);
	}

	public override int Progress(GameStats currentRunStats, int questIteration)
	{
		if (_InARow)
		{
			return currentRunStats.CurrentRow(_ColourIndex);
		}
		return currentRunStats.TotalColour(_ColourIndex);
	}

	public override int TargetCount(int questIteration)
	{
		return _Count.ValueRequired(questIteration);
	}
}
