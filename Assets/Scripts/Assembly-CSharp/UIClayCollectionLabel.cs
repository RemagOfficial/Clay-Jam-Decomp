using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/ClayCollectionLabel")]
public class UIClayCollectionLabel : UIGameStatLabel
{
	public int _ColourIndex;

	private ClayCollection _clayCollection;

	protected override void Start()
	{
		base.Start();
		_FormatString = "{0}";
		_clayCollection = SaveData.Instance.ClayCollected;
		_ColourIndex = 0;
	}

	protected override void UpdateStatValue()
	{
		if (_clayCollection != null)
		{
			base.StatValue = _clayCollection.Amount(_ColourIndex);
		}
	}
}
