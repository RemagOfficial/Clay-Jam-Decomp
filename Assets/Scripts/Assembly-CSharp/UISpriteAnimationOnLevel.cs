using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/ClayJam/Sprite Animation per Level")]
[ExecuteInEditMode]
public class UISpriteAnimationOnLevel : MonoBehaviour
{
	public List<UISpriteAnimationObject_Extended> _Animations;

	private UISprite Sprite;

	private List<string> SpriteNames = new List<string>();

	private UISpriteAnimationObject_Extended CurrentAnimation;

	private float Delta;

	private int Index;

	private bool Stopped = true;

	private void OnEnable()
	{
		int index = CurrentHill.Instance.ID - 1;
		if (_Animations.Count > 0 && _Animations[index]._Frames.Count > 0)
		{
			CurrentAnimation = _Animations[index];
			RebuildSpriteList();
		}
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
			SpriteNames.Add(CurrentAnimation._Prefix + frame);
		}
		Stopped = false;
		Delta = 0f;
		Index = 0;
	}
}
