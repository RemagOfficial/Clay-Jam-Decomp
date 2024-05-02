using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UINextPrizeDistanceLabel : MonoBehaviour
{
	protected UILabel _label;

	private void OnEnable()
	{
		_label = GetComponent<UILabel>();
		if (_label == null)
		{
			Debug.Log("UIClayCollectionLabel needs to be on an object with a UILabel");
		}
		int iD = CurrentHill.Instance.ID;
		PrizeDefinition prizeDefinition = SaveData.Instance.Prizes.NextPrize(iD);
		int num = -1;
		if (prizeDefinition != null)
		{
			num = SaveData.Instance.Prizes.MetersToNextPrize(iD);
		}
		string text = Localization.instance.Get("PRIZE_Distance");
		text = text.Replace("{0}", num.ToString());
		_label.text = text;
	}
}
