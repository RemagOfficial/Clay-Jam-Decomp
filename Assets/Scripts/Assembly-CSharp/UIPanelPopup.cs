using System;
using UnityEngine;

public class UIPanelPopup : MonoBehaviour
{
	public UILabel _Text;

	public GameObject _CloseButton;

	public UIAnchor _anchor;

	private NGUIPopUpData _data;

	public Action OnClosedCallback
	{
		get
		{
			return (_data != null) ? _data.OnClosedCallback : null;
		}
	}

	private void Awake()
	{
		_anchor = GetComponent<UIAnchor>();
	}

	public void SetData(NGUIPopUpData data)
	{
		_data = data;
		if ((bool)_Text)
		{
			_Text.text = Localization.instance.Get(data._TextID);
		}
	}

	private void OnEnable()
	{
		if ((bool)_CloseButton && _data != null)
		{
			if (_data._ShowCloseButton)
			{
				_CloseButton.active = true;
			}
			else
			{
				_CloseButton.active = false;
			}
		}
		if ((bool)_anchor)
		{
			_anchor.side = _data._Anchor;
			_anchor.relativeOffset = _data._AnchorRelativeXY;
		}
	}
}
