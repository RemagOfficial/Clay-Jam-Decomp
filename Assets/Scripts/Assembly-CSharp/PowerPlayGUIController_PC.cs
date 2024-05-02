using UnityEngine;

public class PowerPlayGUIController_PC : MonoBehaviour
{
	private enum State
	{
		NotAvailable = 0,
		WaitingForLaunch = 1,
		Launched = 2,
		NotLaunched = 3
	}

	private const string QuestionOutAnimation = "LaunchQuestionOut";

	public PowerPlayLauncher _PowerPlayLauncher;

	public GameObject _PowerPlayPanel;

	public UISpriteHSVColourer _PowerPlayButtonToColour;

	public Animation _QuestionPanelAnimations;

	private State _currentState;

	private void Awake()
	{
		UIEvents.ButtonPressed += OnButtonPressed;
		InGameController.StateChanged += OnStateChanged;
		InGameController.AskPowerPlayQuestionEvent += OnAskPowerplayQuestionEvent;
	}

	private void OnDestroy()
	{
		UIEvents.ButtonPressed -= OnButtonPressed;
		InGameController.StateChanged -= OnStateChanged;
		InGameController.AskPowerPlayQuestionEvent -= OnAskPowerplayQuestionEvent;
	}

	private void OnStateChanged(InGameController.State newState)
	{
		if (newState == InGameController.State.ResettingForRun)
		{
			OnResetForRun();
		}
	}

	private void OnResetForRun()
	{
		_currentState = State.NotAvailable;
	}

	private void OnAskPowerplayQuestionEvent()
	{
		_PowerPlayLauncher.SetColoursToMatchHil();
		_PowerPlayButtonToColour.UseColour(CurrentHill.Instance.Definition._PowerPlayColour);
		if (BuildDetails.Instance._DemoMode)
		{
			Launch();
			InGameController.Instance.OnPowerPlayQuestionAnswered();
		}
		else
		{
			_PowerPlayPanel.SetActiveRecursively(true);
			_currentState = State.WaitingForLaunch;
		}
	}

	private void Update()
	{
		switch (_currentState)
		{
		case State.NotAvailable:
			break;
		case State.WaitingForLaunch:
			break;
		case State.Launched:
			UpdateLaunched();
			break;
		case State.NotLaunched:
			UpdateNotLaunched();
			break;
		}
	}

	private void UpdateLaunched()
	{
		if (!_PowerPlayLauncher.UpdateAfterLaunch())
		{
			MarkAsNoLongerAvailable();
		}
	}

	private void UpdateNotLaunched()
	{
		if (!_QuestionPanelAnimations.IsPlaying("LaunchQuestionOut"))
		{
			MarkAsNoLongerAvailable();
		}
	}

	private void OnButtonPressed(GameButtonType type)
	{
		if (_currentState == State.WaitingForLaunch)
		{
			bool flag = false;
			switch (type)
			{
			case GameButtonType.Yes:
				flag = true;
				Launch();
				break;
			case GameButtonType.No:
				flag = true;
				DontLaunch();
				break;
			}
			if (flag)
			{
				_QuestionPanelAnimations.Play("LaunchQuestionOut");
				InGameController.Instance.OnPowerPlayQuestionAnswered();
			}
		}
	}

	private void Launch()
	{
		_PowerPlayLauncher.Launch();
		_currentState = State.Launched;
	}

	private void DontLaunch()
	{
		_currentState = State.NotLaunched;
	}

	private void MarkAsNoLongerAvailable()
	{
		_PowerPlayPanel.SetActiveRecursively(false);
		_currentState = State.NotAvailable;
	}
}
