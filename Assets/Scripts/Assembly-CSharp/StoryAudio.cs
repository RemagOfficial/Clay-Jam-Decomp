using System.Collections.Generic;
using Fabric;
using UnityEngine;

public class StoryAudio : ManagedComponent
{
	private const string MusicPrefab = "StoryMusic";

	private const string StoryCompleteMusicPrefab = "StoryCompleteMusic";

	private const string ResourcePathMusic = "Sounds/Story/";

	private const string ResourcePathSFX = "Sounds/Story/StorySFX";

	private const string HPathMusic = "Audio_-Music";

	private const string HPathSFX = "Audio_-SFX_-Story";

	private const string MusicEvent = "StoryMusic";

	private const string StoryCompleteMusicEvent = "StoryCompleteMusic";

	public List<StorySFX> _SFX;

	private List<StorySFX> _playing;

	private List<StorySFX> _toPlay;

	private float _playTime;

	private void Awake()
	{
		StoryController.StoryStartedEvent += OnStoryStarted;
		StoryController.StoryFinishedEvent += OnStoryFinished;
		StoryController.StoryCompleteStartedEvent += OnStoryCompleteStarted;
		StoryController.StoryCompleteFinishedEvent += OnStoryCompleteFinished;
		Load();
	}

	private void OnDestroy()
	{
		StoryController.StoryStartedEvent -= OnStoryStarted;
		StoryController.StoryFinishedEvent -= OnStoryFinished;
		StoryController.StoryCompleteStartedEvent -= OnStoryCompleteStarted;
		StoryController.StoryCompleteFinishedEvent -= OnStoryCompleteFinished;
		Unload();
	}

	private void Update()
	{
		_playTime += Time.deltaTime;
		foreach (StorySFX item in _SFX)
		{
			item.Update(_playTime);
		}
	}

	private void StartPlaying(string eventName)
	{
		MusicController.Instance.StopAll();
		InGameAudio.PostFabricEvent(eventName, EventAction.PlaySound, null, null, InGameAudio.AudioType.Music);
		_playTime = 0f;
	}

	private void StopPlaying(string eventName)
	{
		InGameAudio.PostFabricEvent(eventName, EventAction.StopSound, null, null, InGameAudio.AudioType.Music);
		MusicController.Instance.StartFrontend();
	}

	private void OnStoryStarted()
	{
		base.enabled = true;
		StartPlaying("StoryMusic");
	}

	private void OnStoryFinished()
	{
		StopPlaying("StoryMusic");
		base.enabled = false;
	}

	private void OnStoryCompleteStarted()
	{
		base.enabled = true;
		StartPlaying("StoryCompleteMusic");
	}

	private void OnStoryCompleteFinished()
	{
		StopPlaying("StoryCompleteMusic");
		base.enabled = false;
	}

	private void Unload()
	{
		if (!(FabricManager.Instance == null))
		{
			string componentName = string.Format("{0}_{1}", "Audio_-Music", "StoryMusic");
			FabricManager.Instance.UnloadAsset(componentName);
			componentName = string.Format("{0}_{1}", "Audio_-Music", "StoryCompleteMusic");
			FabricManager.Instance.UnloadAsset(componentName);
			FabricManager.Instance.UnloadAsset("Audio_-SFX_-Story");
		}
	}

	private void Load()
	{
		string prefabName = string.Format("{0}{1}", "Sounds/Story/", "StoryMusic");
		FabricManager.Instance.LoadAsset(prefabName, "Audio_-Music");
		prefabName = string.Format("{0}{1}", "Sounds/Story/", "StoryCompleteMusic");
		FabricManager.Instance.LoadAsset(prefabName, "Audio_-Music");
		FabricManager.Instance.LoadAsset("Sounds/Story/StorySFX", "Audio_-SFX_-Story");
	}
}
