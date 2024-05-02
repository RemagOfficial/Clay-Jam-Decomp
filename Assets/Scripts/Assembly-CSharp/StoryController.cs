using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StoryController : ManagedComponent
{
	private enum State
	{
		NotPlaying = 0,
		PlayingPreamble = 1,
		Playing = 2,
		PlayingIntro = 3,
		PlayingComplete = 4,
		Done = 5
	}

	private List<StoryComponent> _components;

	private State CurrentState;

	private float _timeSincePlayStarted;

	public static StoryController Instance { get; private set; }

	public bool ReadyToRunFrontend
	{
		get
		{
			return CurrentState == State.Done;
		}
	}

	public bool PlayingCompleteStory
	{
		get
		{
			return CurrentState == State.PlayingComplete;
		}
	}

	[method: MethodImpl(32)]
	public static event Action StoryControllerReadyToStart;

	[method: MethodImpl(32)]
	public static event Action StoryStartedEvent;

	[method: MethodImpl(32)]
	public static event Action StoryFinishedEvent;

	[method: MethodImpl(32)]
	public static event Action StoryCompleteStartedEvent;

	[method: MethodImpl(32)]
	public static event Action StoryCompleteFinishedEvent;

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("StoryController created more than once", base.gameObject);
		}
		Instance = this;
		_components = new List<StoryComponent>();
		CurrentState = State.NotPlaying;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public override void ResetForRun()
	{
		if (StoryController.StoryControllerReadyToStart != null)
		{
			StoryController.StoryControllerReadyToStart();
		}
		base.enabled = true;
		if (!MetaGameController.Instance.LoadingFromBootup && SaveData.Instance.Hills.AllDefeated && !SaveData.Instance.Progress._HasPlayedStoryCompleteMovie.Set)
		{
			ShowStoryComplete();
			return;
		}
		if (!MetaGameController.Instance.LoadingFromBootup)
		{
			CurrentState = State.Done;
			return;
		}
		if (!SaveData.Instance.Progress.StoryHasBeenPlayed())
		{
			ShowStory();
			return;
		}
		MusicController.Instance.StartFrontend();
		ShowFrontend();
	}

	public void RegisterComponent(StoryComponent component)
	{
		if (!_components.Contains(component))
		{
			_components.Add(component);
		}
	}

	public void ShowStory()
	{
		foreach (StoryComponent component in _components)
		{
			component.PlayStoryPreamble();
		}
		_timeSincePlayStarted = 0f;
		CurrentState = State.PlayingPreamble;
		if (StoryController.StoryStartedEvent != null)
		{
			StoryController.StoryStartedEvent();
		}
	}

	public void ShowStoryComplete()
	{
		foreach (StoryComponent component in _components)
		{
			component.PlayStoryComplete();
		}
		_timeSincePlayStarted = 0f;
		CurrentState = State.PlayingComplete;
		if (StoryController.StoryCompleteStartedEvent != null)
		{
			StoryController.StoryCompleteStartedEvent();
		}
	}

	public void ShowFrontend()
	{
		foreach (StoryComponent component in _components)
		{
			component.PlayIntroToFrontend();
		}
		CurrentState = State.PlayingIntro;
	}

	public void Update()
	{
		if (CurrentState == State.PlayingIntro)
		{
			foreach (StoryComponent component in _components)
			{
				if (!component.FinishedPlayingIntro())
				{
					return;
				}
			}
			FinishPlayingIntro();
			return;
		}
		if (CurrentState == State.PlayingPreamble)
		{
			bool flag = false;
			foreach (StoryComponent component2 in _components)
			{
				flag |= component2.UpdatePreamble();
			}
			if (!flag)
			{
				FinishPlayingPreamble();
			}
		}
		if (CurrentState == State.Playing)
		{
			bool flag2 = false;
			foreach (StoryComponent component3 in _components)
			{
				flag2 |= component3.UpdateStory(_timeSincePlayStarted);
			}
			if (!flag2)
			{
				FinishPlayingStory();
			}
			_timeSincePlayStarted += Time.deltaTime;
		}
		if (CurrentState != State.PlayingComplete)
		{
			return;
		}
		bool flag3 = false;
		foreach (StoryComponent component4 in _components)
		{
			flag3 |= component4.UpdateStoryComplete(_timeSincePlayStarted);
		}
		if (!flag3)
		{
			FinishPlayingStoryComplete();
		}
		_timeSincePlayStarted += Time.deltaTime;
	}

	public void EndStory()
	{
		foreach (StoryComponent component in _components)
		{
			component.EndStory();
		}
	}

	private void FinishPlayingIntro()
	{
		CurrentState = State.Done;
	}

	private void FinishPlayingPreamble()
	{
		CurrentState = State.Playing;
		foreach (StoryComponent component in _components)
		{
			component.PlayStoryProper();
		}
	}

	private void FinishPlayingStory()
	{
		CurrentHill.Instance.ID = 1;
		CurrentHill.Instance.ProgressData._UpgradeLevelAnimated = 0;
		SaveData.Instance.Progress.MarkStoryPlayed();
		SaveData.Instance.MarkAsNeedToSave(false);
		CurrentState = State.Done;
		if (StoryController.StoryFinishedEvent != null)
		{
			StoryController.StoryFinishedEvent();
		}
	}

	private void FinishPlayingStoryComplete()
	{
		CurrentHill.Instance.ID = 1;
		CurrentHill.Instance.ProgressData._UpgradeLevelAnimated = 0;
		SaveData.Instance.Progress._HasPlayedStoryCompleteMovie.Set = true;
		SaveData.Instance.MarkAsNeedToSave(false);
		CurrentState = State.Done;
		if (StoryController.StoryCompleteFinishedEvent != null)
		{
			StoryController.StoryCompleteFinishedEvent();
		}
	}
}
