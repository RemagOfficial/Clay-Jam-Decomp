using Fabric;
using UnityEngine;

public class GameEventsAudio : MonoBehaviour
{
	public enum FlickAudio
	{
		Weak = 0,
		Medium = 1,
		Strong = 2,
		Final = 3
	}

	private static string WhooshAudioEventName = "WhooshOffCliff";

	private static string FlyStartAudioEventName = "FlyStart";

	private static string CameraZoomAudioEventName = "CameraZoom";

	private static string PowerPlayLaunchEventName = "PowerPlayLaunch";

	private static string JacketEventName = "JacketOn";

	private static string SquashEventName = "SquashOn";

	private static string BestScoreEventName = "BestScore";

	private static string FlickBoostEventName = "FlickBoost";

	private static string GameOverEventName = "GameOver";

	private static string HeartCollectedEventName = "CollectedHeart";

	private static string HeartBrokenEventName = "HeartBreak";

	private static string HeartsMatchedEventName = "HeartsMatched";

	private static float WhooshAudioLengthAtLaunch = 1.7f;

	private bool _playedWhoosh;

	private bool _checkForWhoosh;

	private void Awake()
	{
		InGameController.StateChanged += OnStateChanged;
		InGameController.PausedEvent += OnPaused;
		InGameController.UnpausedEvent += OnUnpaused;
		ZoomLevel.NewZoomLevelEvent += OnCameraZoom;
		PowerPlayLauncher.PowerPlayLaunched += OnPowerPlayLaunched;
		PowerUpManager.SquashOnHandler += OnPowerUpSquash;
		PowerUpManager.SuitUpHandler += OnPowerUpJacket;
		InGameController.NewBestScoreEvent += OnNewBestScore;
		Pebble.NewFlickEvent += OnNewFlickBoost;
		GameModeStateMonsterLove.HeartCollectedEvent += OnHeartCollected;
		GameModeStateMonsterLove.MultiplierDownEvent += OnHeartBroken;
		GameModeStateMonsterLove.HeartsMatchedEvent += OnHeartsMatched;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnStateChanged;
		InGameController.PausedEvent -= OnPaused;
		InGameController.UnpausedEvent -= OnUnpaused;
		ZoomLevel.NewZoomLevelEvent -= OnCameraZoom;
		PowerPlayLauncher.PowerPlayLaunched -= OnPowerPlayLaunched;
		PowerUpManager.SquashOnHandler -= OnPowerUpSquash;
		PowerUpManager.SuitUpHandler -= OnPowerUpJacket;
		InGameController.NewBestScoreEvent -= OnNewBestScore;
		Pebble.NewFlickEvent -= OnNewFlickBoost;
		GameModeStateMonsterLove.HeartCollectedEvent -= OnHeartCollected;
		GameModeStateMonsterLove.MultiplierDownEvent -= OnHeartBroken;
		GameModeStateMonsterLove.HeartsMatchedEvent -= OnHeartsMatched;
	}

	private void FixedUpdate()
	{
		if (!_playedWhoosh && _checkForWhoosh && TimeToLaunch() < WhooshAudioLengthAtLaunch)
		{
			PlayWhooshOffCliff();
		}
	}

	private float TimeToLaunch()
	{
		float length = CurrentHill.Instance.Length;
		float progressSafe = Pebble.ProgressSafe;
		float speed = Pebble.Instance.Speed;
		float num = length - progressSafe;
		return num / speed;
	}

	private float TimeToFlying()
	{
		return TimeToLaunch();
	}

	private void PlayWhooshOffCliff()
	{
		InGameAudio.PostFabricEvent(WhooshAudioEventName);
		_playedWhoosh = true;
		_checkForWhoosh = false;
	}

	private void OnStateChanged(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.WaitingToRoll:
			MusicController.Instance.StartInGame();
			_playedWhoosh = false;
			break;
		case InGameController.State.RollingApproach:
			_checkForWhoosh = true;
			break;
		case InGameController.State.Flying:
			MusicController.Instance.StopAll();
			StartFlyingAudio();
			break;
		case InGameController.State.Landed:
			StopFlyingAudio();
			MusicController.Instance.StartFrontend();
			break;
		case InGameController.State.ConsumedByAvalanche:
			MusicController.Instance.StopAll();
			break;
		case InGameController.State.ShowingResultsGameOver:
			MusicController.Instance.StartFrontend();
			InGameAudio.PostFabricEvent(GameOverEventName);
			break;
		}
	}

	private void OnPaused()
	{
		if (InGameController.Instance.CurrentState == InGameController.State.Flying)
		{
			StopFlyingAudio();
		}
	}

	private void OnUnpaused()
	{
		if (InGameController.Instance.CurrentState == InGameController.State.Flying)
		{
			StartFlyingAudio();
		}
	}

	private void StartFlyingAudio()
	{
		InGameAudio.PostFabricEvent(FlyStartAudioEventName);
		if (Pebble.Instance.HasFlickBoosted)
		{
			OnNewFlickBoost(FlickAudio.Final);
		}
	}

	private void StopFlyingAudio()
	{
		InGameAudio.PostFabricEvent(FlyStartAudioEventName, EventAction.StopSound);
	}

	private void OnCameraZoom(int zoomLevel, bool zoomOut)
	{
		if (zoomOut)
		{
			InGameAudio.PostFabricEvent(CameraZoomAudioEventName, EventAction.SetSwitch, "CameraZoomOut");
		}
		else
		{
			InGameAudio.PostFabricEvent(CameraZoomAudioEventName, EventAction.SetSwitch, "CameraZoomIn");
		}
		InGameAudio.PostFabricEvent(CameraZoomAudioEventName, EventAction.PlaySound);
	}

	private void OnPowerPlayLaunched()
	{
		InGameAudio.PostFabricEvent(PowerPlayLaunchEventName, EventAction.PlaySound);
	}

	private void OnPowerUpSquash()
	{
		InGameAudio.PostFabricEvent(SquashEventName, EventAction.PlaySound);
	}

	private void OnPowerUpJacket()
	{
		InGameAudio.PostFabricEvent(JacketEventName, EventAction.PlaySound);
	}

	private void OnNewBestScore()
	{
		InGameAudio.PostFabricEvent(BestScoreEventName, EventAction.PlaySound);
	}

	private void OnNewFlickBoost(FlickAudio type)
	{
		string parameter = "FlickWeak";
		switch (type)
		{
		case FlickAudio.Weak:
			parameter = "FlickWeak";
			break;
		case FlickAudio.Medium:
			parameter = "FlickMedium";
			break;
		case FlickAudio.Strong:
			parameter = "FlickStrong";
			break;
		case FlickAudio.Final:
			parameter = "FlickFinal";
			break;
		}
		InGameAudio.PostFabricEvent(FlickBoostEventName, EventAction.SetSwitch, parameter);
		InGameAudio.PostFabricEvent(FlickBoostEventName, EventAction.PlaySound);
	}

	private void OnHeartCollected(int colourIndex, int amount)
	{
		InGameAudio.PostFabricEvent(HeartCollectedEventName, EventAction.PlaySound);
	}

	private void OnHeartBroken()
	{
		InGameAudio.PostFabricEvent(HeartBrokenEventName, EventAction.PlaySound);
	}

	private void OnHeartsMatched(int value)
	{
		InGameAudio.PostFabricEvent(HeartsMatchedEventName, EventAction.PlaySound);
	}
}
