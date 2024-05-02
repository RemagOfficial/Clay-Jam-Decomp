using UnityEngine;

public class LeapConnectionMonitor : MonoBehaviour
{
	private enum State
	{
		none = 0,
		animatingErrorIn = 1,
		showingError = 2,
		animatingErrorOut = 3
	}

	private const string _InAnimName = "In";

	private const string _OutAnimName = "Out";

	public Animation _Animation;

	public GameObject _Panel;

	private State _currentState;

	private void Start()
	{
		_currentState = State.none;
		if (!BuildDetails.Instance._UseLeapIfAvailable)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		switch (_currentState)
		{
		case State.none:
			if (ClayJamInput.LeapConnectionError)
			{
				_Panel.SetActiveRecursively(true);
				_Animation.Play("In");
				_currentState = State.animatingErrorIn;
				PauseGame();
			}
			break;
		case State.animatingErrorIn:
			if (!_Animation.IsPlaying("In"))
			{
				_currentState = State.showingError;
			}
			break;
		case State.showingError:
			if (Input.GetKey(KeyCode.Escape))
			{
				Application.Quit();
			}
			else if (!ClayJamInput.LeapConnectionError)
			{
				_Animation.Play("Out");
				_currentState = State.animatingErrorOut;
			}
			break;
		case State.animatingErrorOut:
			if (!_Animation.IsPlaying("Out"))
			{
				_Panel.gameObject.SetActiveRecursively(false);
				_currentState = State.none;
			}
			break;
		}
	}

	private void PauseGame()
	{
		if (InGameController.Instance.CurrentState == InGameController.State.RollingTop && !InGameController.Instance.Paused)
		{
			InGameNGUI.Instance.GoToPauseMenu();
		}
	}
}
