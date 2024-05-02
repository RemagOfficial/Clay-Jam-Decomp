using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Sprite Animation Random")]
[RequireComponent(typeof(UISprite))]
[ExecuteInEditMode]
public class UISpriteAnimationRandom : MonoBehaviour
{
	public string _Prefix = string.Empty;

	public UISpriteAnimationObject _Animation;

	public float _MinRandomTime;

	public float _MaxRandomTime;

	private UISprite Sprite;

	private List<string> SpriteNames = new List<string>();

	private float Delta;

	private int Index;

	private bool Stopped = true;

	private float TimeTillNextAnimation;

	private void OnEnable()
	{
		WorkOutTime();
		Stopped = true;
		Delta = 0f;
		Index = 0;
	}

	private void Update()
	{
		if (_Animation != null && !Stopped && SpriteNames.Count > 0 && Application.isPlaying)
		{
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
					return;
				}
				Stopped = true;
				WorkOutTime();
			}
		}
		else
		{
			TimeTillNextAnimation -= Time.deltaTime;
			if (TimeTillNextAnimation <= 0f)
			{
				RebuildSpriteList();
			}
		}
	}

	private void WorkOutTime()
	{
		TimeTillNextAnimation = Random.Range(_MinRandomTime, _MaxRandomTime);
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
