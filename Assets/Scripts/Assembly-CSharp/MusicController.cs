using Fabric;
using UnityEngine;

public class MusicController : MonoBehaviour
{
	private bool _playingFrontend;

	private bool _playingInGame;

	public static MusicController Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError(string.Format("More than one instance of MusicController,", base.gameObject));
		}
		Instance = this;
	}

	public void StartFrontend()
	{
		StopInGame();
		if (!_playingFrontend)
		{
			_playingFrontend = InGameAudio.PostFabricEvent("FrontendMusic", EventAction.PlaySound, null, null, InGameAudio.AudioType.Music);
		}
	}

	public void StopFrontend()
	{
		InGameAudio.PostFabricEvent("FrontendMusic", EventAction.StopSound, null, null, InGameAudio.AudioType.Music);
		_playingFrontend = false;
	}

	public void StartInGame()
	{
		StopFrontend();
		if (!_playingInGame)
		{
			_playingInGame = InGameAudio.PostFabricEvent("InGameMusic", EventAction.PlaySound, null, null, InGameAudio.AudioType.Music);
		}
	}

	public void StopInGame()
	{
		InGameAudio.PostFabricEvent("InGameMusic", EventAction.StopSound, null, null, InGameAudio.AudioType.Music);
		_playingInGame = false;
	}

	public void StopAll()
	{
		StopInGame();
		StopFrontend();
	}
}
