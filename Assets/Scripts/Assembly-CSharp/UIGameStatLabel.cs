using System.Text;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public abstract class UIGameStatLabel : MonoBehaviour
{
	public string _FormatStringID = "{0}";

	protected string _FormatString;

	protected UILabel _label;

	protected int limit = 9999999;

	private StringBuilder amountText = new StringBuilder(32);

	protected float StatValue { get; set; }

	protected virtual void Start()
	{
		_FormatString = Localization.instance.Get(_FormatStringID).ToUpper();
		_label = GetComponent<UILabel>();
		if (_label == null)
		{
			Debug.Log("UIClayCollectionLabel needs to be on an object with a UILabel");
		}
	}

	private void Update()
	{
		UpdateStatValue();
		amountText.Remove(0, amountText.Length);
		amountText.AppendFormat("{0:0}", Localization.PunctuatedNumber(StatValue, limit));
		_label.text = string.Format(_FormatString, amountText.ToString());
	}

	protected abstract void UpdateStatValue();
}
