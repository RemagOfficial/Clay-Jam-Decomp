using UnityEngine;

public class BestScoreSignPost : GameStatTextObject
{
	protected override string GetFormatString()
	{
		return Localization.instance.Get("PRIZE_Distance");
	}

	protected override int GetStat()
	{
		float f = 0f;
		switch (CurrentGameMode.Type)
		{
		case GameModeType.Quest:
			f = CurrentHill.Instance.ProgressData._BestScore;
			break;
		case GameModeType.MonsterLove:
			f = CurrentHill.Instance.ProgressData._BestScoreMonsterLove;
			break;
		}
		return Mathf.FloorToInt(f);
	}
}
