using Fabric;
using UnityEngine;

public class GougeInput : MonoBehaviour
{
	private const float CameraPebbleLagAllowed = 0.085f;

	public float _TimeToSetGougeSpeed;

	public float _EdgeTolerance = 0.5f;

	public float _ScreenWidthsPerSecondForMaxGougeSpeedBoost;

	public float _ScreenWidthsPerSecondForAnyGougeSpeedBoost;

	private float _currentMaxTouchSpeed;

	private float _currentGougeStartTime;

	private bool _currentGougeCapped;

	private static float _cameraPausedX;

	private static float _cameraBlendedX;

	private static bool _gouging;

	public static Vector3 CameraTarget
	{
		get
		{
			return new Vector3(_cameraBlendedX, Pebble.Position.y, Pebble.Position.z);
		}
	}

	public static Gouge CurrentGouge { get; private set; }

	public static void ResetCameraForRun()
	{
		_cameraBlendedX = 0f;
		_cameraPausedX = 0f;
	}

	private static void UpdateCameraPosition()
	{
		if (!_gouging)
		{
			_cameraPausedX = Pebble.Position.x * 0.2f + _cameraPausedX * 0.8f;
			_cameraBlendedX = _cameraPausedX;
			return;
		}
		float num = 0.085f * CameraDirector.WorldScreenWidth;
		if (_cameraPausedX < Pebble.Position.x - num)
		{
			_cameraPausedX = Pebble.Position.x - num;
		}
		else if (_cameraPausedX > Pebble.Position.x + num)
		{
			_cameraPausedX = Pebble.Position.x + num;
		}
		_cameraBlendedX = _cameraPausedX * 0.2f + _cameraBlendedX * 0.8f;
	}

	private void Awake()
	{
		InGameController.StateChanged += OnNewState;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnNewState;
	}

	private void FixedUpdate()
	{
		UpdateCameraPosition();
	}

	private void Update()
	{
		if (InGameController.Instance.PausedGouges)
		{
			StopGouging();
		}
		else if (!(Camera.main == null))
		{
			if (HillInput.Instance.HasCurrentGroundTouch && InGameController.Instance.CurrentState == InGameController.State.RollingTop)
			{
				GougeAt(HillInput.Instance.CurrentTouchGroundPos);
			}
			else
			{
				StopGouging();
			}
		}
	}

	private bool ClampPointToEdge(ref Vector3 ptToModify)
	{
		float widthLimit = Pebble.Instance.WidthLimit;
		if (ptToModify.x < 0f - widthLimit)
		{
			if (ptToModify.x > 0f - widthLimit - _EdgeTolerance)
			{
				ptToModify.x = 0f - widthLimit;
				return true;
			}
			return false;
		}
		if (ptToModify.x > widthLimit)
		{
			if (ptToModify.x < widthLimit + _EdgeTolerance)
			{
				ptToModify.x = widthLimit;
				return true;
			}
			return false;
		}
		return true;
	}

	private void GougeAt(Vector3 point)
	{
		if (CurrentGouge != null)
		{
			if (!ClampPointToEdge(ref point))
			{
				StopGouging();
			}
			else
			{
				ContinueGouge(point);
			}
		}
		else if (ClampPointToEdge(ref point))
		{
			StartGouging(point);
		}
	}

	private void ContinueGouge(Vector3 point)
	{
		if (!CurrentHill.Instance.ProgressIsBeyondGougeCap(point.z))
		{
			CurrentGouge.AddPoint(point);
		}
		else if (!_currentGougeCapped)
		{
			Vector3 point2 = point;
			point2.z = CurrentHill.Instance.GougeCapProgress;
			CurrentGouge.AddPoint(point2);
			_currentGougeCapped = true;
		}
		UpdateGougeSpeed();
		if (CurrentGouge.Finished)
		{
			CurrentGouge = GougeManager.Instance.StartNewGouge(point, false);
			CurrentGouge.Spline.SetInputStartTime(_currentGougeStartTime);
			CurrentGouge.Spline.SetMaxTouchSpeed(_currentMaxTouchSpeed);
			InGameAudio.PostFabricEvent("Gouge", EventAction.StopSound);
		}
	}

	private void StartGouging(Vector3 point)
	{
		if (CurrentHill.Instance.ProgressIsBeyondGougeCap(point.z))
		{
			return;
		}
		CurrentGouge = GougeManager.Instance.StartNewGouge(point, true);
		if ((bool)CurrentGouge)
		{
			Pebble.Instance.NewGougeStarted(CurrentGouge);
			_cameraPausedX = Pebble.Position.x;
			_gouging = true;
			_currentGougeStartTime = Time.time;
			CurrentGouge.Spline.SetInputStartTime(_currentGougeStartTime);
			_currentMaxTouchSpeed = 0f;
			_currentGougeCapped = false;
			if (CurrentGouge != null)
			{
				InGameAudio.PostFabricEvent("Gouge", EventAction.PlaySound);
			}
		}
	}

	private void StopGouging()
	{
		_gouging = false;
		if (!(CurrentGouge == null))
		{
			CurrentGouge.Stop();
			CurrentGouge = null;
			InGameAudio.PostFabricEvent("Gouge", EventAction.StopSound);
			InGameAudio.PostFabricEvent("GougeEnd", EventAction.PlaySound);
		}
	}

	private void UpdateGougeSpeed()
	{
		if (Time.time - _currentGougeStartTime < _TimeToSetGougeSpeed)
		{
			float num = CalculateNormalisedTouchSpeed();
			if (num > _currentMaxTouchSpeed)
			{
				_currentMaxTouchSpeed = num;
			}
			CurrentGouge.Spline.SetMaxTouchSpeed(_currentMaxTouchSpeed);
		}
	}

	private float CalculateNormalisedTouchSpeed()
	{
		float touchSpeed_ScreenWidthsPerSecond = HillInput.Instance.TouchSpeed_ScreenWidthsPerSecond;
		if (touchSpeed_ScreenWidthsPerSecond < _ScreenWidthsPerSecondForAnyGougeSpeedBoost)
		{
			return 0f;
		}
		float num = _ScreenWidthsPerSecondForMaxGougeSpeedBoost - _ScreenWidthsPerSecondForAnyGougeSpeedBoost;
		float num2 = touchSpeed_ScreenWidthsPerSecond - _ScreenWidthsPerSecondForAnyGougeSpeedBoost;
		float value = num2 / num;
		float num3 = Mathf.Clamp(value, 0f, 1f);
		return num3 * num3;
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
