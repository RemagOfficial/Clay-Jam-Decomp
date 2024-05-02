using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SplatInput : MonoBehaviour
{
	private enum SplatPhase
	{
		WaitingForTouch = 0,
		HaveTouch_NotSplattedYet = 1,
		Splatted = 2,
		NotATap = 3
	}

	private const float MaxSpeedForTap_ScreenWidthsPerSecond = 0.1f;

	private const float MaxScreenMove = 0.1f;

	private Ray _splatRay;

	private float _touchTime;

	private Vector2 _touchStartScreenPos;

	private SplatPhase _splatPhase;

	private bool SplatTouchDown
	{
		get
		{
			return HillInput.Instance.HasCurrentGroundTouch && Pebble.Instance.PowerUpManager.SplatFingerIsOn;
		}
	}

	private bool TouchIsSlowEnough
	{
		get
		{
			if (Input.multiTouchEnabled && Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began)
			{
				return false;
			}
			return HillInput.Instance.TouchSpeed_ScreenWidthsPerSecond < 0.1f;
		}
	}

	private bool TouchMovedTooMuchForTap
	{
		get
		{
			float num = (float)Screen.width * 0.1f;
			float num2 = num * num;
			return (HillInput.Instance.ScreenTouchPos - _touchStartScreenPos).sqrMagnitude > num2;
		}
	}

	[method: MethodImpl(32)]
	public static event Action<Vector2> SplatFingerTouchDown;

	private void Awake()
	{
		InGameController.StateChanged += OnNewState;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnNewState;
	}

	private void Update()
	{
		switch (_splatPhase)
		{
		case SplatPhase.WaitingForTouch:
			if (SplatTouchDown)
			{
				StartSplatTouch();
			}
			break;
		case SplatPhase.HaveTouch_NotSplattedYet:
			if (_touchTime < 0.1f && !SplatTouchDown)
			{
				DoSplat();
			}
			_touchTime += Time.deltaTime;
			if (_touchTime > 0.25f && !BuildDetails.Instance._UseLeapIfAvailable)
			{
				_splatPhase = SplatPhase.NotATap;
			}
			else if (!SplatTouchDown)
			{
				_splatPhase = SplatPhase.WaitingForTouch;
			}
			else if (TouchMovedTooMuchForTap && !BuildDetails.Instance._UseLeapIfAvailable)
			{
				_splatPhase = SplatPhase.NotATap;
			}
			else if (TouchIsSlowEnough)
			{
				DoSplat();
				_splatPhase = ((!BuildDetails.Instance._UseLeapIfAvailable) ? SplatPhase.Splatted : SplatPhase.WaitingForTouch);
			}
			break;
		case SplatPhase.Splatted:
			if (!SplatTouchDown)
			{
				_splatPhase = SplatPhase.WaitingForTouch;
			}
			break;
		case SplatPhase.NotATap:
			if (!SplatTouchDown)
			{
				_splatPhase = SplatPhase.WaitingForTouch;
			}
			break;
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

	private void StartSplatTouch()
	{
		_splatPhase = SplatPhase.HaveTouch_NotSplattedYet;
		_touchStartScreenPos = HillInput.Instance.ScreenTouchPos;
		_splatRay = HillInput.Instance.CurrentTouchRay;
		_touchTime = 0f;
	}

	private void DoSplat()
	{
		if (SplatInput.SplatFingerTouchDown != null)
		{
			SplatInput.SplatFingerTouchDown(_touchStartScreenPos);
		}
		if (HillObstacles.Instance.SquashObstaclesWithRay(_splatRay))
		{
			InGameAudio.PostFabricEvent("SplatHit");
		}
		else
		{
			InGameAudio.PostFabricEvent("SplatFinger");
		}
	}
}
