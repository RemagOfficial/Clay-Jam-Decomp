using System;
using UnityEngine;

[Serializable]
public class FrontendWorldPadlock
{
	public Animation _ModelAnimation;

	public AnimatedSprite _SpriteAnimation;

	public void Unlock()
	{
		InGameAudio.PostFabricEvent("Trumpet");
		if (!(_ModelAnimation == null))
		{
			_ModelAnimation.gameObject.SetActiveRecursively(true);
			_ModelAnimation.Play("PadlockOpen");
			_SpriteAnimation.Play("PadlockOpen");
		}
	}

	public void SetLocked()
	{
		if (!(_ModelAnimation == null))
		{
			_ModelAnimation.gameObject.SetActiveRecursively(true);
			_ModelAnimation.Play("Default");
			_SpriteAnimation.Play("PadlockClosed");
		}
	}

	public void SetUnlocked()
	{
		if (!(_ModelAnimation == null))
		{
			_ModelAnimation.gameObject.SetActiveRecursively(false);
		}
	}
}
