using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Toggle Button")]
public class UIToggleButton : MonoBehaviour
{
	public delegate void ToggleDelegate(bool newValue);

	public UISprite target;

	public string onSprite;

	public string offSprite;

	public ToggleDelegate _OnToggle;

	public bool isOn { get; private set; }

	public void SetOn(bool b)
	{
		isOn = b;
		target.spriteName = ((!isOn) ? offSprite : onSprite);
	}

	private void Start()
	{
		if (target == null)
		{
			target = GetComponentInChildren<UISprite>();
		}
	}

	private void OnClick()
	{
		if (target != null)
		{
			isOn = !isOn;
			target.spriteName = ((!isOn) ? offSprite : onSprite);
			if (_OnToggle != null)
			{
				_OnToggle(isOn);
			}
		}
	}
}
