using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FlyingGUIController : MonoBehaviour
{
	private enum ExitPath
	{
		NotSet = 0,
		Restart = 1,
		Exit = 2,
		Shop = 3
	}

	private enum ResultMode
	{
		Success = 0,
		Failure = 1,
		Unknown = 2
	}

	private enum State
	{
		Off = 0,
		QuestIntro = 1,
		InGame = 2,
		Flying = 3,
		StartGameOverSequence = 4,
		FinalGameOver = 5,
		QuestComplete = 6,
		SwitchToResults = 7,
		HighlightDistancePanel = 8,
		HighlightPowerPlayPanel = 9,
		CountDistanceForPowerPlays = 10,
		MoveClayCollectedPanels = 11,
		ConvertHeartsToClay = 12,
		CountClay = 13,
		FinalResults = 14,
		ReturnToResults = 15,
		JVP = 16,
		TransitionOut = 17
	}

	private delegate void StateUpdateFunction();

	private delegate void StateEnterFunction();

	private delegate void StateExitFunction();

	public GameObject _SquashedAmountHUDPrefab;

	public Transform _SquashedAmountHUDParent;

	public GameObject _DistancePanel;

	public GameObject _DistancePanelResultsBackground;

	public LocalisableText _DistanceTitleLabel;

	public UILabel _DistanceValueLabel;

	public LocalisableText _DistanceGameOverLabel;

	public GameObject _DistanceBestBadge;

	public LocalisableText _DistanceBestLabel;

	public GameObject _DistanceHighlightBorder;

	private string _DistancePanelTransInAnim = "DistancePanelTransIn";

	private string _DistancePanelTransOutAnim = "DistancePanelTransOut";

	private string _DistancePanelHighlightAnim = "DistanceShow";

	private string _DistancePanelNewBestAnim = "DistanceBest";

	private string _DistancePanelMonsterLoveStart = "DistanceLM";

	private string _DistancePanelMonsterLoveFinish = "DistanceLMTransOut";

	public PrizeGUIController _PrizeGUI;

	public GameObject _NextPowerPlayPanel;

	public Animation _FloatDownDistanceAnim;

	private string _NextPowerPlayPanelTransInAnim = "NextPPTransIn";

	private string _NextPowerPlayPanelTransOutAnim = "NextPPTransOut";

	private string _NextPowerPlayPanelHighlightAnim = "NextPPShow";

	public GameObject _ClayCollectedPanel;

	public UICurrentClayCollectionLabel _ClayCollectedValueLabel;

	private string _ClayCollectedRepositionAnim = "ClayCollectedShow";

	private string _ClayCollectedTransOutAnim = "ClayCollectedTransOut";

	private string _ClayCollectedLoseClayAnim = "CounterLoseClay";

	private string _ClayCollectedAnimReadyToConvert = "ClayCollectedHearts";

	public UISpriteAnimation _ClayCollectedSpriteAnim;

	public ClayCounterGUIController _ClayCounterGUI;

	public GameObject _ClayTotalPanel;

	public Animation _ClayTotalAnimation;

	public GameObject _BuyButton;

	private string _ClayTotalTransInAnim = "ClayTotalTransIn";

	private string _ClayTotalTransOutAnim = "ClayTotalTransOut";

	private string _ClayTotalFinished = "ClayTotalFinished";

	public GameObject _PowerupPanel;

	public GameObject _ButtonsPanel;

	public GameObject _StarBurstObject;

	public ParticleEmitter _StarBurstParticles;

	public string _StarBurstAnim = "StarBurst1";

	public QuestGUIController _QuestPanel;

	private MonsterLoveGUIController _monsterLoveGUI;

	private bool _gotNewBestScore;

	private bool _shownNewBestScore;

	private ExitPath _exitPath;

	public GameObject _PauseButton;

	private bool _skipRequested;

	private ResultMode _resultMode;

	private StateUpdateFunction[] StateUpdateFn;

	private StateEnterFunction[] StateEnterFn;

	private StateExitFunction[] StateExitFn;

	private int _lastHillLevelUsedForRatePrompt;

	private float _stateTimer;

	private State _nextState;

	private bool _jvpPositionedMidFlight;

	private bool _zoomInPlayed;

	public static FlyingGUIController Instance { get; set; }

	private string ButtonsTransInAnim
	{
		get
		{
			return (!BuildDetails.Instance._UsingPCAssets) ? "ResultButtonTransIn" : "ResultButtonTransIn_PC";
		}
	}

	private string ButtonsTransOutAnim
	{
		get
		{
			return (!BuildDetails.Instance._UsingPCAssets) ? "ResultButtonTransOut" : "ResultButtonTransOut_PC";
		}
	}

	private State CurrentState { get; set; }

	[method: MethodImpl(32)]
	public static event Action FinishedAnimatingResults;

	[method: MethodImpl(32)]
	public static event Action ReturnedToResults;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one fflyingguicontroller instance");
		}
		Instance = this;
		InGameController.StateChanged += OnInGameStateChanged;
		InGameController.NewBestScoreEvent += OnNewBestScore;
		InGameController.PausedEvent += OnGamePaused;
		InGameController.UnpausedEvent += OnGameUnpaused;
		Pebble.ClayBouncedOffEvent += OnClayBouncedOff;
		UIEvents.ButtonPressed += OnButtonPressed;
		_monsterLoveGUI = GetComponent<MonsterLoveGUIController>();
		int length = Enum.GetValues(typeof(State)).Length;
		StateEnterFn = new StateEnterFunction[length];
		StateUpdateFn = new StateUpdateFunction[length];
		StateExitFn = new StateExitFunction[length];
		StateEnterFn[0] = Enter_Off;
		StateUpdateFn[0] = Update_Off;
		StateExitFn[0] = Exit_Off;
		StateEnterFn[1] = Enter_QuestIntro;
		StateUpdateFn[1] = Update_QuestIntro;
		StateExitFn[1] = Exit_QuestIntro;
		StateEnterFn[6] = Enter_QuestComplete;
		StateUpdateFn[6] = Update_QuestComplete;
		StateExitFn[6] = Exit_QuestComplete;
		StateEnterFn[2] = Enter_InGame;
		StateUpdateFn[2] = Update_InGame;
		StateExitFn[2] = Exit_InGame;
		StateEnterFn[3] = Enter_Flying;
		StateUpdateFn[3] = Update_Flying;
		StateExitFn[3] = Exit_Flying;
		StateEnterFn[4] = Enter_StartGameOverSequence;
		StateUpdateFn[4] = Update_StartGameOverSequence;
		StateExitFn[4] = Exit_StartGameOverSequence;
		StateEnterFn[5] = Enter_FinalGameOver;
		StateUpdateFn[5] = Update_FinalGameOver;
		StateExitFn[5] = Exit_FinalGameOver;
		StateEnterFn[7] = Enter_SwitchToResults;
		StateUpdateFn[7] = Update_SwitchToResults;
		StateExitFn[7] = Exit_SwitchToResults;
		StateExitFn[17] = Exit_TransitionOut;
		StateEnterFn[8] = Enter_HighlightDistancePanel;
		StateUpdateFn[8] = Update_HighlightDistancePanel;
		StateExitFn[8] = Exit_HighlightDistancePanel;
		StateEnterFn[9] = Enter_HighlightPowerPlayPanel;
		StateUpdateFn[9] = Update_HighlightPowerPlayPanel;
		StateExitFn[9] = Exit_HighlightPowerPlayPanel;
		StateEnterFn[10] = Enter_CountDistanceForPowerPlays;
		StateUpdateFn[10] = Update_CountDistanceForPowerPlays;
		StateExitFn[10] = Exit_CountDistanceForPowerPlays;
		StateEnterFn[11] = Enter_MoveClayCollectedPanels;
		StateUpdateFn[11] = Update_MoveClayCollectedPanels;
		StateExitFn[11] = Exit_MoveClayCollectedPanels;
		StateEnterFn[12] = Enter_ConvertHeartsToClay;
		StateUpdateFn[12] = Update_ConvertHeartsToClay;
		StateExitFn[12] = Exit_ConvertHeartsToClay;
		StateEnterFn[13] = Enter_CountClay;
		StateUpdateFn[13] = Update_CountClay;
		StateExitFn[13] = Exit_CountClay;
		StateEnterFn[14] = Enter_FinalResults;
		StateUpdateFn[14] = Update_FinalResults;
		StateExitFn[14] = Exit_FinalResults;
		StateEnterFn[15] = Enter_ReturnToResults;
		StateUpdateFn[15] = Update_ReturnToResults;
		StateExitFn[15] = Exit_ReturnToResults;
		StateEnterFn[17] = Enter_TransitionOut;
		StateUpdateFn[17] = Update_TransitionOut;
		StateExitFn[17] = Exit_TransitionOut;
		_lastHillLevelUsedForRatePrompt = 1;
	}

	private void OnDestroy()
	{
		InGameController.NewBestScoreEvent -= OnNewBestScore;
		InGameController.StateChanged -= OnInGameStateChanged;
		InGameController.PausedEvent -= OnGamePaused;
		InGameController.UnpausedEvent -= OnGameUnpaused;
		Pebble.ClayBouncedOffEvent -= OnClayBouncedOff;
		UIEvents.ButtonPressed -= OnButtonPressed;
	}

	private void OnEnable()
	{
		_stateTimer = 0f;
		CurrentState = State.Off;
		_exitPath = ExitPath.NotSet;
		if (InGameController.Instance.IsInEndSequence)
		{
			JVPController.Instance.ZoomOutToResultsScreen();
			if (_resultMode == ResultMode.Failure)
			{
				ChangeState(State.StartGameOverSequence);
			}
			else
			{
				ChangeState(State.ReturnToResults);
			}
			if (FlyingGUIController.ReturnedToResults != null)
			{
				FlyingGUIController.ReturnedToResults();
			}
		}
		else if (InGameController.Instance.CurrentState == InGameController.State.WaitingToRoll)
		{
			ChangeState(State.QuestIntro);
		}
		else
		{
			ChangeState(State.InGame);
		}
	}

	private void Update()
	{
		if (ClayJamInput.AnythingPressed)
		{
			_skipRequested = true;
		}
		_stateTimer += Time.deltaTime;
		if (_nextState != CurrentState)
		{
			StateExitFn[(int)CurrentState]();
			StateEnterFn[(int)_nextState]();
			CurrentState = _nextState;
			_stateTimer = 0f;
		}
		StateUpdateFn[(int)CurrentState]();
	}

	private void ChangeState(State newState)
	{
		_nextState = newState;
	}

	private void Enter_Off()
	{
	}

	private void Update_Off()
	{
	}

	private void Exit_Off()
	{
	}

	private void Enter_QuestIntro()
	{
		if (CurrentGameMode.Type != 0 || CurrentQuest.Instance.HasQuest)
		{
			_QuestPanel.ShowIntro();
		}
	}

	private void Update_QuestIntro()
	{
		if (!_QuestPanel.Finished)
		{
			_QuestPanel.Update();
		}
		else
		{
			ChangeState(State.InGame);
		}
	}

	private void Exit_QuestIntro()
	{
		InGameController.Instance.QuestShown();
	}

	private void Enter_QuestComplete()
	{
		_QuestPanel.ShowComplete();
	}

	private void Update_QuestComplete()
	{
		if (!_QuestPanel.Finished)
		{
			_QuestPanel.Update();
		}
		else
		{
			ChangeState(State.SwitchToResults);
		}
	}

	private void Exit_QuestComplete()
	{
		_skipRequested = false;
	}

	private void Enter_InGame()
	{
		_resultMode = ResultMode.Unknown;
		_gotNewBestScore = false;
		_PowerupPanel.SetActiveRecursively(true);
		switch (CurrentGameMode.Type)
		{
		case GameModeType.Quest:
			Enter_InGame_QuestMode();
			break;
		case GameModeType.MonsterLove:
			Enter_InGame_MonsterLove();
			break;
		}
	}

	private void Enter_InGame_QuestMode()
	{
		AnimationState animationState = _ClayCollectedPanel.animation[_ClayCollectedRepositionAnim];
		animationState.speed = 0f;
		animationState.normalizedTime = 0f;
		animationState.enabled = true;
		_ClayCollectedPanel.animation.Play(_ClayCollectedRepositionAnim);
		_ClayCounterGUI._PebbleSpriteAnim.Rewind();
		_ClayCollectedValueLabel.enabled = true;
		_ClayCollectedPanel.transform.parent.gameObject.SetActiveRecursively(true);
		_ClayCollectedPanel.SetActiveRecursively(true);
		_QuestPanel.StartInGame();
	}

	private void Enter_InGame_MonsterLove()
	{
		_monsterLoveGUI.InGameEnter();
		_ClayCollectedPanel.transform.parent.gameObject.SetActiveRecursively(false);
		_DistancePanel.SetActiveRecursively(true);
		ActivateDistanceBestElements(false);
		ActivateDistanceGameOverElements(false);
		_DistancePanel.animation.Play(_DistancePanelMonsterLoveStart);
	}

	private void Update_InGame()
	{
		if (CurrentHill.Instance.ProgressIsBeyondHorizon(Pebble.Instance.Progress))
		{
			if (_PauseButton != null && _PauseButton.active)
			{
				_PauseButton.SetActiveRecursively(false);
			}
			_QuestPanel.HideCounter();
		}
		else if (Pebble.Instance.Progress > 1f)
		{
			if (_PauseButton != null && !_PauseButton.active && !InGameController.Instance.Paused)
			{
				_PauseButton.SetActiveRecursively(true);
			}
			if (CurrentGameMode.Type == GameModeType.Quest)
			{
				_QuestPanel.ShowCounter();
			}
			else
			{
				_QuestPanel.HideCounter();
			}
		}
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			_monsterLoveGUI.InGameUpdate();
		}
	}

	private void Exit_InGame()
	{
		_PowerupPanel.SetActiveRecursively(false);
		_QuestPanel.HideCounter();
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			_monsterLoveGUI.InGameExit();
		}
	}

	private void Enter_Flying()
	{
		_resultMode = ResultMode.Success;
		_DistancePanel.SetActiveRecursively(true);
		_DistancePanel.animation.Play(_DistancePanelTransInAnim);
		_DistancePanelResultsBackground.active = false;
		ActivateDistanceBestElements(false);
		ActivateDistanceGameOverElements(false);
		_NextPowerPlayPanel.SetActiveRecursively(false);
		if (_PauseButton != null)
		{
			_PauseButton.SetActiveRecursively(false);
		}
		_jvpPositionedMidFlight = false;
	}

	private void Update_Flying()
	{
		_DistanceTitleLabel.MarkAsChanged();
		if (_gotNewBestScore && !_shownNewBestScore && !_DistancePanel.animation.isPlaying)
		{
			AnimateBestDistanceOn();
		}
		if (!_jvpPositionedMidFlight && _stateTimer > 0.25f)
		{
			JVPController.Instance.PositionJustOffResultsScreen();
			_jvpPositionedMidFlight = true;
		}
	}

	private void Exit_Flying()
	{
	}

	private void Enter_StartGameOverSequence()
	{
		_ClayTotalPanel.active = false;
		_NextPowerPlayPanel.SetActiveRecursively(false);
		_DistancePanel.SetActiveRecursively(true);
		ActivateDistanceGameOverElements(true);
		_DistancePanel.animation.Play(_DistancePanelTransInAnim);
	}

	private void Update_StartGameOverSequence()
	{
		if (!_DistancePanel.animation.IsPlaying(_DistancePanelTransInAnim))
		{
			_ClayCollectedPanel.animation.Play(_ClayCollectedTransOutAnim);
			ChangeState(State.FinalGameOver);
		}
	}

	private void Exit_StartGameOverSequence()
	{
		JVPController.Instance.PositionReadyOnResultsScreen();
	}

	private void Enter_FinalGameOver()
	{
		_DistancePanel.SetActiveRecursively(true);
		ActivateDistanceGameOverElements(true);
		_DistancePanel.animation.Play(_DistancePanelHighlightAnim, AnimationPlayMode.Stop);
		_ButtonsPanel.SetActiveRecursively(true);
		_ButtonsPanel.animation.Play(ButtonsTransInAnim);
	}

	private void Update_FinalGameOver()
	{
	}

	private void Exit_FinalGameOver()
	{
		FinishDistanceHighlightAnim();
	}

	private void FinishDistanceHighlightAnim()
	{
		AnimationState animationState = _DistancePanel.animation[_DistancePanelHighlightAnim];
		animationState.normalizedTime = 1f;
		_DistancePanel.animation.Sample();
	}

	private void Enter_SwitchToResults()
	{
		_DistancePanelResultsBackground.active = true;
		JVPController.Instance.PositionForClayCounting();
		_zoomInPlayed = false;
	}

	private void Update_SwitchToResults()
	{
		if (!_zoomInPlayed && (_skipRequested || _stateTimer > 0.25f))
		{
			InGameAudio.PostFabricEvent("ZoomIn");
			_zoomInPlayed = true;
		}
		if (_skipRequested || _stateTimer > 0.5f)
		{
			ChangeState(State.HighlightDistancePanel);
		}
	}

	private void Exit_SwitchToResults()
	{
	}

	private void Enter_HighlightDistancePanel()
	{
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			_DistancePanel.animation.Play(_DistancePanelMonsterLoveFinish);
		}
		else
		{
			_DistancePanel.animation.Play(_DistancePanelHighlightAnim);
		}
	}

	private void Update_HighlightDistancePanel()
	{
		if (_skipRequested || !_DistancePanel.animation.IsPlaying(_DistancePanelHighlightAnim))
		{
			if (CurrentGameMode.Type == GameModeType.Quest)
			{
				ChangeState(State.HighlightPowerPlayPanel);
			}
			else
			{
				ChangeState(State.MoveClayCollectedPanels);
			}
		}
	}

	private void Exit_HighlightDistancePanel()
	{
		FinishDistanceHighlightAnim();
	}

	private void Enter_HighlightPowerPlayPanel()
	{
		_NextPowerPlayPanel.SetActiveRecursively(true);
		_NextPowerPlayPanel.animation.Play(_NextPowerPlayPanelTransInAnim);
		_FloatDownDistanceAnim.Play();
		_PrizeGUI.StartCounting();
	}

	private void Update_HighlightPowerPlayPanel()
	{
		if (_skipRequested)
		{
			AnimationState animationState = _NextPowerPlayPanel.animation[_NextPowerPlayPanelTransInAnim];
			animationState.normalizedTime = 1f;
			ChangeState(State.CountDistanceForPowerPlays);
		}
		else if (!_NextPowerPlayPanel.animation.isPlaying)
		{
			ChangeState(State.CountDistanceForPowerPlays);
		}
	}

	private void Exit_HighlightPowerPlayPanel()
	{
	}

	private void Enter_CountDistanceForPowerPlays()
	{
	}

	private void Update_CountDistanceForPowerPlays()
	{
		if (_PrizeGUI.UpdateCounting(_skipRequested))
		{
			_NextPowerPlayPanel.animation.Play(_NextPowerPlayPanelHighlightAnim);
			ChangeState(State.MoveClayCollectedPanels);
		}
	}

	private void Exit_CountDistanceForPowerPlays()
	{
	}

	private void Enter_MoveClayCollectedPanels()
	{
		if (CurrentGameMode.Type != GameModeType.MonsterLove)
		{
			_ClayCounterGUI.GetReady();
			AnimationState animationState = _ClayCollectedPanel.animation[_ClayCollectedRepositionAnim];
			animationState.speed = 1f;
			_ClayCollectedPanel.animation.Play(_ClayCollectedRepositionAnim);
		}
		_ClayTotalPanel.SetActiveRecursively(true);
		_ClayTotalAnimation.Play(_ClayTotalTransInAnim);
	}

	private void Update_MoveClayCollectedPanels()
	{
		bool flag = false;
		flag = ((CurrentGameMode.Type == GameModeType.MonsterLove) ? _monsterLoveGUI.ClayConvertFinished : (!_ClayCollectedPanel.animation.IsPlaying(_ClayCollectedRepositionAnim) && !_ClayTotalAnimation.IsPlaying(_ClayTotalTransInAnim)));
		if (flag || _skipRequested)
		{
			ChangeState(State.CountClay);
		}
	}

	private void Exit_MoveClayCollectedPanels()
	{
	}

	private void Enter_ConvertHeartsToClay()
	{
		_ClayCounterGUI.GetReady();
		_monsterLoveGUI.ClayConvertStart();
		_ClayCollectedPanel.transform.parent.gameObject.SetActiveRecursively(true);
		_ClayCollectedPanel.animation.Play(_ClayCollectedAnimReadyToConvert);
	}

	private void Update_ConvertHeartsToClay()
	{
		_monsterLoveGUI.ClayConvertUpdate();
		if (_monsterLoveGUI.ClayConvertFinished || _skipRequested)
		{
			AnimationState animationState = _ClayCollectedPanel.animation[_ClayCollectedAnimReadyToConvert];
			animationState.normalizedTime = 1f;
			ChangeState(State.SwitchToResults);
		}
	}

	private void Exit_ConvertHeartsToClay()
	{
		_monsterLoveGUI.ClayConvertExit();
	}

	private void Enter_CountClay()
	{
		_ClayCounterGUI.Start();
	}

	private void Update_CountClay()
	{
		if (_skipRequested)
		{
			_ClayCounterGUI.Finish();
		}
		else
		{
			_ClayCounterGUI.Update();
		}
		if (_ClayCounterGUI.FinishedCounting)
		{
			if (FlyingGUIController.FinishedAnimatingResults != null)
			{
				FlyingGUIController.FinishedAnimatingResults();
			}
			ChangeState(State.FinalResults);
		}
	}

	private void Exit_CountClay()
	{
		_ClayTotalAnimation.Play(_ClayTotalFinished, AnimationPlayMode.Queue);
		_ClayCollectedPanel.animation.Play(_ClayCollectedTransOutAnim);
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			_monsterLoveGUI.TurnOffCupid();
		}
		InGameAudio.PostFabricEvent("ZoomOut");
		JVPController.Instance.PositionReadyOnResultsScreen();
		JVPController.Instance.ForcePlayAnim("VeryHappy");
	}

	private void Enter_FinalResults()
	{
		_DistancePanel.SetActiveRecursively(true);
		ActivateDistanceGameOverElements(false);
		ActivateDistanceBestElements(_gotNewBestScore);
		if (CurrentGameMode.Type == GameModeType.Quest)
		{
			_NextPowerPlayPanel.SetActiveRecursively(true);
			_PrizeGUI.SetProgressBarValues();
		}
		_ClayTotalPanel.SetActiveRecursively(true);
		_ClayCounterGUI.SetCountedAmounts();
		_ButtonsPanel.SetActiveRecursively(true);
		_ButtonsPanel.animation.Play(ButtonsTransInAnim);
		DoRatingsPrompt();
		SaveData.Instance.SaveIfNeeded();
	}

	private void Update_FinalResults()
	{
		_ClayCounterGUI.Update();
	}

	private void Exit_FinalResults()
	{
		_ClayTotalAnimation.Play(_ClayTotalTransOutAnim);
	}

	private void Enter_ReturnToResults()
	{
		_DistancePanel.SetActiveRecursively(true);
		ActivateDistanceGameOverElements(false);
		ActivateDistanceBestElements(_gotNewBestScore);
		_DistancePanel.animation.Play(_DistancePanelTransInAnim);
		if (CurrentGameMode.Type == GameModeType.Quest)
		{
			_NextPowerPlayPanel.SetActiveRecursively(true);
			_NextPowerPlayPanel.animation.Play(_NextPowerPlayPanelTransInAnim);
			_PrizeGUI.SetProgressBarValues();
		}
		_ClayTotalPanel.SetActiveRecursively(true);
		_ClayTotalAnimation.Play(_ClayTotalTransInAnim);
		_ClayTotalAnimation.Play(_ClayTotalFinished, AnimationPlayMode.Queue);
		_ClayCounterGUI.SetCountedAmounts();
		_ButtonsPanel.SetActiveRecursively(true);
		_ButtonsPanel.animation.Play(ButtonsTransInAnim);
	}

	private void Update_ReturnToResults()
	{
	}

	private void Exit_ReturnToResults()
	{
		_ClayTotalAnimation.Play(_ClayTotalTransOutAnim);
	}

	private void Enter_TransitionOut()
	{
		if (_exitPath == ExitPath.NotSet)
		{
			Debug.LogError("Trying to exit results screen with nowhere to go!");
		}
		_DistancePanel.animation.Play(_DistancePanelTransOutAnim);
		_ButtonsPanel.animation.Play(ButtonsTransOutAnim);
		if (CurrentGameMode.Type == GameModeType.Quest && _resultMode == ResultMode.Success)
		{
			_NextPowerPlayPanel.animation.Play(_NextPowerPlayPanelTransOutAnim);
		}
		JVPController.Instance.PositionJustOffResultsScreen();
	}

	private void Update_TransitionOut()
	{
		bool flag = false;
		flag |= _DistancePanel.animation.IsPlaying(_DistancePanelTransOutAnim);
		flag |= _ButtonsPanel.animation.IsPlaying(ButtonsTransOutAnim);
		if (CurrentGameMode.Type == GameModeType.Quest && _resultMode == ResultMode.Success)
		{
			flag |= _NextPowerPlayPanel.animation.IsPlaying(_NextPowerPlayPanelTransOutAnim);
		}
		if (!flag)
		{
			ChangeState(State.Off);
		}
	}

	private void Exit_TransitionOut()
	{
		switch (_exitPath)
		{
		case ExitPath.Restart:
			InGameController.Instance.PressedPlay();
			break;
		case ExitPath.Exit:
			JVPController.Instance.HideFromResultsScreen();
			UIEvents.SendEvent(UIEventType.ReturnToFrontend, null);
			break;
		case ExitPath.Shop:
			InGameController.Instance.GoToShop();
			break;
		}
	}

	private void OnInGameStateChanged(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.Flying:
			ChangeState(State.Flying);
			break;
		case InGameController.State.Landed:
			ChangeState(State.QuestComplete);
			break;
		case InGameController.State.ShowingResultsGameOver:
			StartGameOverStates();
			break;
		}
	}

	private void StartGameOverStates()
	{
		_skipRequested = false;
		JVPController.Instance.PositionJustOffResultsScreen();
		if (_PauseButton != null)
		{
			_PauseButton.SetActiveRecursively(false);
		}
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			_resultMode = ResultMode.Success;
			ChangeState(State.ConvertHeartsToClay);
		}
		else
		{
			_resultMode = ResultMode.Failure;
			ChangeState(State.StartGameOverSequence);
		}
	}

	private void OnNewBestScore()
	{
		_gotNewBestScore = true;
		_shownNewBestScore = false;
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			AnimateBestDistanceOn();
		}
	}

	private void AnimateBestDistanceOn()
	{
		AnimationState animationState = _DistancePanel.animation[_DistancePanelNewBestAnim];
		animationState.speed = 1f;
		animationState.normalizedTime = 0f;
		animationState.enabled = true;
		_DistancePanel.animation.Play(_DistancePanelNewBestAnim);
		ActivateDistanceBestElements(true);
		_shownNewBestScore = true;
	}

	private void OnClayBouncedOff(float amount, float startedWithAmount)
	{
		_ClayCollectedPanel.animation.Play(_ClayCollectedLoseClayAnim);
		_ClayCollectedSpriteAnim.Play();
	}

	private void OnGamePaused()
	{
		if (_PauseButton != null)
		{
			_PauseButton.SetActiveRecursively(false);
		}
	}

	private void OnGameUnpaused()
	{
	}

	private void OnButtonPressed(GameButtonType type)
	{
		switch (type)
		{
		case GameButtonType.Quit:
			_exitPath = ExitPath.Exit;
			ChangeState(State.TransitionOut);
			break;
		case GameButtonType.Restart:
			_exitPath = ExitPath.Restart;
			ChangeState(State.TransitionOut);
			break;
		case GameButtonType.GoToShop:
			_exitPath = ExitPath.Shop;
			ChangeState(State.TransitionOut);
			break;
		case GameButtonType.Back:
			if (!InGameController.Instance.Paused)
			{
				if (InGameController.Instance.CurrentState == InGameController.State.RollingTop)
				{
					InGameNGUI.Instance.GoToPauseMenu();
				}
				else if (_ButtonsPanel.active)
				{
					_exitPath = ExitPath.Exit;
					ChangeState(State.TransitionOut);
				}
			}
			else if (CurrentState == State.QuestIntro && NGUIPanelManager.Instance.TopPanel == InGameNGUI.Instance._MainPanel)
			{
				UIEvents.SendEvent(UIEventType.ReturnToFrontend, null);
			}
			break;
		case GameButtonType.Continue:
			_QuestPanel.ContinuePressed();
			break;
		case GameButtonType.SkipQuest:
			_QuestPanel.SkipPressed();
			break;
		case GameButtonType.DoRewardedAd:
			RewardedAds.Instance.ShowAd();
			break;
		}
	}

	public void SpawnSquashedAmountIcon(int amount, HSVColour colour, SquashedAmount.AddAmountHandler addAmountHandler)
	{
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(_SquashedAmountHUDPrefab);
		gameObject.transform.position = _SquashedAmountHUDParent.position;
		gameObject.transform.parent = _SquashedAmountHUDParent;
		SquashedAmount component = gameObject.GetComponent<SquashedAmount>();
		component.Begin(amount, colour, addAmountHandler);
	}

	public void PlayBossHitAnim()
	{
		_StarBurstObject.transform.parent.gameObject.SetActiveRecursively(true);
		_StarBurstObject.animation.Play(_StarBurstAnim);
		_StarBurstParticles.Emit();
	}

	private void ActivateDistanceBestElements(bool activate)
	{
		_DistanceBestBadge.active = activate;
		_DistanceBestLabel.Activate(activate);
		_DistanceHighlightBorder.active = activate;
	}

	private void ActivateDistanceGameOverElements(bool activate)
	{
		_DistanceGameOverLabel.Activate(activate);
		ActivateDistanceTextElements(!activate);
		if (activate)
		{
			ActivateDistanceBestElements(false);
		}
	}

	private void ActivateDistanceTextElements(bool activate)
	{
		_DistanceValueLabel.gameObject.SetActiveRecursively(activate);
		_DistanceTitleLabel.Activate(activate);
	}

	private void DoRatingsPrompt()
	{
		if (_gotNewBestScore && CurrentHill.Instance.UpgradeLevel > _lastHillLevelUsedForRatePrompt)
		{
			SystemPrompt.AskForReview();
			_lastHillLevelUsedForRatePrompt += 2;
		}
	}
}
