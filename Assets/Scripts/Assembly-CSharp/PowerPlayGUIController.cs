using UnityEngine;

public class PowerPlayGUIController : MonoBehaviour
{
	private enum State
	{
		NotAvailable = 0,
		WaitingForLaunch = 1,
		Launched = 2,
		Spawned = 3
	}

	private const string PowerPlayTransAnim = "PPTransIn";

	public PowerPlayLauncher _PowerPlayLauncher;

	public GameObject _PowerPlayPanel;

	public UISpriteHSVColourer _PowerPlayButtonToColour;

	public Animation _PanelAnim;

	private AnimationState _animState;

	private float _launchAnimTime;

	private State _currentState;

	private void Awake()
	{
		_animState = _PanelAnim["PPTransIn"];
		UIEvents.ButtonPressed += OnButtonPressed;
		InGameController.StateChanged += OnStateChanged;
	}

	private void OnDestroy()
	{
		UIEvents.ButtonPressed -= OnButtonPressed;
		InGameController.StateChanged -= OnStateChanged;
	}

	private void OnStateChanged(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.ResettingForRun:
			OnResetForRun();
			break;
		case InGameController.State.RollingTop:
			OnRunStarted();
			break;
		}
	}

	private void OnResetForRun()
	{
		if (!BuildDetails.Instance._DemoMode)
		{
			_launchAnimTime = 0f;
			_animState.normalizedTime = 0f;
			_PanelAnim.Stop();
		}
		_currentState = State.NotAvailable;
	}

	private void OnRunStarted()
	{
		int powerPlaysRemaining = CurrentHill.Instance.ProgressData._PowerPlaysRemaining;
		if (powerPlaysRemaining <= 0)
		{
			_currentState = State.NotAvailable;
			return;
		}
		_currentState = State.WaitingForLaunch;
		if (!BuildDetails.Instance._DemoMode)
		{
			if (!InGameController.Instance.Paused)
			{
				_animState.speed = 1f;
			}
			_launchAnimTime = 0f;
			_animState.normalizedTime = 0f;
			_PowerPlayPanel.SetActiveRecursively(true);
			_PowerPlayLauncher.SetColoursToMatchHil();
			_PowerPlayButtonToColour.UseColour(CurrentHill.Instance.Definition._PowerPlayColour);
		}
	}

	private void Update()
	{
		switch (_currentState)
		{
		case State.NotAvailable:
			break;
		case State.WaitingForLaunch:
			if (!BuildDetails.Instance._DemoMode)
			{
				UpdateWaitingForLaunch();
			}
			else
			{
				Launch();
			}
			break;
		case State.Launched:
			UpdateLaunched();
			break;
		case State.Spawned:
			break;
		}
	}

	private void UpdateWaitingForLaunch()
	{
		_animState.speed = 1f;
		if (InGameController.Instance.Paused)
		{
			if (_animState.normalizedTime >= 0.5f)
			{
				_animState.speed = 0f;
				_animState.normalizedTime = 0.5f;
			}
			return;
		}
		if (!_PanelAnim.isPlaying)
		{
			MarkAsNoLongerAvailable();
		}
		if (_animState.normalizedTime > 0f)
		{
			_launchAnimTime = _animState.normalizedTime;
		}
	}

	private void UpdateLaunched()
	{
		if (!_PowerPlayLauncher.UpdateAfterLaunch())
		{
			MarkAsNoLongerAvailable();
		}
	}

	private void OnButtonPressed(GameButtonType type)
	{
		if (_currentState == State.WaitingForLaunch)
		{
			if (type == GameButtonType.LaunchPowerPlay)
			{
				Launch();
			}
		}
	}

	private void Launch()
	{
		_PowerPlayLauncher.Launch();
		_currentState = State.Launched;
	}

	private void OnEnable()
	{
		if (_currentState == State.WaitingForLaunch && !_PowerPlayPanel.active)
		{
			_PowerPlayPanel.SetActiveRecursively(true);
			_animState.normalizedTime = _launchAnimTime;
		}
	}

	private void MarkAsNoLongerAvailable()
	{
		if (!BuildDetails.Instance._DemoMode)
		{
			_PowerPlayPanel.SetActiveRecursively(false);
		}
		_currentState = State.NotAvailable;
	}
}
