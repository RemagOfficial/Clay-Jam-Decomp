using UnityEngine;

[RequireComponent(typeof(LocalisableText))]
public class UIFlickLabel : MonoBehaviour
{
	protected LocalisableText _label;

	private void Awake()
	{
		_label = GetComponent<LocalisableText>();
	}

	private void Start()
	{
		SetLabel();
	}

	private void SetLabel()
	{
		string text = ((!BuildDetails.Instance._UseLeapIfAvailable) ? Localization.instance.Get("TUT_Flick") : Localization.instance.Get("TUT_Flick_Leap"));
		_label.text = text;
	}
}
