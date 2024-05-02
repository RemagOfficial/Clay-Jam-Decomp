using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/BestDistanceLabel")]
public class UIBestDistanceLabel : UIGameStatLabel
{
	protected override void Start()
	{
		_FormatStringID = "PRIZE_Distance";
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
			base.StatValue = SaveData.Instance.Hills.GetHillByID(CurrentHill.Instance.ID)._BestScore;
		}
	}
}
