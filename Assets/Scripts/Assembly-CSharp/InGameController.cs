using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InGameController : MonoBehaviour
{
	public enum State
	{
		NotStarted = 0,
		Loading = 1,
		ResettingForRun = 2,
		WaitingToRoll = 3,
		RollingTop = 4,
		RollingApproach = 5,
		RollingFinalMoments = 6,
		Flying = 7,
		Landed = 8,
		ShowingResults = 9,
		ConsumedByAvalanche = 10,
		ShowingResultsGameOver = 11,
		JVP = 12
	}

	private int _PowerUpsRemainingAtStart;

	private AsyncOperation _hillLoader;

	public float _MinTipReadingTime = 3f;

	private float _runTime;

	public string _George;

	public static InGameController Instance { get; set; }

	public State CurrentState { get; private set; }

	public bool IsLoading
	{
		get
		{
			return CurrentState == State.Loading;
		}
	}

	public bool ReadyToShow
	{
		get
		{
			return CurrentState >= State.WaitingToRoll;
		}
	}

	public bool IsInEndSequence
	{
		get
		{
			return CurrentState >= State.Flying;
		}
	}

	public bool CanFlick
	{
		get
		{
			return CurrentState == State.RollingApproach || CurrentState == State.RollingFinalMoments;
		}
	}

	public bool IsRolling
	{
		get
		{
			return CurrentState == State.RollingFinalMoments || CurrentState == State.RollingApproach || CurrentState == State.RollingTop;
		}
	}

	public int MetersFlown { get; private set; }

	public ClayCollection ClayCollectedThisRun { get; private set; }

	private bool DoneBestScoreEvent { get; set; }

	public float CurrentRunTime { get; private set; }

	public float RunTimeAtFinish { get; private set; }

	public bool Paused { get; private set; }

	public bool PausedGouges { get; private set; }

	public bool Loaded { get; private set; }

	private bool ScenesLoaded { get; set; }

	[method: MethodImpl(32)]
	public static event Action<State> StateChanged;

	[method: MethodImpl(32)]
	public static event Action ReturnToFrontendRequested;

	[method: MethodImpl(32)]
	public static event Action PlayPressedEvent;

	[method: MethodImpl(32)]
	public static event Action PausedEvent;

	[method: MethodImpl(32)]
	public static event Action UnpausedEvent;

	[method: MethodImpl(32)]
	public static event Action NewBestScoreEvent;

	[method: MethodImpl(32)]
	public static event Action RunStarted;

	[method: MethodImpl(32)]
	public static event Action FlickZoneEntered;

	[method: MethodImpl(32)]
	public static event Action RunCompletedSuccess;

	[method: MethodImpl(32)]
	public static event Action AskPowerPlayQuestionEvent;

	private void Awake()
	{
		Instance = this;
		BossMonster.BossHitComplete += OnBossHitAnimationFinished;
		UIEvents.ReturnToFrontend += OnReturnToFrontendRequested;
		FlyingGUIController.FinishedAnimatingResults += FinishedAnimatingResults;
		FlyingGUIController.ReturnedToResults += OnReturnedToResults;
	}

	private void OnDestroy()
	{
		BossMonster.BossHitComplete -= OnBossHitAnimationFinished;
		UIEvents.ReturnToFrontend -= OnReturnToFrontendRequested;
		FlyingGUIController.FinishedAnimatingResults -= FinishedAnimatingResults;
		FlyingGUIController.ReturnedToResults -= OnReturnedToResults;
	}

	private void Start()
	{
		CloseDown();
	}

	private void Update()
	{
		switch (CurrentState)
		{
		case State.NotStarted:
			UpdateNotStarted();
			break;
		case State.Loading:
			UpdateLoading();
			break;
		case State.ResettingForRun:
			UpdateResetting();
			break;
		case State.WaitingToRoll:
			UpdateWaitingToRoll();
			break;
		case State.RollingTop:
			UpdateRollingTop();
			break;
		case State.RollingApproach:
			UpdateRollingApproach();
			break;
		case State.Flying:
			UpdateFlying();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case State.RollingFinalMoments:
		case State.Landed:
		case State.ShowingResults:
		case State.ConsumedByAvalanche:
		case State.ShowingResultsGameOver:
		case State.JVP:
			break;
		}
		if (!Paused)
		{
			CurrentRunTime += Time.deltaTime;
		}
	}

	public void CloseDown()
	{
		if ((bool)InGameComponentManager.Instance)
		{
			InGameComponentManager.Instance.Unload();
		}
		if ((bool)HillComponentManager.Instance)
		{
			HillComponentManager.Instance.Unload();
		}
		if ((bool)JVPComponentManager.Instance)
		{
			JVPComponentManager.Instance.CloseDown();
		}
		GC.Collect();
		Resources.UnloadUnusedAssets();
		SetState(State.NotStarted);
	}

	public void Pause(bool pauseGouges = true)
	{
		Paused = true;
		PausedGouges = pauseGouges;
		if (InGameController.PausedEvent != null)
		{
			InGameController.PausedEvent();
		}
	}

	public void Unpause()
	{
		Paused = false;
		PausedGouges = false;
		if (InGameController.UnpausedEvent != null)
		{
			InGameController.UnpausedEvent();
		}
	}

	public void StartRun()
	{
		if (BuildDetails.Instance._DemoMode)
		{
			CurrentHill.Instance.ProgressData._PowerPlaysRemaining = 1;
		}
		if (!InGameComponentsAreLoaded())
		{
			StartLoading();
		}
		else
		{
			ResetForRun();
		}
	}

	public void QuestShown()
	{
		if (CurrentState != State.WaitingToRoll)
		{
			Debug.LogError(string.Format("QuestShown called in wrong game state {0}", CurrentState));
		}
		if (CurrentHill.Instance.ProgressData._PowerPlaysRemaining > 0)
		{
			AskPowerPlayQuestion();
		}
		else
		{
			Run();
		}
	}

	private void StartLoading()
	{
		Loaded = false;
		SetState(State.Loading);
		StartCoroutine(LoadAsync());
	}

	private IEnumerator LoadAsync()
	{
		int startFrame = Time.frameCount;
		ScenesLoaded = false;
		string sceneName2 = BuildDetails.Instance.HillGenericSceneName;
		_hillLoader = Application.LoadLevelAdditiveAsync(sceneName2);
		yield return _hillLoader;
		sceneName2 = string.Format("hill{0}", CurrentHill.Instance.ID);
		_hillLoader = Application.LoadLevelAdditiveAsync(sceneName2);
		yield return _hillLoader;
		if (startFrame + 1 >= Time.frameCount)
		{
			yield return new WaitForEndOfFrame();
		}
		ScenesLoaded = true;
	}

	private void SetState(State newState)
	{
		CurrentState = newState;
		if (InGameController.StateChanged != null)
		{
			InGameController.StateChanged(newState);
		}
		if (InGameController.RunStarted != null && newState == State.RollingTop)
		{
			InGameController.RunStarted();
		}
		if (InGameController.FlickZoneEntered != null && newState == State.RollingApproach)
		{
			InGameController.FlickZoneEntered();
		}
	}

	private void OnReturnToFrontendRequested(GameObject panel)
	{
		InGameController.ReturnToFrontendRequested();
	}

	private void UpdateNotStarted()
	{
	}

	private void UpdateLoading()
	{
		if (ScenesLoaded && InGameComponentManager.Instance.Initialise() && HillComponentManager.Instance.Initialise())
		{
			Loaded = true;
			ResetForRun();
		}
	}

	public void Restart()
	{
		ResetForRun(true);
		OnFailedRun();
	}

	public void ResetForRun(bool immediate = false)
	{
		MetersFlown = 0;
		ClayCollectedThisRun = null;
		DoneBestScoreEvent = false;
		_PowerUpsRemainingAtStart = CurrentHill.Instance.ProgressData._PowerPlaysRemaining;
		Pebble.Instance.ResetForRun();
		Pause();
		SetState(State.ResettingForRun);
		InGameComponentManager.Instance.ResetForRun();
		HillComponentManager.Instance.ResetForRun();
		JVPComponentManager.Instance.ResetForRun();
		JVPController.Instance.HideFromResultsScreen();
		if (!immediate)
		{
			_runTime = Time.time + _MinTipReadingTime;
		}
	}

	private void UpdateResetting()
	{
		if (!(Time.time < _runTime))
		{
			ShowQuests();
		}
	}

	private void ShowQuests()
	{
		SetState(State.WaitingToRoll);
	}

	private void UpdateWaitingToRoll()
	{
	}

	private void AskPowerPlayQuestion()
	{
		if (InGameController.AskPowerPlayQuestionEvent != null)
		{
			InGameController.AskPowerPlayQuestionEvent();
		}
		else
		{
			Run();
		}
	}

	public void OnPowerPlayQuestionAnswered()
	{
		Run();
	}

	private void Run()
	{
		Unpause();
		SetState(State.RollingTop);
		InGameComponentManager.Instance.Run();
		HillComponentManager.Instance.Run();
		JVPComponentManager.Instance.Run();
		CurrentRunTime = 0f;
	}

	private void UpdateRollingTop()
	{
		if (HillCollapser.Instance.ReachedPebble)
		{
			SetState(State.ConsumedByAvalanche);
			return;
		}
		float progress = Pebble.Instance.Progress;
		if (CurrentHill.Instance.ProgressIsBeyondHorizon(progress))
		{
			SetState(State.RollingApproach);
		}
	}

	private void UpdateRollingApproach()
	{
		float progress = Pebble.Instance.Progress;
		if (progress > CurrentHill.Instance.FinalMomentsProgress)
		{
			SetState(State.RollingFinalMoments);
		}
	}

	private void OnBossHitAnimationFinished()
	{
		StartFlying();
	}

	private void StartFlying()
	{
		EndRun();
		SetState(State.Flying);
	}

	public void OnPebbleDead()
	{
		EndRun();
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			CollectHearts();
			if (InGameController.RunCompletedSuccess != null)
			{
				InGameController.RunCompletedSuccess();
			}
		}
		else
		{
			OnFailedRun();
		}
		SaveRunData();
		SetState(State.ShowingResultsGameOver);
	}

	private void EndRun()
	{
		if (_PowerUpsRemainingAtStart != CurrentHill.Instance.ProgressData._PowerPlaysRemaining)
		{
			AnalyticsController.Instance.UsePowerUp();
		}
		RunTimeAtFinish = CurrentRunTime;
	}

	private void OnMetersFlown(int meters)
	{
		MetersFlown += meters;
		if (!DoneBestScoreEvent && (float)MetersFlown > CurrentHill.Instance.ProgressData._BestScore)
		{
			if (InGameController.NewBestScoreEvent != null)
			{
				InGameController.NewBestScoreEvent();
			}
			DoneBestScoreEvent = true;
		}
	}

	public void MeterCounted()
	{
		if (MetersFlown <= 0)
		{
			Debug.LogError("Meters shouldnt be counted if there aren't any");
		}
		else
		{
			MetersFlown--;
		}
	}

	private void UpdateFlying()
	{
		if (Pebble.Instance.FlightProgress > MetersFlown)
		{
			OnMetersFlown(Pebble.Instance.FlightProgress - MetersFlown);
		}
		if (Pebble.Instance.Landed)
		{
			if (Pebble.Instance.DistanceToFly != MetersFlown)
			{
				Debug.LogWarning(string.Format("Predicted flight of {0} didn't match flown: {1}", Pebble.Instance.DistanceToFly, MetersFlown));
				OnMetersFlown(Pebble.Instance.DistanceToFly - MetersFlown);
			}
			Landed();
		}
	}

	private void Landed()
	{
		RecordClay();
		if (InGameController.RunCompletedSuccess != null)
		{
			InGameController.RunCompletedSuccess();
		}
		SaveRunData();
		SetState(State.Landed);
	}

	private void CollectHearts()
	{
		ClayCollectedThisRun = GameModeStateMonsterLove.Instance.ConvertHeartsToClayCollection();
		SaveData.Instance.AddClay(ClayCollectedThisRun);
	}

	private void RecordClay()
	{
		ClayCollectedThisRun = Pebble.Instance.ClayCollected;
		SaveData.Instance.AddClay(ClayCollectedThisRun);
	}

	private void RecordHillStats()
	{
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			if (Pebble.Instance.MaxProgress > CurrentHill.Instance.ProgressData._BestScoreMonsterLove && InGameController.NewBestScoreEvent != null)
			{
				InGameController.NewBestScoreEvent();
			}
			SaveData.Instance.Hills.RunCompleteForHill_MonsterLove(CurrentHill.Instance.ID, Pebble.Instance.MaxProgress);
		}
		else
		{
			SaveData.Instance.Hills.RunCompleteForHill(CurrentHill.Instance.ID, RunTimeAtFinish, MetersFlown);
		}
	}

	private bool InGameComponentsAreLoaded()
	{
		if (InGameComponentManager.Instance == null)
		{
			return false;
		}
		if (HillComponentManager.Instance == null)
		{
			return false;
		}
		return true;
	}

	private void FinishedAnimatingResults()
	{
		SetState(State.ShowingResults);
	}

	private void OnReturnedToResults()
	{
		SetState(State.ShowingResults);
	}

	private void SaveRunData()
	{
		SaveData.Instance.Progress._FinishedOneLevel.Set = true;
		RecordHillStats();
		SaveData.Instance.MarkAsNeedToSave(false);
	}

	public void GoToShop()
	{
		JVPController.Instance.ZoomInFromResultsScreen();
		SetState(State.JVP);
	}

	public void PressedPlay()
	{
		if (InGameController.PlayPressedEvent != null)
		{
			InGameController.PlayPressedEvent();
		}
	}

	private void OnFailedRun()
	{
		if (CurrentGameMode.Type == GameModeType.Quest && CurrentQuest.Instance.HasQuest)
		{
			CurrentQuest.Instance.MarkFailed();
		}
	}
}
