using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FrontendCameraDirector : MonoBehaviour
{
	private enum FECameraState
	{
		Disabled = 0,
		Idle = 1,
		Animating = 2,
		Waiting = 3
	}

	public static FrontendCameraDirector Instance;

	public List<FrontendCameraVO> _Cameras;

	private FECameraState _currentState;

	private FrontendCameraVO _currentCameraVO;

	private FrontendCameraVO _targetCameraVO;

	private FrontendCameraVO _queuedCameraVO;

	private FrontendCameraVO _previousCameraVO;

	private Camera _currentCameraComponent;

	private Camera _targetCameraComponent;

	private Camera _directedCamera;

	private float _lerpFinishTime;

	private float _lerpStartTime;

	private float _waitTime;

	private float _elapsedTime;

	[method: MethodImpl(32)]
	public static event Action<FrontendCameraVO> CameraReadyEvent;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("Second instance of FrontendCameraDirector ", base.gameObject);
		}
		Instance = this;
		GetJVPCameraObject();
		_directedCamera = GetComponentInChildren<Camera>();
		if (_directedCamera == null)
		{
			Debug.LogError("FrontendCameraDirector.Start(): Unable to find Camera component on GameObject!");
		}
		DisableCamerasOnGameObjects();
		_currentState = FECameraState.Idle;
		AddEventListeners();
	}

	private void OnDestroy()
	{
		RemoveEventListeners();
	}

	private void Update()
	{
		switch (_currentState)
		{
		case FECameraState.Animating:
			DoAnimate();
			break;
		case FECameraState.Waiting:
			DoWait();
			break;
		}
	}

	public void SwitchToCamera(CameraNames.Frontend cameraName)
	{
		FrontendCameraVO cameraVO = _Cameras.Find((FrontendCameraVO item) => item._CameraName == cameraName);
		SwitchToCamera(cameraVO);
	}

	public void SwitchToCamera(FrontendCameraVO cameraVO)
	{
		if (cameraVO == null || !(cameraVO._CameraObject != null))
		{
			return;
		}
		if (_currentState == FECameraState.Animating || _currentState == FECameraState.Waiting)
		{
			_queuedCameraVO = cameraVO;
			return;
		}
		if (cameraVO._Delay > 0f)
		{
			WaitThenSwitch(cameraVO, cameraVO._Delay);
			return;
		}
		_targetCameraComponent = cameraVO._CameraObject.GetComponentInChildren<Camera>();
		_previousCameraVO = _currentCameraVO;
		if (_currentCameraVO != null && cameraVO._TransitionInTime > 0f)
		{
			_currentCameraComponent = _currentCameraVO._CameraObject.GetComponentInChildren<Camera>();
			AnimateToCamera(cameraVO);
		}
		else
		{
			SnapToCamera(cameraVO);
		}
	}

	public void WaitThenSwitch(FrontendCameraVO cameraVO, float waitTime = 0f)
	{
		_elapsedTime = 0f;
		_queuedCameraVO = cameraVO;
		_waitTime = waitTime;
		_currentState = FECameraState.Waiting;
	}

	public void SetCameraIdle()
	{
		_currentState = FECameraState.Idle;
		_queuedCameraVO = null;
	}

	private void AnimateToCamera(FrontendCameraVO cameraVO)
	{
		_lerpStartTime = Time.time;
		_lerpFinishTime = Time.time + cameraVO._TransitionInTime;
		_targetCameraVO = cameraVO;
		_currentState = FECameraState.Animating;
	}

	private void SnapToCamera(FrontendCameraVO cameraVO)
	{
		_currentCameraVO = cameraVO;
		if ((bool)_targetCameraComponent)
		{
			_directedCamera.isOrthoGraphic = _targetCameraComponent.isOrthoGraphic;
			_directedCamera.orthographicSize = _targetCameraComponent.orthographicSize;
		}
		base.transform.position = new Vector3(_currentCameraVO._CameraObject.transform.position.x, _currentCameraVO._CameraObject.transform.position.y, _currentCameraVO._CameraObject.transform.position.z);
		base.transform.rotation = new Quaternion(_currentCameraVO._CameraObject.transform.rotation.x, _currentCameraVO._CameraObject.transform.rotation.y, _currentCameraVO._CameraObject.transform.rotation.z, _currentCameraVO._CameraObject.transform.rotation.w);
		if (FrontendCameraDirector.CameraReadyEvent != null)
		{
			FrontendCameraDirector.CameraReadyEvent(_currentCameraVO);
		}
	}

	private void OnAnimateComplete()
	{
		_currentCameraVO = _targetCameraVO;
		_currentState = FECameraState.Idle;
		if (FrontendCameraDirector.CameraReadyEvent != null)
		{
			FrontendCameraDirector.CameraReadyEvent(_currentCameraVO);
		}
		if (_currentCameraVO._HoldTime > 0f)
		{
			WaitThenSwitch(_previousCameraVO, _currentCameraVO._HoldTime);
		}
		else if (_queuedCameraVO != null)
		{
			FrontendCameraVO queuedCameraVO = _queuedCameraVO;
			_queuedCameraVO = null;
			SwitchToCamera(queuedCameraVO);
		}
	}

	private void DisableCamerasOnGameObjects()
	{
		foreach (FrontendCameraVO camera in _Cameras)
		{
			if (camera._CameraObject != null)
			{
				Camera componentInChildren = camera._CameraObject.transform.GetComponentInChildren<Camera>();
				if (componentInChildren != null)
				{
					componentInChildren.enabled = false;
				}
			}
		}
	}

	private void DoAnimate()
	{
		if ((bool)_targetCameraComponent)
		{
			_directedCamera.isOrthoGraphic = _targetCameraComponent.isOrthoGraphic;
		}
		if (_lerpFinishTime > Time.time)
		{
			float num = (Time.time - _lerpStartTime) / (_lerpFinishTime - _lerpStartTime);
			num = 1f - num;
			num *= num;
			num *= num;
			num = 1f - num;
			Vector3 vector = Vector3.Lerp(_currentCameraVO._CameraObject.transform.position, _targetCameraVO._CameraObject.transform.position, num);
			float orthographicSize = Mathf.Lerp(_currentCameraComponent.orthographicSize, _targetCameraComponent.orthographicSize, num);
			base.transform.position = new Vector3(vector.x, vector.y, vector.z);
			_directedCamera.orthographicSize = orthographicSize;
		}
		else
		{
			OnAnimateComplete();
		}
	}

	private void DoWait()
	{
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime > _waitTime)
		{
			_currentState = FECameraState.Idle;
			FrontendCameraVO frontendCameraVO = _queuedCameraVO.Clone();
			frontendCameraVO._Delay = 0f;
			_queuedCameraVO = null;
			SwitchToCamera(frontendCameraVO);
		}
	}

	private void AddEventListeners()
	{
		FrontendWorldController.StateChangeEvent += OnFEStateChange;
	}

	private void RemoveEventListeners()
	{
		FrontendWorldController.StateChangeEvent -= OnFEStateChange;
	}

	private void GetJVPCameraObject()
	{
		foreach (FrontendCameraVO camera in _Cameras)
		{
			if (camera._CameraObject == null)
			{
				if (camera._CameraName == CameraNames.Frontend.JVP)
				{
					camera._CameraObject = JVPController.Instance._CameraObject;
				}
				else
				{
					Debug.LogError("FECameraDirector has a camera without a VO that isn't JVP", base.gameObject);
				}
			}
		}
	}

	private void OnFEStateChange(FrontendWorldController.State state)
	{
		if (state == FrontendWorldController.State.Loading)
		{
			_directedCamera.enabled = false;
		}
		else
		{
			_directedCamera.enabled = true;
		}
		switch (state)
		{
		case FrontendWorldController.State.HillSelection:
			SwitchToCamera(CameraNames.Frontend.HillSide);
			break;
		case FrontendWorldController.State.JVP:
			SwitchToCamera(CameraNames.Frontend.JVP);
			break;
		case FrontendWorldController.State.Story:
			SwitchToCamera(CameraNames.Frontend.Story);
			break;
		case FrontendWorldController.State.BuyingHillItem:
			SwitchToCamera(CameraNames.Frontend.HillSideBuyItem);
			break;
		}
	}
}
