using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/BestDistanceMLLabel")]
public class UIBestDistanceMLLabel : UIGameStatLabel
{
	protected override void Start()
	{
		_FormatStringID = "PRIZE_Distance";
		limit = 99999;
		base.Start();
	}

	protected override void UpdateStatValue()
	{
		if (CurrentHill.Instance == null)
		{
			base.StatValue = 0f;
		}
		else
		{
			base.StatValue = SaveData.Instance.Hills.GetHillByID(CurrentHill.Instance.ID)._BestScoreMonsterLove;
		}
	}
}
