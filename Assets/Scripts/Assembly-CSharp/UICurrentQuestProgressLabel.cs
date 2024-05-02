using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/UICurrentQuestProgressLabel")]
public class UICurrentQuestProgressLabel : MonoBehaviour
{
	protected UILabel _label;

	private void OnEnable()
	{
		_label = GetComponent<UILabel>();
		if (_label == null)
		{
			Debug.Log("UIClayCollectionLabel needs to be on an object with a UILabel");
		}
		_label.text = string.Format("{0}/{1}", CurrentHill.Instance.ProgressData._CurrentQuestIndex, QuestDatabase.Instance.QuestCount(CurrentHill.Instance.ID));
	}
}
