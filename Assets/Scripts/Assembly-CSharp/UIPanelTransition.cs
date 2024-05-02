using System.Collections.Generic;
using Fabric;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/ClayJam/Transition between panels")]
[RequireComponent(typeof(UISprite))]
public class UIPanelTransition : UIPanelTransitionBasic
{
	public string _Prefix = string.Empty;

	public UISpriteAnimationObject _TransitionOn;

	public UISpriteAnimationObject _TransitionOff;

	public UIEventType _EventOnTransitionOnFinished;

	public UIEventType _EventOnTransitionOffFinished;

	private UISprite Sprite;

	private List<string> SpriteNames = new List<string>();

	private UISpriteAnimationObject CurrentAnimation;

	private float Delta;

	private int Index;

	private bool Stopped = true;

	public override void TransitionOn()
	{
		if (_TransitionOn._Frames.Count > 0)
		{
			CurrentAnimation = _TransitionOn;
			RebuildSpriteList();
			InGameAudio.PostFabricEvent("TransitionOn", EventAction.SetSwitch, "TransitionOn");
			InGameAudio.PostFabricEvent("TransitionOn", EventAction.PlaySound);
		}
	}

	public override void TransitionOff()
	{
		if (_TransitionOff._Frames.Count > 0)
		{
			CurrentAnimation = _TransitionOff;
			RebuildSpriteList();
			InGameAudio.PostFabricEvent("TransitionOn", EventAction.SetSwitch, "TransitionOff");
			InGameAudio.PostFabricEvent("TransitionOn", EventAction.PlaySound);
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
		if (++Index < SpriteNames.Count)
		{
			return;
		}
		Stopped = true;
		if (CurrentAnimation == _TransitionOff)
		{
			base.TransitionOff();
			if (_EventOnTransitionOffFinished != 0)
			{
				UIEvents.SendEvent(_EventOnTransitionOffFinished, _Panel);
			}
		}
		else if (_EventOnTransitionOnFinished != 0)
		{
			UIEvents.SendEvent(_EventOnTransitionOnFinished, _Panel);
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
}
