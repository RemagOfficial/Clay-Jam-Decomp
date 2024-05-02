using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fabric;
using UnityEngine;

public class FrontendWorldController : ManagedComponent
{
	public enum State
	{
		Loading = 0,
		HillSelection = 1,
		JVP = 2,
		BuyingHillItem = 3,
		Story = 4,
		TransitionHillsToMenu = 5,
		Menu = 6,
		TransitionMenuToHills = 7
	}

	public enum HillButtons
	{
		All = 0,
		PlayOnly = 1
	}

	public List<FrontendWorldHillSelector> _Hills;

	public UIToggleButton _ButtonMusic;

	public UIToggleButton _ButtonSFX;

	public UIToggleButton _ButtonTutorial;

	public float _AudioFullScreenVolume;

	public float _AudioBackgroundVolume;

	private bool _readyToShowHillUpgradingAnimations;

	private State _currentState;

	private string _currentTransitionAnim;

	public GameObject _ClayCounter;

	public GameObject _IAPButton;

	public GameObject _FreeClayButton;

	private bool _temporarilySwitchedFromFullScreen;

	private string _URLToCall;

	public static FrontendWorldController Instance { get; private set; }

	private State CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			_currentState = value;
			if (FrontendWorldController.StateChangeEvent != null)
			{
				FrontendWorldController.StateChangeEvent(_currentState);
			}
		}
	}

	private FrontendWorldHillSelector SelectedHill
	{
		get
		{
			return _Hills[SelectedHillIndex];
		}
	}

	private int SelectedHillIndex { get; set; }

	[method: MethodImpl(32)]
	public static event Action FirstPlayButtonShown;

	[method: MethodImpl(32)]
	public static event Action PlayButtonHidden;

	[method: MethodImpl(32)]
	public static event Action StartGamePressed;

	[method: MethodImpl(32)]
	public static event Action<State> StateChangeEvent;

	[method: MethodImpl(32)]
	public static event Action NewHillSelectedEvent;

	[method: MethodImpl(32)]
	public static event Action HillAboutToBeDeselected;

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("More tahn one FronbtendWorldContolelr", base.gameObject);
		}
		Instance = this;
		JVPController.StateChangeEvent += OnJVPStateChange;
		FrontendCameraDirector.CameraReadyEvent += OnCameraReady;
		StoryController.StoryStartedEvent += OnStoryStarted;
		StoryController.StoryFinishedEvent += OnStoryFinished;
		StoryController.StoryCompleteStartedEvent += OnStoryStarted;
		StoryController.StoryCompleteFinishedEvent += OnStoryFinished;
		UIEvents.ButtonPressed += OnButtonPressed;
		NGUIPanelManager.PanelActivated += OnPanelActivated;
		NGUIPanelManager.PanelClosed += OnPanelClose;
		_ButtonMusic._OnToggle = ToggleMusicButton;
		_ButtonSFX._OnToggle = ToggleSFXButton;
		_ButtonTutorial._OnToggle = ToggleTutorialButton;
		CurrentState = State.Loading;
		base.OnAwake();
	}

	private void OnDestroy()
	{
		JVPController.StateChangeEvent -= OnJVPStateChange;
		FrontendCameraDirector.CameraReadyEvent -= OnCameraReady;
		StoryController.StoryStartedEvent -= OnStoryStarted;
		StoryController.StoryFinishedEvent -= OnStoryFinished;
		StoryController.StoryCompleteStartedEvent -= OnStoryStarted;
		StoryController.StoryCompleteFinishedEvent -= OnStoryFinished;
		UIEvents.ButtonPressed -= OnButtonPressed;
		NGUIPanelManager.PanelActivated -= OnPanelActivated;
		StopAudio();
		if (JVPController.Instance != null)
		{
			JVPController.Instance.HideFromFrontend();
		}
	}

	protected override bool DoInitialise()
	{
		SelectedHillIndex = 0;
		foreach (FrontendWorldHillSelector hill in _Hills)
		{
			hill.Initialise(this);
		}
		_ButtonMusic.SetOn(SaveData.Instance.Progress._optionMusicOn.Set);
		_ButtonSFX.SetOn(SaveData.Instance.Progress._optionSFXOn.Set);
		_ButtonTutorial.SetOn(SaveData.Instance.Progress._optionTutorialsOn.Set);
		_readyToShowHillUpgradingAnimations = false;
		JVPController.Instance.ShowForFrontend();
		return true;
	}

	public override void ResetForRun()
	{
		base.ResetForRun();
		CurrentState = State.HillSelection;
		ResetToCurrentHill();
		if (!SaveData.Instance.Progress._FinishedOneLevel.Set)
		{
			SetActiveHillControls(false);
			SetActiveHillControls(true, HillButtons.PlayOnly);
		}
		else
		{
			SetActiveHillControls(true);
		}
	}

	protected override void OnRunStarted()
	{
		JVPController.Instance.PlayMainAnim();
		StartAudio();
		_readyToShowHillUpgradingAnimations = true;
		SelectedHill.Refresh();
		if (BuildDetails.Instance._DemoMode)
		{
			Play();
		}
	}

	private void Update()
	{
		switch (_currentState)
		{
		case State.HillSelection:
			CheckButtonPresses();
			break;
		case State.TransitionHillsToMenu:
			if (!base.animation.IsPlaying(_currentTransitionAnim))
			{
				UIEvents.SendEvent(UIEventType.ResetToPanel, FrontendNGUI.Instance._MenuPanel);
				_currentState = State.Menu;
			}
			break;
		case State.TransitionMenuToHills:
			if (!base.animation.IsPlaying(_currentTransitionAnim))
			{
				UIEvents.SendEvent(UIEventType.ResetToPanel, FrontendNGUI.Instance._StartPanel);
				_currentState = State.HillSelection;
			}
			break;
		}
	}

	private void ToggleMusicButton(bool newValue)
	{
		InGameAudio.MusicToggled(newValue);
		SaveData.Instance.Progress.SetMusicOn(newValue);
		SaveData.Instance.Save();
		if (newValue)
		{
			MusicController.Instance.StartFrontend();
		}
	}

	private void ToggleSFXButton(bool newValue)
	{
		InGameAudio.SFXToggled(newValue);
		SaveData.Instance.Progress.SetSFXOn(newValue);
		SaveData.Instance.Save();
	}

	private void ToggleTutorialButton(bool newValue)
	{
		SaveData.Instance.Progress.SetTutorialsOn(newValue);
		SaveData.Instance.MarkAsNeedToSave(true);
		SaveData.Instance.Save();
	}

	public void Play()
	{
		if (FrontendWorldController.StartGamePressed != null)
		{
			FrontendWorldController.StartGamePressed();
		}
	}

	public void ShowMenu()
	{
		_currentState = State.TransitionHillsToMenu;
		_currentTransitionAnim = string.Format("Hill{0}MenuIn", SelectedHillIndex + 1);
		base.animation.Play(_currentTransitionAnim);
		UIEvents.SendEvent(UIEventType.PopPanel, FrontendNGUI.Instance._StartPanel);
	}

	public void HideMenu()
	{
		_currentState = State.TransitionMenuToHills;
		_currentTransitionAnim = string.Format("Hill{0}MenuOut", SelectedHillIndex + 1);
		base.animation.Play(_currentTransitionAnim);
		UIEvents.SendEvent(UIEventType.PopPanel, FrontendNGUI.Instance._MenuPanel);
		SelectedHill.DisplayBoss(true);
	}

	public void SelectHillLeft()
	{
		int num = SelectedHillIndex - 1;
		if (num < 0)
		{
			num = _Hills.Count - 1;
		}
		InGameAudio.PostFabricEvent("HillSelect");
		SelectHill(num, false);
	}

	public void SelectHillRight()
	{
		int num = SelectedHillIndex + 1;
		if (num >= _Hills.Count)
		{
			num = 0;
		}
		InGameAudio.PostFabricEvent("HillSelect");
		SelectHill(num, false);
	}

	public bool IsTransitionAnimationPlaying()
	{
		return base.animation.IsPlaying(_currentTransitionAnim);
	}

	private void SelectHill(int newIndex, bool restart)
	{
		if (FrontendWorldController.HillAboutToBeDeselected != null)
		{
			FrontendWorldController.HillAboutToBeDeselected();
		}
		_currentTransitionAnim = string.Format("{0}to{1}", SelectedHillIndex + 1, newIndex + 1);
		base.animation.Play(_currentTransitionAnim);
		SelectedHillIndex = newIndex;
		CurrentHill.Instance.ID = SelectedHill._HillID;
		SelectedHill.Refresh();
		if (FrontendWorldController.NewHillSelectedEvent != null)
		{
			FrontendWorldController.NewHillSelectedEvent();
		}
	}

	private void CheckButtonPresses()
	{
		if (!NGUIPanelManager.Instance.CurrentPanelPreventsWorldInteraction)
		{
			string currentMouseOverButton = "none";
			Ray ray = Camera.main.ScreenPointToRay(ClayJamInput.CursorScreenPosition);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, 100f, 4096))
			{
				currentMouseOverButton = hitInfo.collider.name;
			}
			SelectedHill.UpdateButtons(currentMouseOverButton);
		}
	}

	private void OnButtonPressed(GameButtonType type)
	{
		switch (type)
		{
		case GameButtonType.GoToMenu:
			ShowMenu();
			break;
		case GameButtonType.GoToHills:
			HideMenu();
			break;
		case GameButtonType.PlayStory:
			StoryController.Instance.ShowStory();
			break;
		case GameButtonType.GoToShop:
			JVPController.Instance.ZoomIn();
			break;
		case GameButtonType.Back:
			if (NGUIPanelManager.Instance.TopPanel == null || CurrentState == State.BuyingHillItem)
			{
				break;
			}
			if (NGUIPanelManager.Instance.TopPanel == FrontendNGUI.Instance._IAPPanel)
			{
				if (StaticIAPItems.Instance._BackButton.active)
				{
					if (_currentState == State.JVP)
					{
						NGUIPanelManager.Instance.ResetToPanel(FrontendNGUI.Instance._JVPPanel);
						JVPController.Instance.SetActive();
					}
					else
					{
						NGUIPanelManager.Instance.ResetToPanel(FrontendNGUI.Instance._MapScreenPanel);
					}
				}
			}
			else if (NGUIPanelManager.Instance.TopPanel == FrontendNGUI.Instance._MenuPanel)
			{
				HideMenu();
			}
			else if (NGUIPanelManager.Instance.TopPanel == FrontendNGUI.Instance._MapScreenPanel)
			{
				Application.Quit();
			}
			else if (NGUIPanelManager.Instance.TopPanel == FrontendNGUI.Instance._StoryPanel)
			{
				StoryController.Instance.EndStory();
				FrontendCameraDirector.Instance.SetCameraIdle();
			}
			else if (NGUIPanelManager.Instance.TopPanel.name == "CreditsPanel")
			{
				NGUIPanelManager.Instance.ResetToPanel(FrontendNGUI.Instance._MenuPanel);
			}
			else
			{
				NGUIPanelManager.Instance.ResetToPanel(FrontendNGUI.Instance._MapScreenPanel);
			}
			break;
		case GameButtonType.TermsOfService:
			SwitchToBowserAndOpenURL("http://www.fatpebble.com/index.php/games/clay-jam/tos/");
			break;
		case GameButtonType.PrivacyPolicy:
			SwitchToBowserAndOpenURL("http://www.fatpebble.com/index.php/games/clay-jam/privacy-policy/");
			break;
		case GameButtonType.ResetTutorials:
			SaveData.Instance.Tutorials.Reset();
			SaveData.Instance.MarkAsNeedToSave(true);
			SaveData.Instance.Save();
			break;
		case GameButtonType.DoRewardedAd:
			RewardedAds.Instance.ShowAd();
			break;
		case GameButtonType.LaunchPowerPlay:
		case GameButtonType.DBG_DeleteSaveGame:
		case GameButtonType.DBG_GetClay:
		case GameButtonType.SkipQuest:
			break;
		}
	}

	public void OnJVPStateChange(JVPController.State state)
	{
		switch (state)
		{
		case JVPController.State.ZoomingIn:
			CurrentState = State.JVP;
			UIEvents.SendEvent(UIEventType.ResetToPanel, FrontendNGUI.Instance._JVPPanel);
			base.enabled = false;
			break;
		case JVPController.State.Interactive:
			if (CurrentState != State.JVP)
			{
				CurrentState = State.JVP;
			}
			SelectedHill.HideBoss();
			break;
		case JVPController.State.ZoomingOut:
			CurrentState = State.HillSelection;
			SetActiveHillControls(true);
			base.enabled = true;
			break;
		case JVPController.State.ZoomedOut:
			if (CurrentState != State.Story)
			{
				SelectedHill.Refresh(!_readyToShowHillUpgradingAnimations);
			}
			break;
		case JVPController.State.BuyingHillItem:
			CurrentState = State.BuyingHillItem;
			break;
		}
		SetAudioForJVPActivate(!base.enabled);
	}

	public void OnCameraReady(FrontendCameraVO cameraVO)
	{
		switch (cameraVO._CameraName)
		{
		case CameraNames.Frontend.Story:
			SetActiveHillControls(false);
			break;
		case CameraNames.Frontend.JVP:
			if (CurrentState == State.JVP)
			{
				SetActiveHillControls(false);
			}
			break;
		case CameraNames.Frontend.HillSideBuyItem:
			SelectedHill.Refresh(!_readyToShowHillUpgradingAnimations);
			break;
		}
	}

	private void SetAudioForJVPActivate(bool activated)
	{
		if (activated)
		{
			InGameAudio.PostFabricEvent("MapScreenActivate", EventAction.SetVolume, _AudioBackgroundVolume);
		}
		else
		{
			InGameAudio.PostFabricEvent("MapScreenActivate", EventAction.SetVolume, _AudioFullScreenVolume);
		}
	}

	private void StartAudio()
	{
		InGameAudio.PostFabricEvent("MapScreenActivate", EventAction.PlaySound);
	}

	private void StopAudio()
	{
		if (!(EventManager.Instance == null))
		{
			InGameAudio.PostFabricEvent("MapScreenActivate", EventAction.StopSound);
		}
	}

	private void PurchaseAllCreatures()
	{
		SaveData.Instance.Casts.PurchaseEveything();
	}

	private void ResetToCurrentHill()
	{
		SelectedHillIndex = _Hills.FindIndex((FrontendWorldHillSelector h) => h._HillID == CurrentHill.Instance.ID);
		SelectedHill.Refresh(true);
		string text = string.Format("Hill{0}Static", SelectedHillIndex + 1);
		base.animation.Play(text);
	}

	private void SetActiveHillControls(bool active, HillButtons whichButtons = HillButtons.All)
	{
		foreach (FrontendWorldHillSelector hill in _Hills)
		{
			if (whichButtons == HillButtons.All)
			{
				hill._LeftArrow._GameObject.SetActiveRecursively(active);
				hill._RightArrow._GameObject.SetActiveRecursively(active);
			}
			if (active)
			{
				hill.SetPlayButtonState();
			}
			else
			{
				hill._PlayButton._GameObject.SetActiveRecursively(false);
			}
		}
	}

	private void OnStoryStarted()
	{
		CurrentState = State.Story;
		SetActiveHillControls(false);
		foreach (FrontendWorldHillSelector hill in _Hills)
		{
			hill.SetupForStory();
		}
		UIEvents.SendEvent(UIEventType.ResetToPanel, FrontendNGUI.Instance._StoryPanel);
	}

	private void OnStoryFinished()
	{
		CurrentState = State.HillSelection;
		if (SaveData.Instance.Progress._FinishedOneLevel.Set)
		{
			SetActiveHillControls(true);
		}
		ResetToCurrentHill();
		UIEvents.SendEvent(UIEventType.ResetToPanel, FrontendNGUI.Instance._StartPanel);
	}

	private void OnPanelActivated(GameObject panel)
	{
		if (!SaveData.Instance.Progress._FinishedOneLevel.Set && panel.name == "MapScreenPanel")
		{
			SetActiveHillControls(false);
			SetActiveHillControls(true, HillButtons.PlayOnly);
			_ClayCounter.SetActive(false);
			if (BuildDetails.Instance._HasIAP)
			{
				_IAPButton.SetActive(false);
			}
			_FreeClayButton.SetActive(false);
			if (SaveData.Instance.Progress.StoryHasBeenPlayed() && FrontendWorldController.FirstPlayButtonShown != null)
			{
				FrontendWorldController.FirstPlayButtonShown();
			}
		}
	}

	private void OnPanelClose(GameObject panel)
	{
		if (panel.name == "MapScreenPanel" && FrontendWorldController.PlayButtonHidden != null)
		{
			FrontendWorldController.PlayButtonHidden();
		}
		if (FreeClayComponent.Instance != null)
		{
			FreeClayComponent.Instance.HidePanel();
		}
	}

	private void ___OnGUI()
	{
		if (GUILayout.Button("Upgrade Hill Upgrade Hill Upgrade Hill"))
		{
			CurrentHill.Instance.Upgrade(true);
		}
		if (GUILayout.Button("Purchase All Creatures Purchase All Creatures"))
		{
			PurchaseAllCreatures();
		}
		if (GUILayout.Button("Show Story"))
		{
			StoryController.Instance.ShowStory();
		}
	}

	public bool CanUnlockNextHill()
	{
		int num = SelectedHillIndex + 1;
		if (num >= _Hills.Count)
		{
			num = 0;
		}
		HillData hillByID = SaveData.Instance.Hills.GetHillByID(_Hills[num]._HillID);
		if (hillByID._State == LockState.Locked && hillByID.CanAffordUpgrade())
		{
			return true;
		}
		return false;
	}

	private void CheckFullScreenBeforeURL()
	{
	}

	private void CheckFullScreenOnRegainFocus()
	{
	}

	private void SwitchToBowserAndOpenURL(string url)
	{
		CheckFullScreenBeforeURL();
		float time = 0f;
		if (_temporarilySwitchedFromFullScreen)
		{
			time = 0.25f;
		}
		_URLToCall = url;
		Invoke("OpenURL", time);
	}

	private void OpenURL()
	{
		Application.OpenURL(_URLToCall);
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			CheckFullScreenOnRegainFocus();
		}
	}
}
