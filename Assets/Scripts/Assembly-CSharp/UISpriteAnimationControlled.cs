using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Clay Jam/Sprite Animation Controlled")]
[RequireComponent(typeof(UISprite))]
public class UISpriteAnimationControlled : MonoBehaviour
{
	public string _Prefix = string.Empty;

	public UISpriteAnimationObject _Animation;

	private UISprite Sprite;

	private List<string> SpriteNames = new List<string>();

	private int Index;

	private bool Stopped = true;

	public int NumFrames
	{
		get
		{
			return _Animation._Frames.Count;
		}
	}

	public int CurrentFrame
	{
		get
		{
			return Index;
		}
	}

	private void Awake()
	{
		Rewind();
	}

	public bool NextFrame()
	{
		if (_Animation == null || Stopped || SpriteNames.Count == 0)
		{
			return false;
		}
		if (++Index >= SpriteNames.Count)
		{
			if (_Animation._Repeat)
			{
				Index = 0;
			}
			else
			{
				Stopped = true;
			}
		}
		if (!Stopped)
		{
			Sprite.spriteName = SpriteNames[Index];
			return true;
		}
		return false;
	}

	public void GotoFrame(int frame)
	{
		if (SpriteNames.Count == 0)
		{
			RebuildSpriteList();
		}
		Index = ((frame >= SpriteNames.Count) ? (SpriteNames.Count - 1) : frame);
		Stopped = false;
		Sprite.spriteName = SpriteNames[Index];
	}

	private void RebuildSpriteList()
	{
		if (Sprite == null)
		{
			Sprite = GetComponent<UISprite>();
		}
		SpriteNames.Clear();
		if (!(Sprite != null) || !(Sprite.atlas != null) || _Animation == null)
		{
			return;
		}
		foreach (string frame in _Animation._Frames)
		{
			SpriteNames.Add(_Prefix + frame);
		}
		Stopped = false;
		Index = 0;
	}

	public void Rewind()
	{
		if (SpriteNames.Count == 0)
		{
			RebuildSpriteList();
		}
		Index = 0;
		Stopped = false;
		Sprite.spriteName = SpriteNames[Index];
	}

	public void GoToLastFrame()
	{
		if (SpriteNames.Count == 0)
		{
			RebuildSpriteList();
		}
		GotoFrame(SpriteNames.Count - 1);
	}
}
