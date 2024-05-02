using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/Hill Label")]
[RequireComponent(typeof(LocalisableText))]
public class UIHillNameLabel : MonoBehaviour
{
	protected LocalisableText _label;

	public int _HillID = -1;

	private void Awake()
	{
		_label = GetComponent<LocalisableText>();
		if (_label == null)
		{
			Debug.Log("UIClayCollectionLabel needs to be on an object with a UILabel");
		}
	}

	private void OnEnable()
	{
		SetLabel();
	}

	private void SetLabel()
	{
		int num = CurrentHill.Instance.ID;
		if (_HillID != -1)
		{
			num = _HillID;
		}
		string text = Localization.instance.Get(string.Format("HILL_Title_" + num));
		_label.text = text;
	}
}
