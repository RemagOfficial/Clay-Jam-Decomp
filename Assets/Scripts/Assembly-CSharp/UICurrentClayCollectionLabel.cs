using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/ClayCollectionLabel")]
public class UICurrentClayCollectionLabel : UIGameStatLabel
{
	protected override void Start()
	{
		base.Start();
		_FormatString = "{0}";
	}

	protected override void UpdateStatValue()
	{
		limit = 99999;
		base.StatValue = Pebble.Instance.ClayCollected_DisplayAmount;
	}
}
