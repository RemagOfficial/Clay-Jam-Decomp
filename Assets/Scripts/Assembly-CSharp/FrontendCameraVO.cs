using System;
using UnityEngine;

[Serializable]
public class FrontendCameraVO
{
	public CameraNames.Frontend _CameraName;

	public GameObject _CameraObject;

	public float _Delay;

	public float _TransitionInTime;

	public float _TransitionOutTime;

	public float _HoldTime;

	public float _Easing;

	public FrontendCameraVO Clone()
	{
		FrontendCameraVO frontendCameraVO = new FrontendCameraVO();
		frontendCameraVO._CameraName = _CameraName;
		frontendCameraVO._CameraObject = _CameraObject;
		frontendCameraVO._Delay = _Delay;
		frontendCameraVO._TransitionInTime = _TransitionInTime;
		frontendCameraVO._TransitionOutTime = _TransitionOutTime;
		frontendCameraVO._HoldTime = _HoldTime;
		frontendCameraVO._Easing = _Easing;
		return frontendCameraVO;
	}
}
