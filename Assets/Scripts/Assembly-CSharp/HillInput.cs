using UnityEngine;

public class HillInput : MonoBehaviour
{
	public UISpriteAnimationOnEvent _PauseButton;

	public static HillInput Instance { get; private set; }

	public bool HasCurrentGroundTouch { get; private set; }

	public Ray CurrentTouchRay { get; private set; }

	public Vector3 CurrentTouchGroundPos { get; private set; }

	public bool ScreenTouchDown
	{
		get
		{
			if ((bool)_PauseButton && _PauseButton.IsPressedDown)
			{
				return false;
			}
			return ClayJamInput.GougeAction;
		}
	}

	public Vector2 ScreenTouchPos
	{
		get
		{
			return ClayJamInput.CursorScreenPosition;
		}
	}

	private float RawTouchSpeed
	{
		get
		{
			return ClayJamInput.CursorSpeed;
		}
	}

	public float TouchSpeed_ScreenWidthsPerSecond
	{
		get
		{
			float rawTouchSpeed = RawTouchSpeed;
			return rawTouchSpeed / (float)Screen.width;
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of HillInput");
		}
		Instance = this;
		InGameController.StateChanged += OnNewState;
	}

	private void OnEnable()
	{
		HasCurrentGroundTouch = false;
	}

	private void OnDisable()
	{
		HasCurrentGroundTouch = false;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnNewState;
	}

	private void Update()
	{
		if (Camera.main == null)
		{
			return;
		}
		HasCurrentGroundTouch = false;
		CurrentTouchGroundPos = Vector3.zero;
		if (!InGameController.Instance.Paused && InGameController.Instance.CurrentState == InGameController.State.RollingTop && ScreenTouchDown)
		{
			Ray ray = Camera.main.ScreenPointToRay(ScreenTouchPos);
			float enter;
			if (new Plane(Vector3.up, 0f).Raycast(ray, out enter))
			{
				HasCurrentGroundTouch = true;
				CurrentTouchGroundPos = ray.GetPoint(enter);
				CurrentTouchRay = ray;
			}
		}
	}

	private void OnNewState(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.Flying:
			base.enabled = false;
			break;
		case InGameController.State.RollingTop:
			base.enabled = true;
			break;
		}
	}
}
