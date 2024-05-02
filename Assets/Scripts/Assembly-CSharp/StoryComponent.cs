using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryComponent : MonoBehaviour
{
	[Serializable]
	private class AnimationAndName
	{
		public Animation _Anim;

		public string _Name;
	}

	public List<TimedSpriteAnimation> _SpriteAnimationTimes;

	public List<TimedSpriteAnimation> _StoryCompleteSpriteAnimationTimes;

	private int _nextSpriteAnimatiom;

	private Animation[] _allUnityAnimations;

	private List<AnimationAndName> _mainStoryAnimations;

	private List<MaterialAnim> _materialAnimations;

	private List<AnimatedSprite> _spriteAnimations;

	private List<AnimationAndName>[] _introAnimations;

	private List<AnimationAndName> _preAnimations;

	private bool _waitingForPreamble;

	private List<AnimationAndName> _storyCompleteAnimations;

	private bool _storySetToEnd;

	protected virtual List<AnimatedSprite> SpriteAnimations
	{
		get
		{
			return _spriteAnimations;
		}
	}

	private void Awake()
	{
		StoryController.StoryControllerReadyToStart += OnStroyControllerReady;
		GetUnityAnimations();
		GetMaterialAnimations();
		GetSpriteAnimations();
	}

	private void OnDestroy()
	{
		StoryController.StoryControllerReadyToStart -= OnStroyControllerReady;
	}

	private void OnStroyControllerReady()
	{
		StoryController.Instance.RegisterComponent(this);
	}

	public void ShowStory()
	{
		PlayPreambleAnimations();
	}

	public void PlayStoryPreamble()
	{
		PlayPreambleAnimations();
	}

	public void PlayStoryProper()
	{
		PlayUnityAnimations(_mainStoryAnimations);
		_nextSpriteAnimatiom = 0;
		_storySetToEnd = false;
	}

	public void PlayStoryComplete()
	{
		PlayUnityAnimations(_storyCompleteAnimations);
		_nextSpriteAnimatiom = 0;
		_storySetToEnd = false;
	}

	public void PlayIntroToFrontend()
	{
		PlayIntroAnimations();
	}

	public bool UpdatePreamble()
	{
		if (!PreambleFinished())
		{
			return true;
		}
		return false;
	}

	public bool UpdateStoryComplete(float time)
	{
		bool result = false;
		foreach (AnimationAndName storyCompleteAnimation in _storyCompleteAnimations)
		{
			if (storyCompleteAnimation._Anim.IsPlaying(storyCompleteAnimation._Name))
			{
				result = true;
				break;
			}
		}
		UpdateSpriteAnimations(_StoryCompleteSpriteAnimationTimes, time);
		return result;
	}

	public bool UpdateStory(float time)
	{
		bool result = false;
		foreach (AnimationAndName mainStoryAnimation in _mainStoryAnimations)
		{
			if (mainStoryAnimation._Anim.IsPlaying(mainStoryAnimation._Name))
			{
				result = true;
				break;
			}
		}
		if (!_storySetToEnd)
		{
			foreach (MaterialAnim materialAnimation in _materialAnimations)
			{
				materialAnimation.UpdateAnim(time);
			}
		}
		UpdateSpriteAnimations(_SpriteAnimationTimes, time);
		return result;
	}

	public void EndStory()
	{
		foreach (AnimationAndName mainStoryAnimation in _mainStoryAnimations)
		{
			foreach (AnimationState item in mainStoryAnimation._Anim.animation)
			{
				if (item.enabled)
				{
					item.normalizedTime = 1f;
				}
			}
		}
		foreach (MaterialAnim materialAnimation in _materialAnimations)
		{
			materialAnimation.SetToEnd();
		}
		_storySetToEnd = true;
	}

	private void UpdateSpriteAnimations(List<TimedSpriteAnimation> spriteAnimationTimes, float time)
	{
		while (_nextSpriteAnimatiom < spriteAnimationTimes.Count && spriteAnimationTimes[_nextSpriteAnimatiom]._Time <= time)
		{
			PlaySpriteAnimationInChildren(spriteAnimationTimes[_nextSpriteAnimatiom]._Anim);
			if (spriteAnimationTimes[_nextSpriteAnimatiom]._PlayMatchingUnityAnimations)
			{
				PlayUnityAnimationInChildren(spriteAnimationTimes[_nextSpriteAnimatiom]._Anim);
			}
			_nextSpriteAnimatiom++;
		}
	}

	public bool FinishedPlayingIntro()
	{
		if (!PreambleFinished())
		{
			return false;
		}
		int indexForID = HillDatabase.Instance.GetIndexForID(CurrentHill.Instance.ID);
		foreach (AnimationAndName item in _introAnimations[indexForID])
		{
			if (item._Anim.IsPlaying(item._Name))
			{
				return false;
			}
		}
		return true;
	}

	private bool PreambleFinished()
	{
		foreach (AnimationAndName preAnimation in _preAnimations)
		{
			if (preAnimation._Anim.IsPlaying(preAnimation._Name))
			{
				return false;
			}
		}
		return true;
	}

	private void GetUnityAnimations()
	{
		_allUnityAnimations = GetComponentsInChildren<Animation>();
		_mainStoryAnimations = new List<AnimationAndName>(_allUnityAnimations.Length);
		_introAnimations = new List<AnimationAndName>[HillDatabase.NumHills];
		for (int i = 0; i < _introAnimations.Length; i++)
		{
			_introAnimations[i] = new List<AnimationAndName>(_allUnityAnimations.Length);
		}
		_preAnimations = new List<AnimationAndName>(_allUnityAnimations.Length);
		_storyCompleteAnimations = new List<AnimationAndName>(_allUnityAnimations.Length);
		Animation[] allUnityAnimations = _allUnityAnimations;
		foreach (Animation animation in allUnityAnimations)
		{
			foreach (AnimationState item in animation)
			{
				if (item.name.Contains("Story"))
				{
					_mainStoryAnimations.Add(new AnimationAndName
					{
						_Anim = animation,
						_Name = item.name
					});
				}
				for (int k = 0; k < _introAnimations.Length; k++)
				{
					string value = string.Format("Intro{0}", HillDatabase.Instance.GetDefintionFromIndex(k)._ID);
					bool flag = false;
					if (item.name.Contains(value))
					{
						flag = true;
					}
					else if (item.name.Contains("IntroX"))
					{
						flag = true;
					}
					if (flag)
					{
						_introAnimations[k].Add(new AnimationAndName
						{
							_Anim = animation,
							_Name = item.name
						});
					}
				}
				if (item.name.Contains("Preamble"))
				{
					_preAnimations.Add(new AnimationAndName
					{
						_Anim = animation,
						_Name = item.name
					});
				}
				if (item.name.Contains("complete"))
				{
					_storyCompleteAnimations.Add(new AnimationAndName
					{
						_Anim = animation,
						_Name = item.name
					});
				}
			}
		}
	}

	private void GetMaterialAnimations()
	{
		MaterialAnim[] componentsInChildren = GetComponentsInChildren<MaterialAnim>();
		_materialAnimations = new List<MaterialAnim>(componentsInChildren.Length);
		MaterialAnim[] array = componentsInChildren;
		foreach (MaterialAnim item in array)
		{
			_materialAnimations.Add(item);
		}
	}

	protected virtual void GetSpriteAnimations()
	{
		AnimatedSprite[] componentsInChildren = GetComponentsInChildren<AnimatedSprite>();
		_spriteAnimations = new List<AnimatedSprite>(componentsInChildren.Length);
		AnimatedSprite[] array = componentsInChildren;
		foreach (AnimatedSprite item in array)
		{
			_spriteAnimations.Add(item);
		}
	}

	private void PlayUnityAnimations(List<AnimationAndName> animations)
	{
		foreach (AnimationAndName animation in animations)
		{
			animation._Anim.Play(animation._Name);
		}
	}

	private void PlaySpriteAnimationInChildren(string animName)
	{
		foreach (AnimatedSprite spriteAnimation in SpriteAnimations)
		{
			if (spriteAnimation.HasAnim(animName))
			{
				spriteAnimation.Play(animName);
			}
		}
	}

	private void PlayUnityAnimationInChildren(string animName)
	{
		Animation[] allUnityAnimations = _allUnityAnimations;
		foreach (Animation animation in allUnityAnimations)
		{
			if (animation[animName] != null)
			{
				animation.Play(animName);
			}
		}
	}

	private void PlayIntroAnimations()
	{
		PlayPreambleAnimations();
		int indexForID = HillDatabase.Instance.GetIndexForID(CurrentHill.Instance.ID);
		foreach (AnimationAndName item in _introAnimations[indexForID])
		{
			if (item._Anim.IsPlaying("Preamble"))
			{
				item._Anim.Play(item._Name, AnimationPlayMode.Queue);
			}
			else
			{
				item._Anim.Play(item._Name);
			}
		}
	}

	private void PlayPreambleAnimations()
	{
		foreach (AnimationAndName preAnimation in _preAnimations)
		{
			preAnimation._Anim.Play(preAnimation._Name);
		}
	}
}
