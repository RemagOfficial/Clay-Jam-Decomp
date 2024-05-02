using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Clay Jam/Sprite Animation")]
[RequireComponent(typeof(UISprite))]
public class UISpriteAnimation : MonoBehaviour
{
	public string _Prefix = string.Empty;

	public UISpriteAnimationObject _Animation;

	private UISprite Sprite;

	private List<string> SpriteNames = new List<string>();

	private float Delta;

	private int Index;

	private bool Stopped = true;

	private void Start()
	{
		RebuildSpriteList();
		Stopped = true;
	}

	public void Play()
	{
		Stopped = false;
		Index = 0;
		Delta = 0f;
	}

	private void Update()
	{
		if (_Animation == null || Stopped || SpriteNames.Count <= 0)
		{
			return;
		}
		Delta += Time.deltaTime;
		float num = ((!((float)_Animation._FPS > 0f)) ? 0f : (1f / (float)_Animation._FPS));
		if (!(num < Delta))
		{
			return;
		}
		Sprite.spriteName = SpriteNames[Index];
		Delta = ((!(num > 0f)) ? 0f : (Delta - num));
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
		Delta = 0f;
		Index = 0;
	}
}
