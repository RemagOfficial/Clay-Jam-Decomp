using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/CurrentRunDistanceLabel")]
public class UICurrentRunDistanceLabel : UIGameStatLabel
{
	protected override void Start()
	{
		_FormatStringID = "PRIZE_Distance";
		base.Start();
	}

	protected override void UpdateStatValue()
	{
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			base.StatValue = Pebble.Instance.MaxProgress;
		}
		else
		{
			base.StatValue = InGameController.Instance.MetersFlown;
		}
	}
}
