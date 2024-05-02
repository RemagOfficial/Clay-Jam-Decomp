public class QuestDistance : Quest
{
	public string _DescriptionKey = "QUEST_02";

	public QuestRequirementInt _Distance;

	public override bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration)
	{
		return currentRunStats.MetersFlown >= _Distance.ValueRequired(questIteration);
	}

	public override string Description(int questIteration)
	{
		string key = string.Format("BOSS_Name_{0}", CurrentHill.Instance.ID);
		string arg = Localization.instance.Get(key);
		string format = Localization.instance.Get(_DescriptionKey);
		return string.Format(format, arg, _Distance.ValueRequired(questIteration));
	}

	public override string CounterText(GameStats currentRunStats, int questIteration)
	{
		string text = Localization.instance.Get("PRIZE_Distance");
		string arg = Localization.PunctuatedNumber(_Distance.ValueRequired(questIteration), int.MaxValue);
		return string.Format(text.ToUpper(), arg);
	}
}
