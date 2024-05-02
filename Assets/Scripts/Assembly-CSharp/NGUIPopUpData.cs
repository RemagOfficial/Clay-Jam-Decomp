using System;
using UnityEngine;

[Serializable]
public class NGUIPopUpData
{
	public string _TextID;

	public bool _ShowCloseButton;

	public UIAnchor.Side _Anchor;

	public Vector2 _AnchorRelativeXY;

	public Action OnClosedCallback { get; set; }

	public void SetFromPrizeData(PrizeDefinition prizeDef)
	{
		_AnchorRelativeXY = Vector2.zero;
		_Anchor = UIAnchor.Side.Center;
		_ShowCloseButton = true;
		_TextID = "PRIZE_New";
	}
}
