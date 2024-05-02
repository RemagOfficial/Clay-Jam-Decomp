using System.Collections.Generic;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{
	public List<CameraFollow> _HillTopCams;

	public List<CameraFollow> _HorizonCams;

	public List<CameraFollow> _FinalMomentsCams;

	public List<CameraFollow> _FlyingSideCams;

	public List<CameraFollow> _ShowResultsCams;

	private List<CameraFollow> _currentCameras;

	public float _HorizonTransitionTime;

	public float _ZoomLevelTransitionTime;

	public float _ResultsTransitionTime;

	private int _zoomLevel;

	private CameraFollow _lastCamera;

	private float _lerpFinishTime;

	private float _lerpStartTime;

	private bool _fixPosition;

	private static float _screenTopWorldPos;

	private static float _screenBottomWorldPos;

	private static float _screenLeftWorldPos;

	private static float _screenRightWorldPos;

	private Vector3 _shake = Vector3.zero;

	private float _shakeMagnitude;

	private float _shakeTimer;

	private float _shakeDuration;

	public float _ShakeDurationDefault = 0.5f;

	public float _ShakeMagnitudeDefault = 0.3f;

	public float _MinFingerPrintScale;

	public float _MaxFingerPrintScale;

	private float _minZoomDist;

	private float _maxZoomDist;

	private float _zoomRatio;

	private float _fingerPrintRange;

	public static CameraDirector Instance { get; private set; }

	public CameraFollow CurrentCamera { get; private set; }

	public static float ScreenTop
	{
		get
		{
			return _screenTopWorldPos;
		}
	}

	public static float ScreenLeft
	{
		get
		{
			return _screenLeftWorldPos;
		}
	}

	public static float ScreenRight
	{
		get
		{
			return _screenRightWorldPos;
		}
	}

	public static float ScreenBottom
	{
		get
		{
			return _screenBottomWorldPos;
		}
	}

	public static float WorldScreenHeight
	{
		get
		{
			return _screenTopWorldPos - _screenBottomWorldPos;
		}
	}

	public static float WorldScreenWidth
	{
		get
		{
			return _screenRightWorldPos - _screenLeftWorldPos;
		}
	}

	public float FingerPrintScale
	{
		get
		{
			return _MinFingerPrintScale + _zoomRatio * _fingerPrintRange;
		}
	}

	public Quaternion FixedBillboardRotation { get; set; }

	public static float TimeNow
	{
		get
		{
			return Time.time;
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("Second instance of CameraDirector ", base.gameObject);
		}
		Instance = this;
		InitialiseZoomConsts();
	}

	private void Start()
	{
		InGameController.StateChanged += OnStateChanged;
		ZoomLevel.NewZoomLevelEvent += OnZoomLevelChanged;
		SetCamera(null, 0f);
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnStateChanged;
		ZoomLevel.NewZoomLevelEvent -= OnZoomLevelChanged;
	}

	private void FixedUpdate()
	{
		DoUpdate();
	}

	public void StartShake()
	{
		_shakeMagnitude = _ShakeMagnitudeDefault;
		_shakeDuration = _ShakeDurationDefault;
		_shakeTimer = _ShakeDurationDefault;
	}

	private void UpdateShake()
	{
		if (_shakeTimer > 0f)
		{
			_shakeTimer -= Time.deltaTime;
			_shakeTimer = Mathf.Max(0f, _shakeTimer);
			float num = _shakeTimer / _shakeDuration;
			_shake = Random.insideUnitSphere * (_shakeMagnitude * num);
		}
	}

	public void DoUpdate()
	{
		if (CurrentCamera != null)
		{
			Vector3 vector = CurrentCamera._Offset;
			Vector3 vector2 = CurrentCamera._Rotation;
			float num = CurrentCamera._FOV;
			bool inTransition = false;
			if (_lerpFinishTime > TimeNow)
			{
				inTransition = true;
				float num2 = (TimeNow - _lerpStartTime) / (_lerpFinishTime - _lerpStartTime);
				num2 = 1f - num2;
				num2 *= num2;
				num2 *= num2;
				num2 = 1f - num2;
				vector = Vector3.Lerp(_lastCamera._Offset, vector, num2);
				vector2 = Vector3.Lerp(_lastCamera._Rotation, vector2, num2);
				num = Mathf.Lerp(_lastCamera._FOV, num, num2);
			}
			UpdateShake();
			vector += _shake;
			SetValues(vector, vector2, num, inTransition);
		}
	}

	private void SetValues(Vector3 offset, Vector3 parentRotation, float fov, bool inTransition)
	{
		if (!_fixPosition || inTransition)
		{
			Vector3 cameraTarget = GougeInput.CameraTarget;
			if (CurrentGameMode.Type == GameModeType.MonsterLove)
			{
				cameraTarget.z -= 1.5f;
			}
			if (CurrentCamera._MaintainHeight && Pebble.Instance != null)
			{
				cameraTarget.y = Pebble.Instance.RadiusMeters;
			}
			base.transform.parent.position = cameraTarget;
			base.transform.localPosition = offset;
		}
		base.transform.parent.rotation = Quaternion.Euler(parentRotation);
		base.camera.fov = fov;
		_zoomRatio = (offset.z - _minZoomDist) / (_maxZoomDist - _minZoomDist);
		_zoomRatio = Mathf.Clamp(_zoomRatio, 0f, 1f);
		FixedBillboardRotation = Quaternion.LookRotation(base.transform.position - base.transform.parent.position);
	}

	private void LateUpdate()
	{
		if (InGameController.Instance.CurrentState > InGameController.State.ResettingForRun)
		{
			CalculateScreenEdgeWorldPositions();
		}
	}

	public void OnStateChanged(InGameController.State state)
	{
		switch (state)
		{
		case InGameController.State.NotStarted:
			SetCamera(null, 0f);
			break;
		case InGameController.State.Loading:
			SetCamera(null, 0f);
			break;
		case InGameController.State.ResettingForRun:
			ResetForRun();
			break;
		case InGameController.State.WaitingToRoll:
			break;
		case InGameController.State.RollingTop:
			break;
		case InGameController.State.RollingApproach:
			UnFixPosition();
			SetNewCameraList(_HorizonCams, _HorizonTransitionTime);
			break;
		case InGameController.State.RollingFinalMoments:
		{
			FixPosition();
			float num = HillDatabase.Instance._FinalMomentsDistance * 0.5f;
			float transitionTime = num / Pebble.Instance.Speed;
			SetNewCameraList(_FinalMomentsCams, transitionTime);
			break;
		}
		case InGameController.State.Flying:
			UnFixPosition();
			SetNewCameraList(_FlyingSideCams, 0f);
			break;
		case InGameController.State.Landed:
			SetNewCameraList(_ShowResultsCams, _ResultsTransitionTime);
			break;
		case InGameController.State.ShowingResults:
			break;
		case InGameController.State.ConsumedByAvalanche:
			FixPosition();
			break;
		case InGameController.State.ShowingResultsGameOver:
			break;
		}
	}

	public void OnZoomLevelChanged(int zoomLevel, bool zoomOut)
	{
		_zoomLevel = zoomLevel;
		TransitionToNewCamera(_ZoomLevelTransitionTime);
	}

	private void TransitionToNewCamera(float transitionTime)
	{
		if (_currentCameras != null && _currentCameras.Count != 0)
		{
			if (_currentCameras.Count > _zoomLevel)
			{
				SetCamera(_currentCameras[_zoomLevel], transitionTime);
			}
			else
			{
				SetCamera(_currentCameras[_currentCameras.Count - 1], transitionTime);
			}
		}
	}

	private void SetNewCameraList(List<CameraFollow> newCameras, float transitionTime)
	{
		_currentCameras = newCameras;
		TransitionToNewCamera(transitionTime);
	}

	private void SetCamera(CameraFollow newCamera, float transitionTime)
	{
		if (newCamera == null)
		{
			CurrentCamera = null;
			base.camera.enabled = false;
			return;
		}
		_lastCamera = ((CurrentCamera != null) ? CurrentCamera : newCamera);
		CurrentCamera = newCamera;
		_lerpStartTime = TimeNow;
		_lerpFinishTime = _lerpStartTime + transitionTime;
		base.camera.enabled = true;
		FixedUpdate();
	}

	private void CalculateScreenEdgeWorldPositions()
	{
		Ray ray = base.camera.ScreenPointToRay(new Vector3(0f, Screen.height, 0f));
		Plane plane = new Plane(Vector3.up, 0f);
		float enter;
		if (plane.Raycast(ray, out enter))
		{
			_screenTopWorldPos = ray.GetPoint(enter).z;
		}
		Ray ray2 = base.camera.ScreenPointToRay(new Vector3(0f, 0f, 0f));
		if (plane.Raycast(ray2, out enter))
		{
			_screenBottomWorldPos = ray2.GetPoint(enter).z;
		}
		Ray ray3 = base.camera.ScreenPointToRay(new Vector3(-Screen.width / 2, Screen.height / 2, 0f));
		if (plane.Raycast(ray3, out enter))
		{
			Vector3 point = ray3.GetPoint(enter);
			_screenLeftWorldPos = point.x;
			float num = base.transform.parent.position.x - point.x;
			_screenRightWorldPos = base.transform.parent.position.x + num;
		}
	}

	private void InitialiseZoomConsts()
	{
		_minZoomDist = _HillTopCams[0]._Offset.z;
		_maxZoomDist = _HillTopCams[_HillTopCams.Count - 1]._Offset.z;
		_fingerPrintRange = _MaxFingerPrintScale - _MinFingerPrintScale;
	}

	private void FixPosition()
	{
		_fixPosition = true;
	}

	private void UnFixPosition()
	{
		_fixPosition = false;
	}

	private void ResetForRun()
	{
		_screenTopWorldPos = 10f;
		_screenBottomWorldPos = -2f;
		_screenLeftWorldPos = -6f;
		_screenRightWorldPos = 6f;
		GougeInput.ResetCameraForRun();
		_zoomLevel = 0;
		UnFixPosition();
		SetNewCameraList(_HillTopCams, 0f);
	}
}
