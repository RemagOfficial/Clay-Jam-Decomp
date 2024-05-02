using UnityEngine;

[RequireComponent(typeof(LocalisableText))]
public class UIVersionNumberLabel : MonoBehaviour
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
		string text = string.Format("V. {0}", BuildDetails.Instance._VersionNumber);
		_label.text = text;
	}
}
