public class QuestColourBullyBeast : Quest
{
	public string _DescriptionKey = "QUEST_07";

	public int _ColourIndex;

	public string _ColourNameKey;

	public QuestRequirementInt _Distance;

	public override bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration)
	{
		if (!OnlyCollectedCorrectColour(currentRunStats))
		{
			return false;
		}
		return currentRunStats.MetersFlown >= _Distance.ValueRequired(questIteration);
	}

	public override string Description(int questIteration)
	{
		string arg = Localization.instance.Get(_ColourNameKey);
		string key = string.Format("BOSS_Name_{0}", CurrentHill.Instance.ID);
		string arg2 = Localization.instance.Get(key);
		string format = Localization.instance.Get(_DescriptionKey);
		return string.Format(format, arg, arg2, _Distance.ValueRequired(questIteration));
	}

	public override bool CannotComplete(GameStats currentRunStats, int questIteration)
	{
		if (!OnlyCollectedCorrectColour(currentRunStats))
		{
			return true;
		}
		return false;
	}

	private bool OnlyCollectedCorrectColour(GameStats currentRunStats)
	{
		for (int i = 0; i < 3; i++)
		{
			if (i != _ColourIndex && currentRunStats.TotalColour(i) > 0)
			{
				return false;
			}
		}
		return true;
	}

	public override string CounterText(GameStats currentRunStats, int questIteration)
	{
		string text = Localization.instance.Get("PRIZE_Distance");
		string arg = Localization.PunctuatedNumber(_Distance.ValueRequired(questIteration), int.MaxValue);
		return string.Format(text.ToUpper(), arg);
	}
}
