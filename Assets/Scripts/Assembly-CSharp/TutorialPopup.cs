using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
	private enum State
	{
		on = 0,
		waitingToClose = 1,
		closing = 2,
		off = 3
	}

	private const float MinShowTime = 1f;

	private GameObject _panelObject;

	private Animation _animation;

	private float _earliestHideTime;

	private float _hideDelay;

	private State state;

	public bool IsShowing
	{
		get
		{
			return state == State.on;
		}
	}

	private void Awake()
	{
		state = State.off;
	}

	private void Update()
	{
		switch (state)
		{
		case State.on:
			if (!_animation.isPlaying)
			{
				_animation.Play("Idle");
			}
			break;
		case State.waitingToClose:
			if (Time.time > _earliestHideTime && Time.time > _hideDelay)
			{
				_animation.Play("Out");
				state = State.closing;
			}
			break;
		case State.closing:
			if (!_animation.isPlaying)
			{
				_panelObject.SetActiveRecursively(false);
				state = State.off;
			}
			break;
		case State.off:
			break;
		}
	}

	public void SetPanelObject(GameObject panelObject)
	{
		_panelObject = panelObject;
		_panelObject.SetActiveRecursively(true);
		_animation = _panelObject.GetComponentInChildren<Animation>();
		_panelObject.SetActiveRecursively(false);
	}

	public void Show()
	{
		_panelObject.SetActiveRecursively(true);
		_animation.Play("In");
		state = State.on;
		_earliestHideTime = Time.time + 1f;
	}

	public void Hide(float hidedelay)
	{
		_hideDelay = Time.time + hidedelay;
		state = State.waitingToClose;
	}
}
