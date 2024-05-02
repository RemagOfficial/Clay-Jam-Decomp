using UnityEngine;

[RequireComponent(typeof(LocalisableText))]
public class UIHillNameLabelInsert : MonoBehaviour
{
	protected LocalisableText _label;

	public string LocalisationKey;

	private void Awake()
	{
		_label = GetComponent<LocalisableText>();
		if (_label == null)
		{
			Debug.Log("UIHillNameLabelInsert needs to be on an object with a LocalisableText");
		}
	}

	private void OnEnable()
	{
		if (CurrentHill.Instance != null)
		{
			SetLabel();
		}
	}

	private void SetLabel()
	{
		int iD = CurrentHill.Instance.ID;
		string newValue = Localization.instance.Get(string.Format("HILL_Title_" + iD));
		string text = Localization.instance.Get(LocalisationKey);
		string text2 = text.Replace("{0}", newValue);
		_label.text = text2;
	}
}
