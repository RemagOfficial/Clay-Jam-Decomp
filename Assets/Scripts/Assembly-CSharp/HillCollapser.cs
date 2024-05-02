using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HillCollapser : MonoBehaviour
{
	public enum State
	{
		WaitingToRoll = 0,
		Rolling = 1,
		Stopped = 2,
		ConsumingPebble = 3,
		ConsumedPebble = 4
	}

	private const int MaxLevel = 2;

	private const int MinLevel = 0;

	private const int StartLevel = 1;

	private const float MetersPerLevel = 1.5f;

	private HillCollapseParams _params;

	public float BackOffSpeed = 3f;

	private float _progressForMaxSpeed;

	private float _currentWarningTime;

	private float _consumeSpeed;

	private State _currentState;

	public static HillCollapser Instance { get; private set; }

	public float PointOfCollapse { get; private set; }

	public float BackOffAmount { get; private set; }

	public float TimeToDeath
	{
		get
		{
			return _params._TimeToDeath;
		}
	}

	public float CurrentSpeed { get; private set; }

	public float NormalisedSpeed { get; private set; }

	public bool Stopped
	{
		get
		{
			return CurrentState == State.Stopped || CurrentState == State.ConsumedPebble;
		}
	}

	public int LevelIndex
	{
		get
		{
			return Level;
		}
	}

	public int Level { get; private set; }

	public State CurrentState
	{
		get
		{
			return _currentState;
		}
		private set
		{
			_currentState = value;
			if (HillCollapser.StateChanged != null)
			{
				HillCollapser.StateChanged(_currentState);
			}
		}
	}

	private float ExtraMetersBack
	{
		get
		{
			if (CurrentGameMode.Type != GameModeType.MonsterLove)
			{
				return 0f;
			}
			return (!ForceDeath) ? ((float)Level * 1.5f) : (_params._MaxDistBehindPebble * -1.1f);
		}
	}

	public bool ReachedPebble
	{
		get
		{
			return PointOfCollapse >= Pebble.Instance.Progress;
		}
	}

	private bool ForceDeath { get; set; }

	[method: MethodImpl(32)]
	public static event Action<State> StateChanged;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of HillCollapser");
		}
		Instance = this;
		InGameController.StateChanged += OnGameStateChanged;
		GameModeStateMonsterLove.HeartsMatchedEvent += OnHeartsMatched;
		GameModeStateMonsterLove.ObstalceHitEvent += OnLoveObstacleHit;
		_currentState = State.WaitingToRoll;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		Instance = null;
		InGameController.StateChanged -= OnGameStateChanged;
		GameModeStateMonsterLove.HeartsMatchedEvent -= OnHeartsMatched;
		GameModeStateMonsterLove.ObstalceHitEvent -= OnLoveObstacleHit;
	}

	private void FixedUpdate()
	{
		if (!InGameController.Instance.Paused)
		{
			switch (CurrentState)
			{
			case State.WaitingToRoll:
				UpdateWaitingToRoll();
				break;
			case State.Rolling:
				UpdateRolling();
				break;
			case State.Stopped:
				UpdateStoped();
				break;
			case State.ConsumingPebble:
				UpdateConsumingPebble();
				break;
			case State.ConsumedPebble:
				break;
			}
		}
	}

	private void UpdateWaitingToRoll()
	{
		if (InGameController.Instance.CurrentRunTime > _params._StartTime)
		{
			CurrentState = State.Rolling;
		}
	}

	private void UpdateRolling()
	{
		if (CurrentHill.Instance.ProgressIsBeyondHorizon(PointOfCollapse))
		{
			CurrentState = State.Stopped;
			return;
		}
		float pointOfCollapse = PointOfCollapse;
		NormalisedSpeed = CalculateNormalisedSpeed();
		CurrentSpeed = CalculateSpeed(NormalisedSpeed);
		pointOfCollapse += CurrentSpeed * Time.fixedDeltaTime;
		float progress = Pebble.Instance.Progress;
		pointOfCollapse = Mathf.Max(pointOfCollapse, progress - (_params._MaxDistBehindPebble + ExtraMetersBack));
		float num = progress - _params._WarningDistanceBehindPebble - Pebble.Instance.RadiusMeters;
		if (!ForceDeath && pointOfCollapse > num)
		{
			_currentWarningTime += Time.fixedDeltaTime;
			if (_currentWarningTime < _params._WarningHoldTime)
			{
				pointOfCollapse = num;
			}
		}
		else
		{
			_currentWarningTime = 0f;
		}
		if (pointOfCollapse < PointOfCollapse)
		{
			pointOfCollapse = PointOfCollapse;
		}
		PointOfCollapse = pointOfCollapse;
	}

	private void UpdateConsumingPebble()
	{
		float pointOfCollapse = PointOfCollapse;
		pointOfCollapse = (PointOfCollapse = pointOfCollapse + CurrentSpeed * Time.fixedDeltaTime);
		if (pointOfCollapse > CameraDirector.ScreenTop)
		{
			CurrentState = State.ConsumedPebble;
			InGameController.Instance.OnPebbleDead();
		}
	}

	private void UpdateStoped()
	{
		if (CurrentHill.Instance.ProgressIsBeyondHorizon(PointOfCollapse) && BackOffAmount > -20f)
		{
			BackOffAmount -= BackOffSpeed * Time.fixedDeltaTime;
		}
	}

	private void OnGameStateChanged(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.ResettingForRun:
			Reset();
			break;
		case InGameController.State.ConsumedByAvalanche:
			StartConsumingPebble();
			break;
		}
	}

	private void StartConsumingPebble()
	{
		CurrentState = State.ConsumingPebble;
		float screenTop = CameraDirector.ScreenTop;
		float num = screenTop - Pebble.Instance.Progress;
		CurrentSpeed = ((!(_params._TimeToConsume > 0f)) ? 1f : (num / _params._TimeToConsume));
	}

	private void Reset()
	{
		base.enabled = true;
		ForceDeath = false;
		HillDefinition definition = CurrentHill.Instance.Definition;
		_params = definition.CollapseParams;
		PointOfCollapse = 0f - _params._DistBehindPebbleTostart - CurrentHill.Instance.Definition._PebbleHandlingParams._ModelRadiusMeters;
		BackOffAmount = 0f;
		_progressForMaxSpeed = definition.LenfthOfUpgradeLevel(definition.MaxUpgradeLevel);
		_currentWarningTime = 0f;
		CurrentSpeed = _params._SpeedAtStart;
		NormalisedSpeed = 0f;
		CurrentState = State.WaitingToRoll;
		Level = 1;
	}

	private float CalculateNormalisedSpeed()
	{
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			return Pebble.Instance.NormalisedSpeedForMonsterLove;
		}
		float progress = Pebble.Instance.Progress;
		float value = progress / _progressForMaxSpeed;
		return Mathf.Clamp01(value);
	}

	private float CalculateSpeed(float normalisedSpeed)
	{
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			if (NormalisedSpeed < 1f)
			{
				float minPebbleSpeedRatio = _params._MinPebbleSpeedRatio;
				minPebbleSpeedRatio = Mathf.Clamp01(minPebbleSpeedRatio);
				float num = 1f - minPebbleSpeedRatio;
				float num2 = minPebbleSpeedRatio + num * NormalisedSpeed;
				float maxSpeedForMonsterLove = Pebble.Instance.MaxSpeedForMonsterLove;
				return maxSpeedForMonsterLove * num2;
			}
			float progress = Pebble.Instance.Progress;
			float num3 = progress - CurrentHill.Instance.Definition._PebbleHandlingParams._MLove_ProgressForMaxSpeed;
			float value = num3 / _params._ProgressPassedMaxPebbleSpeedForMaxRatio;
			value = Mathf.Clamp01(value);
			float maxPebbleSpeedRatio = _params._MaxPebbleSpeedRatio;
			float num4 = maxPebbleSpeedRatio - 1f;
			float num5 = 1f + num4 * value;
			float maxSpeedForMonsterLove2 = Pebble.Instance.MaxSpeedForMonsterLove;
			return maxSpeedForMonsterLove2 * num5;
		}
		return _params._SpeedAtStart + NormalisedSpeed * (_params._SpeedAtEnd - _params._SpeedAtStart);
	}

	public void OnContinue()
	{
		float progress = Pebble.Instance.Progress;
		float num = progress - _params._MaxDistBehindPebble;
	}

	private void OnHeartsMatched(int value)
	{
		Level++;
		if (Level > 2)
		{
			Level = 2;
		}
	}

	private void OnLoveObstacleHit()
	{
		Level--;
		if (Level < 0)
		{
			Level = 0;
			ForceDeath = true;
		}
	}
}
