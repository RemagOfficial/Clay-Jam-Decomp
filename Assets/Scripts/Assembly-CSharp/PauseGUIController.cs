using Fabric;
using UnityEngine;

public class PauseGUIController : MonoBehaviour
{
	private enum State
	{
		Inactive = 0,
		Paused = 1,
		AskingToRestart = 2,
		AskingToQuit = 3
	}

	public GameObject _ContinueGroup;

	public GameObject _RestartGroup;

	public GameObject _QuitGroup;

	public GameObject _YesGroup;

	public GameObject _NoGroup;

	public UIButtonEvent _YesButton;

	public GameObject _PausedText;

	public GameObject _RestartQuestionText;

	public GameObject _QuitQuestionText;

	public UIToggleButton _MusicToggleButton;

	public UIToggleButton _SFXToggleButton;

	private State CurrentState { get; set; }

	private void Awake()
	{
		CurrentState = State.Inactive;
		UIEvents.ButtonPressed += OnButtonPressed;
	}

	private void OnDestroy()
	{
		UIEvents.ButtonPressed -= OnButtonPressed;
	}

	private void OnEnable()
	{
		CurrentState = State.Paused;
		_PausedText.SetActiveRecursively(true);
		_RestartQuestionText.SetActiveRecursively(false);
		_QuitQuestionText.SetActiveRecursively(false);
		_ContinueGroup.SetActiveRecursively(true);
		_YesGroup.SetActiveRecursively(false);
		_NoGroup.SetActiveRecursively(false);
		_YesButton._EventType = UIEventType.ButtonPressed;
		if (!BuildDetails.Instance._DemoMode)
		{
			_RestartGroup.SetActiveRecursively(true);
			_QuitGroup.SetActiveRecursively(true);
		}
		_MusicToggleButton.SetOn(SaveData.Instance.Progress._optionMusicOn.Set);
		_SFXToggleButton.SetOn(SaveData.Instance.Progress._optionSFXOn.Set);
		_MusicToggleButton._OnToggle = ToggleMusicButton;
		_SFXToggleButton._OnToggle = ToggleSFXButton;
		InGameAudio.PostFabricEvent("TransitionOn", EventAction.SetSwitch, "TransitionOn");
		InGameAudio.PostFabricEvent("TransitionOn", EventAction.PlaySound);
	}

	private void OnButtonPressed(GameButtonType type)
	{
		if (CurrentState == State.Inactive || type == GameButtonType.Pause)
		{
			return;
		}
		_PausedText.SetActiveRecursively(false);
		_RestartQuestionText.SetActiveRecursively(false);
		_QuitQuestionText.SetActiveRecursively(false);
		_ContinueGroup.SetActiveRecursively(false);
		_YesGroup.SetActiveRecursively(false);
		_NoGroup.SetActiveRecursively(false);
		if (!BuildDetails.Instance._DemoMode)
		{
			_RestartGroup.SetActiveRecursively(false);
			_QuitGroup.SetActiveRecursively(false);
		}
		switch (type)
		{
		case GameButtonType.Continue:
			UnPause();
			break;
		case GameButtonType.Restart:
			CurrentState = State.AskingToRestart;
			_RestartQuestionText.SetActiveRecursively(true);
			_YesGroup.SetActiveRecursively(true);
			_NoGroup.SetActiveRecursively(true);
			break;
		case GameButtonType.Quit:
			CurrentState = State.AskingToQuit;
			_QuitQuestionText.SetActiveRecursively(true);
			_YesGroup.SetActiveRecursively(true);
			_NoGroup.SetActiveRecursively(true);
			_YesButton._EventType = UIEventType.ReturnToFrontend;
			break;
		case GameButtonType.No:
			HandleNo();
			break;
		case GameButtonType.Yes:
			HandleYes();
			break;
		case GameButtonType.Back:
			if (CurrentState == State.Paused)
			{
				UnPause();
			}
			else if (CurrentState == State.AskingToRestart || CurrentState == State.AskingToQuit)
			{
				HandleNo();
			}
			break;
		case GameButtonType.GoToMenu:
		case GameButtonType.GoToHills:
		case GameButtonType.PlayStory:
		case GameButtonType.GoToShop:
			break;
		}
	}

	private void HandleYes()
	{
		if (CurrentState == State.AskingToRestart)
		{
			InGameController.Instance.Restart();
			Close();
		}
		SaveData.Instance.Save();
	}

	private void HandleNo()
	{
		CurrentState = State.Paused;
		_PausedText.SetActiveRecursively(true);
		_ContinueGroup.SetActiveRecursively(true);
		_RestartGroup.SetActiveRecursively(true);
		_QuitGroup.SetActiveRecursively(true);
		_YesButton._EventType = UIEventType.ButtonPressed;
	}

	private void ToggleMusicButton(bool newValue)
	{
		InGameAudio.MusicToggled(newValue);
		SaveData.Instance.Progress.SetMusicOn(newValue);
		SaveData.Instance.Save();
		if (newValue)
		{
			MusicController.Instance.StartInGame();
		}
	}

	private void ToggleSFXButton(bool newValue)
	{
		InGameAudio.SFXToggled(newValue);
		SaveData.Instance.Progress.SetSFXOn(newValue);
		SaveData.Instance.Save();
	}

	private void UnPause()
	{
		InGameController.Instance.Unpause();
		Close();
	}

	private void Close()
	{
		InGameAudio.PostFabricEvent("TransitionOn", EventAction.SetSwitch, "TransitionOff");
		InGameAudio.PostFabricEvent("TransitionOn", EventAction.PlaySound);
		NGUIPanelManager.Instance.ResetToPanel(InGameNGUI.Instance._MainPanel);
		CurrentState = State.Inactive;
	}
}
