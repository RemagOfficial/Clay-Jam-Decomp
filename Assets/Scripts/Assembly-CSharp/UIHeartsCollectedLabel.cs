using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/HeartsCollected")]
public class UIHeartsCollectedLabel : UIGameStatLabel
{
	protected override void Start()
	{
		base.Start();
		_FormatString = "{0}";
	}

	protected override void UpdateStatValue()
	{
		limit = 99999;
		base.StatValue = GameModeStateMonsterLove.Instance.Hearts;
	}
}
