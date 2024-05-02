using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Sprite Animation On Event")]
[ExecuteInEditMode]
public class UISpriteAnimationOnEvent : MonoBehaviour
{
	public string _Prefix = string.Empty;

	public UISpriteAnimationObject _OnEnable;

	public UISpriteAnimationObject _OnClick;

	public UISpriteAnimationObject _OnPress;

	public UISpriteAnimationObject _OnRelease;

	private UISprite Sprite;

	private List<string> SpriteNames = new List<string>();

	private UISpriteAnimationObject CurrentAnimation;

	private float Delta;

	private int Index;

	private bool Stopped = true;

	public bool IsPressedDown
	{
		get
		{
			return CurrentAnimation == _OnPress;
		}
	}

	private void OnEnable()
	{
		if (_OnEnable._Frames.Count > 0)
		{
			CurrentAnimation = _OnEnable;
			RebuildSpriteList();
		}
	}

	public void PlayOnEnableAnim()
	{
		OnEnable();
	}

	public void PlayOnReleaseAnim()
	{
		CurrentAnimation = _OnRelease;
		RebuildSpriteList();
	}

	private void Update()
	{
		if (CurrentAnimation == null || Stopped || SpriteNames.Count <= 0 || !Application.isPlaying)
		{
			return;
		}
		Delta += Time.deltaTime;
		float num = ((!((float)CurrentAnimation._FPS > 0f)) ? 0f : (1f / (float)CurrentAnimation._FPS));
		if (!(num < Delta))
		{
			return;
		}
		Sprite.spriteName = SpriteNames[Index];
		Delta = ((!(num > 0f)) ? 0f : (Delta - num));
		if (++Index >= SpriteNames.Count)
		{
			if (CurrentAnimation._Repeat)
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
		if (!(Sprite != null) || !(Sprite.atlas != null) || CurrentAnimation == null)
		{
			return;
		}
		foreach (string frame in CurrentAnimation._Frames)
		{
			SpriteNames.Add(_Prefix + frame);
		}
		Stopped = false;
		Delta = 0f;
		Index = 0;
	}

	private void OnClick()
	{
		if (_OnClick._Frames.Count > 0)
		{
			CurrentAnimation = _OnClick;
			RebuildSpriteList();
		}
	}

	private void OnPress(bool isPressed)
	{
		if (isPressed && _OnPress._Frames.Count > 0)
		{
			CurrentAnimation = _OnPress;
			RebuildSpriteList();
		}
		else if (!isPressed && _OnRelease._Frames.Count > 0)
		{
			CurrentAnimation = _OnRelease;
			RebuildSpriteList();
		}
	}
}
