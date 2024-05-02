using System;
using UnityEngine;

[Serializable]
public class ObjectAsButtonButton
{
	private enum State
	{
		In = 0,
		Highlight = 1,
		Out = 2,
		Idle = 3
	}

	public GameObject _GameObject;

	public UnityEngine.Object _AnimPrefab;

	private AnimatedSprite _spriteAnimation;

	private bool _shouldPlayIdleAnim;

	public string _InAnimationName = "In";

	public string _OutAnimationName = "Out";

	public string _IdleAnimationName = "Active";

	private State CurrentState;

	public bool ShouldPlayIdleAnim
	{
		set
		{
			if (_shouldPlayIdleAnim == value)
			{
				return;
			}
			_shouldPlayIdleAnim = value;
			CurrentState = State.Out;
			AnimateIdle();
			if (_spriteAnimation != null)
			{
				if (!_shouldPlayIdleAnim)
				{
					_spriteAnimation.ShowFrame(0);
				}
				else
				{
					_spriteAnimation.PlayForward();
				}
			}
		}
	}

	public bool IsIn
	{
		get
		{
			return CurrentState == State.In;
		}
	}

	public void Activate(bool active)
	{
		if (!(_GameObject.collider == null))
		{
			_GameObject.collider.enabled = active;
		}
	}

	public void PopOut()
	{
		if (CurrentState != State.Out)
		{
			AnimateOut();
			CurrentState = State.Out;
		}
	}

	public void PopIn()
	{
		if (CurrentState != 0)
		{
			if (CurrentState != State.Highlight)
			{
				AnimateIn();
			}
			CurrentState = State.In;
		}
	}

	public void Highlight()
	{
		if (CurrentState != State.Highlight && CurrentState != 0)
		{
			AnimateIn();
			CurrentState = State.Highlight;
		}
	}

	public bool UpdateState(string mouseOverButtonName)
	{
		if (mouseOverButtonName == _GameObject.name)
		{
			if (ClayJamInput.CursorButtonUp())
			{
				PopIn();
				return true;
			}
			Highlight();
		}
		else if (CurrentState == State.Highlight)
		{
			PopOut();
		}
		else if (CurrentState != 0)
		{
			AnimateIdle();
		}
		return false;
	}

	public void Initialise(int layer)
	{
		MakeInteractive(layer);
		LoadSpriteAnimation();
		if ((bool)_spriteAnimation && _spriteAnimation.HasAnim(_OutAnimationName))
		{
			_spriteAnimation.Play(_OutAnimationName);
		}
		CurrentState = State.Out;
	}

	private void MakeInteractive(int layer)
	{
		FrontendController.MakeGameObjectInteractive(_GameObject, layer);
	}

	private void LoadSpriteAnimation()
	{
		if (_AnimPrefab != null)
		{
			LoadSpriteAnimationFromPrefab();
		}
		else
		{
			LoadSpriteAnimationOnObject();
		}
	}

	private void LoadSpriteAnimationFromPrefab()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_AnimPrefab) as GameObject;
		gameObject.transform.parent = _GameObject.transform;
		_spriteAnimation = gameObject.GetComponent<AnimatedSprite>();
		_spriteAnimation.Material = _GameObject.renderer.material;
	}

	private void LoadSpriteAnimationOnObject()
	{
		_spriteAnimation = _GameObject.GetComponent<AnimatedSprite>();
	}

	private void AnimateOut()
	{
		if ((bool)_spriteAnimation && _spriteAnimation.HasAnim(_OutAnimationName))
		{
			_spriteAnimation.Play(_OutAnimationName);
		}
		InGameAudio.PostFabricEvent("ButtonOut");
	}

	private void AnimateIn()
	{
		if ((bool)_spriteAnimation && _spriteAnimation.HasAnim(_InAnimationName))
		{
			_spriteAnimation.Play(_InAnimationName);
		}
		InGameAudio.PostFabricEvent("ButtonIn");
	}

	private void AnimateIdle()
	{
		if (_shouldPlayIdleAnim && CurrentState != State.Idle && CurrentState != State.Highlight && (bool)_spriteAnimation && _spriteAnimation.Finished && _spriteAnimation.HasAnim(_IdleAnimationName))
		{
			_spriteAnimation.Play(_IdleAnimationName);
			CurrentState = State.Idle;
		}
	}
}
