using UnityEngine;

public class ClayJamInput : MonoBehaviour
{
	public enum CursorDisplayType
	{
		Pointer = 0,
		Gouger = 1
	}

	public bool _ForceMouse;

	private float _bottomMostCursorPosY = -1f;

	private float _lastCursorPosY = -1f;

	private float _swipeTime;

	private float _bottomMostPointableHeight = -1f;

	private static bool[] _cursorButtonDown = new bool[3];

	private static bool[] _cursorButton = new bool[3];

	private static bool[] _cursorButtonUp = new bool[3];

	private static Vector3 InvalidCursorPos = new Vector3(-1f, -1f, 0f);

	public static Vector3 CursorScreenPosition { get; private set; }

	public static Vector3 CursorScreenPositionLastFrame { get; private set; }

	public static bool GougeAction { get; private set; }

	public static bool FlickActionStarted { get; private set; }

	public static bool FlickActionEnded { get; private set; }

	public static float FlickActionSpeed_InchesPerSecond { get; private set; }

	public static float CursorSpeed { get; private set; }

	public static bool AnythingPressed { get; private set; }

	public static float CursorTimer { get; private set; }

	public static bool HatBrimActionStarted { get; set; }

	public static bool HatBrimAction { get; set; }

	public static bool HatBrimActionEnded { get; set; }

	public static CursorDisplayType CurrentCursorType { get; private set; }

	public static bool CursorActive { get; private set; }

	public static bool LeapConnectionError
	{
		get
		{
			return false;
		}
	}

	public static bool ShowCursorNow
	{
		get
		{
			return false;
		}
	}

	public static bool ShowCursorEver
	{
		get
		{
			return false;
		}
	}

	public static bool IsLeapActive
	{
		get
		{
			return false;
		}
	}

	public static bool CursorButtonDown(int button = 0)
	{
		if (button >= 0 && button < 3)
		{
			return _cursorButtonDown[button];
		}
		return false;
	}

	public static bool CursorButton(int button = 0)
	{
		if (button >= 0 && button < 3)
		{
			return _cursorButton[button];
		}
		return false;
	}

	public static bool CursorButtonUp(int button = 0)
	{
		if (button >= 0 && button < 3)
		{
			return _cursorButtonUp[button];
		}
		return false;
	}

	private void Start()
	{
	}

	private void Update()
	{
		CursorScreenPositionLastFrame = CursorScreenPosition;
		ClearInputFlags();
		UpdateLeap();
		UpdateUnity();
		if (CursorScreenPosition == InvalidCursorPos)
		{
			Vector3 zero = Vector3.zero;
		}
		else
		{
			CursorSpeed = ((CursorScreenPosition - CursorScreenPositionLastFrame) / Time.deltaTime).magnitude;
		}
	}

	private void ClearInputFlags()
	{
		for (int i = 0; i < 3; i++)
		{
			_cursorButtonDown[i] = false;
			_cursorButton[i] = false;
			_cursorButtonUp[i] = false;
		}
		GougeAction = false;
		FlickActionStarted = false;
		FlickActionEnded = false;
		FlickActionSpeed_InchesPerSecond = 0f;
		AnythingPressed = false;
		HatBrimActionStarted = false;
		HatBrimActionEnded = false;
		HatBrimAction = false;
		CursorScreenPosition = InvalidCursorPos;
	}

	private void UpdateLeap()
	{
	}

	private void UpdateUnity()
	{
		if (!BuildDetails.Instance._OnlyUseLeap)
		{
			UpdateUnityCursorScreenPosition();
			UpdateUnityCursorButton();
			UpdateUnityAnythingPressed();
			UpdateUnityGougeAction();
			UpdateUnityFlickAction();
			UpdateUnityHatBrimAction();
		}
	}

	private void UpdateUnityHatBrimAction()
	{
		HatBrimActionStarted = CursorButtonDown();
		HatBrimAction = CursorButton();
		HatBrimActionEnded = CursorButtonUp();
	}

	private void UpdateUnityFlickAction()
	{
		FlickActionStarted |= Input.GetMouseButtonDown(0);
		FlickActionEnded |= Input.GetMouseButtonUp(0);
		if (FlickActionEnded && FlickActionSpeed_InchesPerSecond == 0f)
		{
			FlickActionSpeed_InchesPerSecond = -1f;
		}
	}

	private void UpdateUnityGougeAction()
	{
		if (!GougeAction)
		{
			if (Input.multiTouchEnabled && Input.touches.Length > 0)
			{
				GougeAction = Input.touches[0].phase != TouchPhase.Canceled && Input.touches[0].phase != TouchPhase.Ended;
			}
			else
			{
				GougeAction = Input.GetMouseButton(0);
			}
		}
	}

	private void UpdateUnityAnythingPressed()
	{
		AnythingPressed |= Input.GetMouseButtonUp(0);
	}

	private void UpdateUnityCursorButton()
	{
		for (int i = 0; i < 3; i++)
		{
			_cursorButtonDown[i] |= Input.GetMouseButtonDown(i);
			_cursorButton[i] |= Input.GetMouseButton(i);
			_cursorButtonUp[i] |= Input.GetMouseButtonUp(i);
		}
	}

	private void UpdateUnityCursorScreenPosition()
	{
		CursorScreenPosition = Input.mousePosition;
	}
}
