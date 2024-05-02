using System;
using System.Runtime.CompilerServices;
using Fabric;
using UnityEngine;

public class Pebble : MonoBehaviour
{
	public enum State
	{
		NotStarted = 0,
		HillRolling = 1,
		Bounced = 2,
		FinalMomentsRolling = 3,
		Flying = 4,
		Landed = 5,
		ConsumedByAvalanche = 6
	}

	private enum SizeForAudio
	{
		NotSet = -1,
		Small = 0,
		Medium = 1,
		Large = 2
	}

	private const string GrowAnim = "AnimGrow";

	private const int FlickGrades = 3;

	private const int FlickPebbleSizes = 3;

	private Vector3 _velocity;

	private float _gougeSpeedBoost;

	private float _momentumSpeedBoost;

	private float _leapSpeedBoost;

	private float _spinSpeedOverride;

	private bool _spinSpeedOverrideSet;

	private GougeSplineWalker SplineWalker;

	private GougeSectionCollider LastHitGougeSection;

	private bool _inGouge;

	private float _distanceTravelledUpHill;

	private float _maxProgress;

	public UnityEngine.Object _ShadowPrefab;

	private GameObject _shadow;

	private SquashMarkController _SquashMarker;

	public UnityEngine.Object _BounceParticlePrefab;

	private GameObject _bounceParticle;

	public GameObject _PebbleModel;

	public UnityEngine.Object _JacketPrefab;

	private GameObject _Jacket;

	private Animation _JacketAnimation;

	private bool _doingFlickAction;

	private float _flickSpeedBoost;

	private Vector3 _flickStartScreenPos = Vector3.zero;

	private float _flickStartTime;

	private float _flickBoostNormalised;

	public ParticleEmitter[] _FlickParticleWeakOptions = new ParticleEmitter[3];

	public ParticleEmitter[] _FlickParticleMediumOptions = new ParticleEmitter[3];

	public ParticleEmitter[] _FlickParticleStrongOptions = new ParticleEmitter[3];

	private ParticleEmitter _flickParticles;

	public ParticleEmitter[] _FlyingParticleWeakOptions = new ParticleEmitter[3];

	public ParticleEmitter[] _FlyingParticleMediumOptions = new ParticleEmitter[3];

	public ParticleEmitter[] _FlyingParticleStrongOptions = new ParticleEmitter[3];

	private ParticleEmitter _flyingParticles;

	public GameObject[] _FlyingFlameEffectOptions = new GameObject[3];

	private GameObject _flyingFlameEffect;

	private float _growAnimTimer;

	private float _growAnimDuration;

	private float _growAnimMaxRadius = 1f;

	private float GrowAnimDuration = 0.15f;

	private float GrowAnimDurationPulse = 0.25f;

	private float GrowAnimRadiusScale = 12f;

	private FlightData _flightData = new FlightData();

	private bool _flickEnded;

	private bool _flickStarted;

	private float _flickEndSpeed;

	private float _size;

	private static string AudioEventObstacleBounce = "BounceObstacle";

	private static string AudioEventPebbleSquash = "SquashPebble";

	private static string AudioEventObstacleSquash = "SquashObstacle";

	private static string AudioEventBounceLand = "BounceLand";

	private static string AudioEventPebbleRoll = "PebbleRoll";

	private bool _pulsing;

	public static Pebble Instance { get; private set; }

	private PebbleHandlingParams Params
	{
		get
		{
			return CurrentHill.Instance.Definition._PebbleHandlingParams;
		}
	}

	public float RadiusScale { get; private set; }

	public float RadiusMeters { get; private set; }

	private float RadiusGained
	{
		get
		{
			return RadiusScale - Params._OriginalRadiusScale;
		}
	}

	public float WidthLimit
	{
		get
		{
			return CurrentHill.Instance.Definition._HillHalfWidth - RadiusMeters * Params._RadiusRatio_TouchingCurb;
		}
	}

	public float ClayCollectedAmount
	{
		get
		{
			return ClayCollected.Amount(0);
		}
	}

	public ClayCollection ClayCollected { get; private set; }

	public int ClayCollected_DisplayAmount { get; set; }

	public Vector3 Velocity
	{
		get
		{
			return _velocity;
		}
		set
		{
			_velocity = value;
			Speed = _velocity.magnitude;
			if (Speed > 0f)
			{
				Direction = _velocity / Speed;
			}
			else
			{
				Direction = Vector3.forward;
			}
		}
	}

	public float Speed { get; private set; }

	public float DownhillSpeed
	{
		get
		{
			return Velocity.z;
		}
	}

	public Vector3 Direction { get; private set; }

	public float MaxSpeedForCurrentRadius
	{
		get
		{
			return Params._BaseMaxSpeed + RadiusGained * Params.MaxSpeedPerRadius;
		}
	}

	public float MaxSpeedForMonsterLove
	{
		get
		{
			return Params.MonsterLoveSpeedForProgress(MaxProgress);
		}
	}

	public float NormalisedSpeedForMonsterLove
	{
		get
		{
			return Params.MonsterLoveNormalisedSpeed(MaxProgress);
		}
	}

	public Vector3 SpinAxis { get; set; }

	private float SpinSpeed
	{
		get
		{
			if (_spinSpeedOverrideSet)
			{
				ResetSpinSpeedOveride();
				return _spinSpeedOverride;
			}
			float num = RadiusMeters * (float)Math.PI * 2f;
			float speed = Speed;
			return speed / num;
		}
	}

	public float SpinSpeedOverride
	{
		set
		{
			_spinSpeedOverrideSet = true;
			_spinSpeedOverride = value;
		}
	}

	private float SpinfactorForCurrentRadius
	{
		get
		{
			return Params._SpinFactorOriginalRadius + RadiusGained * Params.SpinfactorPerRadius;
		}
	}

	private State CurrentState { get; set; }

	private float TimeInState { get; set; }

	public bool CanHitAnyObstacle
	{
		get
		{
			return CurrentState == State.HillRolling;
		}
	}

	public bool Landed
	{
		get
		{
			return CurrentState == State.Landed;
		}
	}

	public bool Launched
	{
		get
		{
			return CurrentState == State.Flying || CurrentState == State.Landed;
		}
	}

	public bool IsInAvalanche
	{
		get
		{
			return CurrentState == State.ConsumedByAvalanche;
		}
	}

	private bool InBottomGougeBoostZome
	{
		get
		{
			return CurrentHill.Instance.ProgressIsBeyondGougeBoostZone(Progress);
		}
	}

	public ObstacleMould BounceObstacle { get; set; }

	private float BounceHeight
	{
		get
		{
			return Params._BounceHeightSmallestSize + RadiusGained * Params.BounceHeightPerRadius;
		}
	}

	private float BounceDistance
	{
		get
		{
			return Params._BounceDistanceSmallestSize + RadiusGained * Params.BounceDistancePerRadius;
		}
	}

	private Vector3 StartPos { get; set; }

	public static Vector3 Position
	{
		get
		{
			return (!Instance) ? Vector3.zero : Instance.transform.position;
		}
	}

	public float HeightOffGround { get; private set; }

	public float Progress
	{
		get
		{
			return base.transform.position.z;
		}
	}

	public static float ProgressSafe
	{
		get
		{
			return (!Instance) ? 0f : Instance.Progress;
		}
	}

	public float MaxProgress
	{
		get
		{
			if (Progress > _maxProgress)
			{
				_maxProgress = Progress;
			}
			return _maxProgress;
		}
		set
		{
			_maxProgress = value;
		}
	}

	public int FlightProgress
	{
		get
		{
			return Mathf.FloorToInt(Progress - _flightData.StartDistance);
		}
	}

	public int DistanceToFly
	{
		get
		{
			return Mathf.FloorToInt(_flightData.DistanceToFly);
		}
	}

	public PowerUpManager PowerUpManager { get; private set; }

	public bool JacketExists { get; private set; }

	public bool JacketComing { get; private set; }

	public bool JacketOn { get; private set; }

	public bool HasFlickBoosted
	{
		get
		{
			return _flickSpeedBoost > 1f;
		}
	}

	public bool _PowerPlayActive { get; set; }

	public float Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
			_size = Mathf.Max(Params._StartSize, _size);
			SetRadius();
			if (Pebble.NewPebbleSize != null)
			{
				Pebble.NewPebbleSize(_size);
			}
		}
	}

	[method: MethodImpl(32)]
	public static event Action<float, float> ClayBouncedOffEvent;

	[method: MethodImpl(32)]
	public static event Action<GameEventsAudio.FlickAudio> NewFlickEvent;

	[method: MethodImpl(32)]
	public static event Action<float> NewPebbleSize;

	private void ResetSpinSpeedOveride()
	{
		_spinSpeedOverrideSet = false;
	}

	private void Awake()
	{
		Instance = this;
		Vector3 position = base.transform.position;
		position.y = 0.25f;
		StartPos = position;
		SetState(State.NotStarted);
		CreateSplineWalker();
		CreateParticles();
		CreateShadow();
		CreateSquashMarks();
		CreatePowerUpManager();
		TurnOffAllFlickParticles();
		InGameController.StateChanged += OnInGameControllerStateChange;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnInGameControllerStateChange;
		UnityEngine.Object.Destroy(_bounceParticle);
		UnityEngine.Object.Destroy(_shadow);
		DestroyFlickParticles();
	}

	private void Update()
	{
		UpdateInputsUsedInFixedUpdate();
	}

	private void FixedUpdate()
	{
		DoFixedUpdate();
		ResetInputsUsedInFixedUpdate();
	}

	private void DoFixedUpdate()
	{
		if (InGameController.Instance.Paused)
		{
			return;
		}
		TimeInState += Time.fixedDeltaTime;
		if (JacketExists)
		{
			if (JacketComing)
			{
				CheckJacketAnimation();
			}
			UpdateJacket();
		}
		switch (CurrentState)
		{
		case State.NotStarted:
			break;
		case State.HillRolling:
			UpdatePhysicsHillRolling();
			UpdateGrowAnim();
			break;
		case State.FinalMomentsRolling:
			UpdatePhysicsFinalMoments();
			UpdateGrowAnim();
			break;
		case State.Bounced:
			UpdatePhysicsBounced();
			break;
		case State.Flying:
			UpdatePhysicsFlying();
			break;
		case State.Landed:
			break;
		case State.ConsumedByAvalanche:
			UpdatePhysicsConsumedByAvalanche();
			break;
		}
	}

	private void ActivateInstantiatedObjects(bool activate)
	{
		_shadow.SetActiveRecursively(activate);
		_bounceParticle.SetActiveRecursively(activate);
	}

	public void ResetForRun()
	{
		SetState(State.NotStarted);
		base.transform.position = StartPos;
		Velocity = Vector3.zero;
		ResetGouging();
		ResetClayCollected();
		_growAnimTimer = float.MaxValue;
		_gougeSpeedBoost = 1f;
		_momentumSpeedBoost = 1f;
		_leapSpeedBoost = ((!BuildDetails.Instance._UseLeapIfAvailable) ? 1f : 1.08f);
		_doingFlickAction = false;
		_flickSpeedBoost = 1f;
		_flickStartScreenPos = Vector3.zero;
		_flickStartTime = 0f;
		_flickBoostNormalised = 0f;
		_PowerPlayActive = false;
		ActivateInstantiatedObjects(true);
		_SquashMarker.StartRun();
		PowerUpManager.StartRun();
		TurnOffAllFlickParticles();
		MaxProgress = 0f;
	}

	public void StartRun()
	{
		SetState(State.HillRolling);
	}

	public void HitObstacle(ObstacleMould obstacle)
	{
		if (CanHitAnyObstacle)
		{
			if (CanSquashObstacle(obstacle))
			{
				SquashObstacle(obstacle);
			}
			else if (CurrentState != State.Bounced)
			{
				BounceOffObstacle(obstacle);
			}
		}
	}

	public bool CanSquashObstacle(ObstacleMould obstacle)
	{
		return CurrentGameMode.GameModeState.CanSquashObstacle(this, obstacle);
	}

	public bool CanSquashObstacle(ObstacleDefinition obstacleDef)
	{
		if (!obstacleDef._Squashable)
		{
			return false;
		}
		return Size >= obstacleDef._Size;
	}

	private void Move()
	{
		base.transform.position += Velocity * Time.fixedDeltaTime;
		base.transform.RotateAround(SpinAxis, SpinSpeed * (float)Math.PI * 2f * Time.fixedDeltaTime);
	}

	private void UpdatePhysicsHillRolling()
	{
		UpdateGougeSpeedBoost();
		UpdateMomentumSpeedBoost();
		UpdateFlickSpeedBoost();
		float speed = Speed;
		float num = PowerUpManager.GetAcceleration(Params._Acceleration);
		float currentMaxSpeed = CurrentGameMode.GameModeState.GetCurrentMaxSpeed(this);
		currentMaxSpeed = PowerUpManager.GetMaxSpeed(currentMaxSpeed);
		currentMaxSpeed *= _momentumSpeedBoost;
		currentMaxSpeed *= _gougeSpeedBoost;
		currentMaxSpeed *= _flickSpeedBoost;
		currentMaxSpeed *= _leapSpeedBoost;
		float num2 = Vector3.Dot(Direction, Vector3.forward);
		if (num2 < 0f)
		{
			_distanceTravelledUpHill -= Direction.z * Time.fixedDeltaTime;
			num = speed / Params._TimeToStopWhenTravelingDirectlyUpHill;
			num *= num2;
		}
		else
		{
			_distanceTravelledUpHill = 0f;
		}
		if (!CurrentGameMode.GameModeState.HasLivesLeft)
		{
			num = speed / Params._TimeToStopWhenOutOfLives;
			num *= -1f;
		}
		if (speed > currentMaxSpeed)
		{
			float num3 = (currentMaxSpeed - speed) / 1f;
			if (num3 < num)
			{
				num = num3;
			}
			speed += num * Time.fixedDeltaTime;
		}
		else
		{
			speed += num * Time.fixedDeltaTime;
			speed = Mathf.Clamp(speed, 0f, currentMaxSpeed);
		}
		if (_inGouge && InGameController.Instance.CanFlick)
		{
			LeaveGouge();
		}
		if (_inGouge)
		{
			SetVelocitySpinInGouge(speed);
		}
		else
		{
			Vector3 vector = Vector3.forward * currentMaxSpeed * (Time.fixedDeltaTime / 0.5f);
			Vector3 velocity = Velocity;
			Vector3 vector2 = vector + velocity;
			vector2.Normalize();
			Velocity = vector2 * speed;
			SpinAxis = Vector3.right;
			CheckForNewLeapGouge();
		}
		KeepWithinBounds(speed);
		Move();
		StickToHill();
		if (_flickParticles != null)
		{
			_flickParticles.transform.LookAt(Camera.main.transform);
		}
	}

	private void CheckForNewLeapGouge()
	{
		if (ClayJamInput.IsLeapActive && !InGameController.Instance.CanFlick)
		{
			SetNewLeapGouge();
		}
	}

	private void SetNewLeapGouge()
	{
		if ((bool)GougeInput.CurrentGouge)
		{
			NewGougeStarted(GougeInput.CurrentGouge);
		}
	}

	private void KeepWithinBounds(float speed)
	{
		Vector3 vector = Position + Velocity * Time.fixedDeltaTime;
		float widthLimit = WidthLimit;
		if (!(widthLimit > 0f))
		{
			return;
		}
		bool flag = false;
		if (Velocity.x <= 0f && vector.x <= 0f - widthLimit)
		{
			base.transform.position = new Vector3(0f - widthLimit, Position.y, Position.z);
			flag = true;
		}
		else if (Velocity.x >= 0f && vector.x >= widthLimit)
		{
			base.transform.position = new Vector3(widthLimit, Position.y, Position.z);
			flag = true;
		}
		if (flag)
		{
			LeaveGouge();
			Vector3 velocity = new Vector3(0f - Velocity.x, Velocity.y, Velocity.z);
			Velocity = velocity;
			PlayBounceAudio(null);
			if (InGameController.Instance.CurrentState == InGameController.State.RollingTop)
			{
				CurrentRunStats.Instance.ObstacleBounced("Gutter");
			}
		}
	}

	private void SetVelocitySpinInGouge(float speed)
	{
		if (_distanceTravelledUpHill > Params._DistanceAllowedToTravelUpHill || speed < Params._SpeedToDropOutOfGouge)
		{
			if (!ClayJamInput.IsLeapActive)
			{
				LeaveGouge();
			}
			return;
		}
		float dist = speed * Time.fixedDeltaTime;
		bool stillOnSpline = true;
		Vector3 vector = SplineWalker.Walk(dist, ref stillOnSpline);
		if (!stillOnSpline)
		{
			if (!ClayJamInput.IsLeapActive)
			{
				LeaveGouge();
			}
		}
		else
		{
			Velocity = vector / Time.fixedDeltaTime;
			SpinAxis = Vector3.Cross(SplineWalker.Tangent, Vector3.down);
		}
	}

	private void LoseMomentum(float ratio)
	{
		float num = _momentumSpeedBoost - 1f;
		float num2 = num * (1f - ratio);
		num2 += 1f;
		_momentumSpeedBoost = num2;
	}

	private void UpdateMomentumSpeedBoost()
	{
		if (Direction.z > Params._MomentumDownhillCosineToGain)
		{
			float num = (Params._MomentumMaxBoost - 1f) * (Time.fixedDeltaTime / Params._MomentumBuilupTime);
			float num2 = 1f;
			if (Direction.z < Params._MomentumDownhillCosineForMaxGain)
			{
				num2 = (Direction.z - Params._MomentumDownhillCosineToGain) / (Params._MomentumDownhillCosineForMaxGain - Params._MomentumDownhillCosineToGain);
			}
			float num3 = num * num2;
			_momentumSpeedBoost += num3;
		}
		else if (Direction.z < Params._MomentumDownhillCosineToLose)
		{
			float num4 = (Params._MomentumMaxBoost - 1f) * (Time.fixedDeltaTime / Params._MomentumLossTime);
			float num5 = 1f;
			if (Direction.z > Params._MomentumDownhillCosineForMaxLoss)
			{
				num5 = (Direction.z - Params._MomentumDownhillCosineForMaxLoss) / (Params._MomentumDownhillCosineToLose - Params._MomentumDownhillCosineForMaxLoss);
			}
			float num6 = num4 * num5;
			_momentumSpeedBoost -= num6;
		}
		_momentumSpeedBoost = Mathf.Clamp(_momentumSpeedBoost, 1f, Params._MomentumMaxBoost);
	}

	private void UpdateGougeSpeedBoost()
	{
		if (!_inGouge)
		{
			DecayGougeSpeedBoost();
			return;
		}
		Vector3 vector = Vector3.Normalize(SplineWalker.Tangent);
		if (vector.z > Params._GougeDownHillRatioForMinBoost)
		{
			float num = Mathf.Clamp(vector.z, Params._GougeDownHillRatioForMinBoost, Params._GougeDownHillRatioForMaxBoost);
			float num2 = ((!(num >= Params._GougeDownHillRatioForMaxBoost)) ? ((num - Params._GougeDownHillRatioForMinBoost) / (Params._GougeDownHillRatioForMaxBoost - Params._GougeDownHillRatioForMinBoost)) : 1f);
			float currentSpeedBoost = SplineWalker.Spline.GetCurrentSpeedBoost(InBottomGougeBoostZome);
			float num3 = ((!InBottomGougeBoostZome) ? Params._GougeMaxBoost : Params._GougeMaxBoostInBottomZone);
			float num4 = num3 - Params._GougeMinBoost;
			float num5 = num4 * num2 * currentSpeedBoost;
			num5 += Params._GougeMinBoost;
			float b = ((!InBottomGougeBoostZome) ? 1f : _gougeSpeedBoost);
			_gougeSpeedBoost = Mathf.Max(num5, b);
		}
		else
		{
			DecayGougeSpeedBoost();
		}
	}

	private void DecayGougeSpeedBoost()
	{
		if (!InBottomGougeBoostZome && _gougeSpeedBoost > 1f)
		{
			_gougeSpeedBoost -= Time.fixedDeltaTime * 4f;
		}
		_gougeSpeedBoost = Mathf.Max(_gougeSpeedBoost, 1f);
	}

	private void UpdateFlickSpeedBoost()
	{
		if (_flickParticles != null)
		{
			_flickParticles.useWorldSpace = false;
		}
		if (!InGameController.Instance.CanFlick)
		{
			return;
		}
		bool flag = false;
		if (BuildDetails.Instance._UseLeapIfAvailable)
		{
			flag = _flickEnded;
		}
		else if (!_doingFlickAction)
		{
			if (_flickStarted)
			{
				_flickStartScreenPos = ClayJamInput.CursorScreenPosition;
				_flickStartTime = Time.time;
				_doingFlickAction = true;
			}
		}
		else if (_flickEnded)
		{
			_doingFlickAction = false;
			flag = true;
		}
		if (!flag || (!BuildDetails.Instance._UseLeapIfAvailable && !(ClayJamInput.CursorScreenPosition.y >= _flickStartScreenPos.y)))
		{
			return;
		}
		float num = _flickEndSpeed;
		if (num < 0f)
		{
			float magnitude = (ClayJamInput.CursorScreenPosition - _flickStartScreenPos).magnitude;
			float num2 = Screen.dpi;
			if (num2 == 0f)
			{
				num2 = 72f;
			}
			float num3 = magnitude / num2;
			float num4 = Time.time - _flickStartTime;
			num = num3 / num4;
		}
		float value = num / CurrentHill.Instance.Definition._PebbleHandlingParams._FlickMaxSpeedInchesPerSec;
		value = Mathf.Clamp01(value);
		float num5 = Mathf.Lerp(CurrentHill.Instance.Definition._PebbleHandlingParams._FlickMinBoost, CurrentHill.Instance.Definition._PebbleHandlingParams._FlickMaxBoost, value);
		if (num5 > _flickSpeedBoost)
		{
			SaveData.Instance.Progress._HasFlicked.Set = true;
			_flickSpeedBoost = num5;
			_flickBoostNormalised = value;
		}
		float num6 = (RadiusScale - Params._OriginalRadiusScale) / (Params._MaxRadiusScale - Params._OriginalRadiusScale);
		int num7 = 0;
		if (num6 > 0.66f)
		{
			num7 = 2;
		}
		else if (num6 > 0.33f)
		{
			num7 = 1;
		}
		ParticleEmitter particleEmitter;
		ParticleEmitter flyingParticles;
		if (_flickBoostNormalised > CurrentHill.Instance.Definition._PebbleHandlingParams._MinFlickPowerNormalisedForSpecialFX)
		{
			particleEmitter = _FlickParticleStrongOptions[num7];
			flyingParticles = _FlyingParticleStrongOptions[num7];
			if (_flyingFlameEffect != null)
			{
				_flyingFlameEffect.SetActiveRecursively(false);
			}
			_flyingFlameEffect = _FlyingFlameEffectOptions[num7];
			if (Pebble.NewFlickEvent != null)
			{
				Pebble.NewFlickEvent(GameEventsAudio.FlickAudio.Strong);
			}
		}
		else if (_flickBoostNormalised > CurrentHill.Instance.Definition._PebbleHandlingParams._MinFlickPowerNormalisedForMediumFX)
		{
			particleEmitter = _FlickParticleMediumOptions[num7];
			flyingParticles = _FlyingParticleMediumOptions[num7];
			if (Pebble.NewFlickEvent != null)
			{
				Pebble.NewFlickEvent(GameEventsAudio.FlickAudio.Medium);
			}
		}
		else
		{
			particleEmitter = _FlickParticleWeakOptions[num7];
			flyingParticles = _FlyingParticleWeakOptions[num7];
			if (Pebble.NewFlickEvent != null)
			{
				Pebble.NewFlickEvent(GameEventsAudio.FlickAudio.Weak);
			}
		}
		if (_flickParticles != particleEmitter)
		{
			_flickParticles = particleEmitter;
		}
		if (_flyingParticles != particleEmitter)
		{
			if (_flyingParticles != null)
			{
				_flyingParticles.emit = false;
			}
			_flyingParticles = flyingParticles;
		}
		if (_flickParticles != null)
		{
			_flickParticles.Emit();
		}
		if (_flyingParticles != null)
		{
			_flyingParticles.emit = true;
		}
		if (_flyingFlameEffect != null)
		{
			_flyingFlameEffect.SetActiveRecursively(true);
		}
	}

	private void TurnOffAllFlickParticles()
	{
		for (int i = 0; i < 3; i++)
		{
			_FlickParticleWeakOptions[i].emit = false;
			_FlickParticleMediumOptions[i].emit = false;
			_FlickParticleStrongOptions[i].emit = false;
			_FlyingParticleWeakOptions[i].emit = false;
			_FlyingParticleMediumOptions[i].emit = false;
			_FlyingParticleStrongOptions[i].emit = false;
			_FlyingFlameEffectOptions[i].SetActiveRecursively(false);
			_FlyingFlameEffectOptions[i].transform.parent = base.transform.parent;
			_flyingFlameEffect = null;
			_flyingParticles = null;
			_flyingFlameEffect = null;
		}
	}

	private void DestroyFlickParticles()
	{
		for (int i = 0; i < 3; i++)
		{
			UnityEngine.Object.Destroy(_FlyingFlameEffectOptions[i]);
		}
	}

	private void UpdatePhysicsConsumedByAvalanche()
	{
	}

	private void UpdatePhysicsFinalMoments()
	{
		if (OnHill())
		{
			UpdatePhysicsHillRolling();
		}
		else
		{
			Vector3 vector = BossMonster.HitTransform.position - base.transform.position;
			vector += Vector3.forward;
			vector.Normalize();
			float num = Speed;
			if (num < Params._MinHitMonsterSpeed)
			{
				num += 2f * Time.fixedDeltaTime;
			}
			Velocity = vector * num;
			Move();
		}
		if (_flickParticles != null)
		{
			_flickParticles.transform.LookAt(Camera.main.transform);
		}
	}

	private bool OnHill()
	{
		return !CurrentHill.Instance.ProgressIsBeyondGougeCap(Progress);
	}

	private void OnInGameControllerStateChange(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.ConsumedByAvalanche:
			StartBeingConsumedByAvalanche();
			break;
		case InGameController.State.RollingFinalMoments:
			StartFinalMoments();
			break;
		case InGameController.State.Flying:
			StartFlying();
			break;
		}
	}

	private void StartBeingConsumedByAvalanche()
	{
		Velocity = Vector3.zero;
		SetState(State.ConsumedByAvalanche);
	}

	private void StartFinalMoments()
	{
		_flightData.CalculateForClayAndFlick(ClayCollectedAmount, _flickBoostNormalised);
		SetState(State.FinalMomentsRolling);
	}

	private void StartFlying()
	{
		SetState(State.Flying);
		UpdatePhysicsFlying();
	}

	private void UpdatePhysicsFlying()
	{
		if (_flightData.GetFlightProgressAtTime(TimeInState) > _flickBoostNormalised)
		{
			if (_flyingParticles != null)
			{
				_flyingParticles.emit = false;
			}
			if (_flyingFlameEffect != null)
			{
				_flyingFlameEffect.SetActiveRecursively(false);
			}
		}
		float y = base.transform.position.y;
		float heightATTime = _flightData.GetHeightATTime(TimeInState);
		float y2 = (heightATTime - y) / Time.fixedDeltaTime;
		float z = base.transform.position.z;
		float num = z;
		num = ((!(heightATTime >= 0f)) ? _flightData.LandDistance : _flightData.GetDistAtTime(TimeInState));
		float z2 = (num - z) / Time.fixedDeltaTime;
		Vector3 position = base.transform.position;
		position.y = heightATTime;
		position.z = num;
		base.transform.position = position;
		Velocity = new Vector3(0f, y2, z2);
		base.transform.RotateAround(Vector3.left, (float)Math.PI * 2f * Params._BounceSpinRPS * Time.fixedDeltaTime);
		float num2 = 1f;
		if (!BossMonster.Instance.FinishedShowingDeathSequence())
		{
			num2 = 3f;
		}
		if (TimeInState >= Params._FlyTime + num2)
		{
			SetState(State.Landed);
			if (_flyingParticles != null)
			{
				_flyingParticles.emit = false;
			}
			if (_flyingFlameEffect != null)
			{
				_flyingFlameEffect.SetActiveRecursively(false);
			}
			Time.timeScale = 1f;
		}
	}

	private void UpdatePhysicsBounced_INAIR_UNSUED()
	{
		float num = BounceDistance / Speed;
		Vector3 vector = Vector3.forward * Speed * Time.fixedDeltaTime;
		Vector3 position = base.transform.position + vector;
		base.transform.position = position;
		float num2 = TimeInState / num;
		num2 *= 2f;
		if (num2 > 1f)
		{
			num2 = 2f - num2;
		}
		num2 = 1f - num2;
		num2 *= num2;
		num2 = 1f - num2;
		float height = BounceHeight * num2;
		SetHeight(height);
		base.transform.RotateAround(Vector3.left, (float)Math.PI * 2f * Params._BounceSpinRPS * Time.fixedDeltaTime);
		if (TimeInState >= num)
		{
			LandFromBounce();
		}
	}

	private void UpdatePhysicsBounced()
	{
		if (BounceObstacle != null && BounceObstacle.UpdateBounce(this))
		{
			BounceObstacle = null;
		}
		Move();
		if (BounceObstacle == null)
		{
			SetState(State.HillRolling);
		}
	}

	private void LandFromBounce()
	{
		float speed = Speed;
		Velocity = Vector3.forward * speed;
		SetState(State.HillRolling);
		PlayBounceLandingAudio();
	}

	private void CheckJacketAnimation()
	{
		if (!_JacketAnimation.isPlaying)
		{
			_Jacket.transform.parent = base.transform;
			_Jacket.transform.localPosition = Vector3.zero;
			_Jacket.transform.localScale = Vector3.one;
			_PebbleModel.renderer.enabled = false;
			JacketOn = true;
			JacketComing = false;
			_SquashMarker.ResetAllSquashMarks();
			CurrentGameMode.GameModeState.GiveClayBoost(this);
		}
	}

	private void UpdateJacket()
	{
		if (JacketComing)
		{
			_Jacket.transform.position = base.transform.position;
			Quaternion identity = Quaternion.identity;
			identity.eulerAngles = new Vector3(base.transform.rotation.eulerAngles.x, 0f, 0f);
			_Jacket.transform.rotation = identity;
			_Jacket.transform.localScale = Vector3.one * RadiusForIncomingJacket();
		}
	}

	public void SquashObstacle(ObstacleMould obstacle, bool isSplatFinger = false)
	{
		obstacle.OnSquashed();
		if (isSplatFinger && !PowerupDatabase.Instance.IsSplatUpgraded)
		{
			return;
		}
		if (obstacle.Definition._Type == ObstacleType.PowerUp)
		{
			PowerUpManager.ActivateRandomPowerup();
			CurrentRunStats.Instance.OnPoweUpCollected();
			return;
		}
		CurrentGameMode.GameModeState.OnObstacleSquash(obstacle);
		int colourIndex = -1;
		if (obstacle.Type == ObstacleType.Creature || obstacle.Type == ObstacleType.Rascal)
		{
			colourIndex = obstacle.ColourIndex;
		}
		if (!isSplatFinger)
		{
			CurrentRunStats.Instance.ObstacleSquashed(obstacle.Definition.StatName, colourIndex, PowerUpManager.FlameIsOn, PowerUpManager.SquashIsOn);
		}
	}

	public void GetClayFromObstacle(ObstacleMould obstacle)
	{
		ClayData clayData = new ClayData(obstacle.ClayCollectedWhenSquashed._ColourIndex, obstacle.ClayCollectedWhenSquashed._Amount);
		if (obstacle.Definition._Type == ObstacleType.Rascal)
		{
			clayData._Amount *= CurrentHill.Instance.Definition._RascalMultiplier;
		}
		int amount = (int)GiveClay(clayData);
		FlyingGUIController.Instance.SpawnSquashedAmountIcon(amount, obstacle.Colour, AddDisplayClay);
		_SquashMarker.AddMark(base.transform.rotation, obstacle);
	}

	public void GetClayInstantly(ClayData clay)
	{
		int num = (int)GiveClay(clay, false);
		ClayCollected_DisplayAmount += num;
	}

	private float GiveClay(ClayData clay, bool allowMultiplier = true)
	{
		ClayData clayData = clay;
		if (allowMultiplier)
		{
			clayData = PowerUpManager.GiveClay(clay);
		}
		Size += clayData._Amount;
		ClayCollected.AddSingleColour(clayData);
		return clayData._Amount;
	}

	private void AddDisplayClay(int amount)
	{
		ClayCollected_DisplayAmount += amount;
	}

	private void BounceOffObstacle(ObstacleMould obstacle)
	{
		obstacle.OnBounce();
		DoBounce(obstacle);
		if (obstacle.Definition._BounceLosesClay)
		{
			LoseClayFromBounce(-1f);
		}
		CurrentGameMode.GameModeState.OnObstacleBounce(obstacle);
		CurrentRunStats.Instance.ObstacleBounced(obstacle.Definition.StatName);
	}

	public void DoBounce(ObstacleMould obstacle = null)
	{
		LoseMomentum(Params._MomentumBounceLoss);
		PlayBounceAudio(obstacle);
		LeaveGouge();
		if (obstacle == null)
		{
			Velocity = Vector3.back * Speed;
			return;
		}
		BounceObstacle = obstacle;
		BounceObstacle.StartBounce(this);
		SetState(State.Bounced);
	}

	public void DoJump()
	{
	}

	public void DoJacket()
	{
		if (JacketOn)
		{
			DestroyJacket();
		}
		CreateJacket();
	}

	public void RemoveJacket()
	{
		if (JacketOn)
		{
			DestroyJacket();
		}
	}

	public void LoseClayFromBounce(float amount = -1f)
	{
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			return;
		}
		float num = amount;
		if (num == -1f)
		{
			num = ClayCollectedAmount * (Params._PercentLostPerBounce * 0.01f);
			num = PowerUpManager.LoseClay(num);
		}
		if (num < 1f)
		{
			num = 1f;
		}
		if (num > 0f)
		{
			Size -= num;
			SquashMark squashMark = _SquashMarker.LoseTopLayer();
			if (squashMark != null)
			{
				EmitBounceParticles(squashMark);
			}
			float arg = ClayCollected.Amount(0);
			ClayCollected.Subtract(0, num);
			ClayCollected_DisplayAmount = (int)Mathf.Max(0f, (float)ClayCollected_DisplayAmount - num);
			if (Pebble.ClayBouncedOffEvent != null)
			{
				Pebble.ClayBouncedOffEvent(num, arg);
			}
		}
	}

	public void EnterGougeSection(GougeSectionCollider gougeSection, bool allowUphillStart = true)
	{
		if ((bool)LastHitGougeSection && LastHitGougeSection.Touches(gougeSection))
		{
			LastHitGougeSection = gougeSection;
			return;
		}
		LastHitGougeSection = gougeSection;
		if (gougeSection.Spline != SplineWalker.Spline && (SplineWalker.Spline == null || SplineWalker.Spline.Order <= gougeSection.Spline.Order) && SplineWalker.StartNewSpline(gougeSection, allowUphillStart))
		{
			_inGouge = true;
			_distanceTravelledUpHill = 0f;
			PlayRollingAudio();
		}
	}

	public void LeaveGougeSpline(GougeSpline spline)
	{
		if (SplineWalker.Spline == spline)
		{
			LeaveGouge();
		}
	}

	private void LeaveGouge()
	{
		_inGouge = false;
		SplineWalker.StartNewSpline(null);
		_distanceTravelledUpHill = 0f;
		StopRollingAudio();
	}

	public void SetHeight(float height)
	{
		HeightOffGround = height;
		Vector3 position = base.transform.position;
		position.y = height + RadiusScale * Params._ModelRadiusMeters;
		base.transform.position = position;
	}

	public void StickToHill()
	{
		SetHeight(0f);
	}

	private void SetState(State newState)
	{
		TimeInState = 0f;
		CurrentState = newState;
	}

	private void CreateJacket()
	{
		if (_Jacket == null)
		{
			_Jacket = UnityEngine.Object.Instantiate(_JacketPrefab) as GameObject;
			_JacketAnimation = _Jacket.GetComponent<Animation>();
			JacketExists = true;
			JacketComing = true;
		}
	}

	private void DestroyJacket()
	{
		_JacketAnimation = null;
		UnityEngine.Object.Destroy(_Jacket);
		_Jacket = null;
		_PebbleModel.renderer.enabled = true;
		JacketComing = false;
		JacketOn = false;
		JacketExists = false;
		SetRadius();
	}

	private void CreateSplineWalker()
	{
		SplineWalker = new GougeSplineWalker(base.transform);
	}

	private void CreateParticles()
	{
		_bounceParticle = UnityEngine.Object.Instantiate(_BounceParticlePrefab) as GameObject;
	}

	private void CreateShadow()
	{
		_shadow = UnityEngine.Object.Instantiate(_ShadowPrefab) as GameObject;
	}

	private void CreateSquashMarks()
	{
		_SquashMarker = GetComponent<SquashMarkController>();
	}

	private void CreatePowerUpManager()
	{
		PowerUpManager = GetComponent<PowerUpManager>();
	}

	public void ResetScale()
	{
		SetRadius();
	}

	private void SetRadius()
	{
		float radiusScaleForSize = Params.GetRadiusScaleForSize(Size);
		StartGrowAnim(radiusScaleForSize);
		RadiusScale = radiusScaleForSize;
		base.transform.localScale = Vector3.one * RadiusScale;
		RadiusMeters = RadiusScale * Params._ModelRadiusMeters;
	}

	private void StartGrowAnim(float newSize)
	{
		float radiusScale = RadiusScale;
		float num = newSize - RadiusScale;
		if (!(num < 0f))
		{
			num = Mathf.Clamp(num, 0f, 0.1f);
			_growAnimMaxRadius = radiusScale + num * GrowAnimRadiusScale;
			_growAnimTimer = 0f;
			_growAnimDuration = GrowAnimDuration;
		}
	}

	private void UpdateGrowAnim()
	{
		if (_pulsing || _growAnimTimer <= _growAnimDuration)
		{
			_growAnimTimer += Time.deltaTime;
			float num = _growAnimTimer / _growAnimDuration;
			if (!_pulsing)
			{
				num = Mathf.Clamp01(num);
			}
			float f = num * (float)Math.PI;
			float num2 = Mathf.Sin(f);
			if (num2 < 0f)
			{
				num2 *= -1f;
			}
			float num3 = Mathf.Lerp(RadiusScale, _growAnimMaxRadius, num2);
			base.transform.localScale = Vector3.one * num3;
		}
	}

	private float RadiusForIncomingJacket()
	{
		ClayData jacketClay = PowerupDatabase.Instance.GetJacketClay(Size);
		float size = Size + jacketClay._Amount;
		return Params.GetRadiusScaleForSize(size);
	}

	private void ResetGouging()
	{
		LastHitGougeSection = null;
		_gougeSpeedBoost = 1f;
		LeaveGouge();
	}

	private void ResetClayCollected()
	{
		Size = Params._StartSize;
		if (ClayCollected == null)
		{
			ClayCollected = new ClayCollection(ColourDatabase.NumCollectableColours);
		}
		else
		{
			ClayCollected.Clear();
		}
		ClayCollected_DisplayAmount = 0;
	}

	private void EmitBounceParticles(SquashMark markBouncedOff)
	{
		_bounceParticle.transform.position = base.transform.position;
		_bounceParticle.particleEmitter.Emit();
		HSVColour colour = markBouncedOff.Colour;
		colour.UseOnHSVMaterial(_bounceParticle.renderer.material);
	}

	private void PrizeBoardBulletTime()
	{
		float num = FlyingLandscape.Instance.PrizeStickObject.transform.position.z - 2f;
		float z = base.transform.position.z;
		float num2 = 1f;
		float num3 = num - z;
		if (z < num)
		{
			num2 = Mathf.Abs(0.1f + num3 * 0.9f / (num - (num - 100f)));
			if ((double)num2 > 1.0)
			{
				num2 = 1f;
			}
		}
		else if (z > num + 2f)
		{
			num2 = Mathf.Abs(0.2f + num3 * 0.8f / (num - (num + 100f)));
			if ((double)num2 > 1.0)
			{
				num2 = 1f;
			}
		}
		else
		{
			Time.timeScale = 1f;
		}
		Time.timeScale = num2;
	}

	private static SizeForAudio GetAudioSizeFromClaySizeObstacle(float claySize)
	{
		if (claySize <= 10f)
		{
			return SizeForAudio.Small;
		}
		if (claySize <= 75f)
		{
			return SizeForAudio.Medium;
		}
		return SizeForAudio.Large;
	}

	private static SizeForAudio GetAudioSizeFromClaySizePebble(float claySize)
	{
		if (claySize <= 10f)
		{
			return SizeForAudio.Small;
		}
		if (claySize <= 100f)
		{
			return SizeForAudio.Medium;
		}
		return SizeForAudio.Large;
	}

	private void PlayBounceLandingAudio()
	{
		SizeForAudio audioSizeFromClaySizePebble = GetAudioSizeFromClaySizePebble(Size);
		string parameter = "LandSmall";
		switch (audioSizeFromClaySizePebble)
		{
		case SizeForAudio.Medium:
			parameter = "LandMedium";
			break;
		case SizeForAudio.Large:
			parameter = "LandLarge";
			break;
		}
		InGameAudio.PostFabricEvent(AudioEventBounceLand, EventAction.SetSwitch, parameter);
		InGameAudio.PostFabricEvent(AudioEventBounceLand, EventAction.PlaySound);
	}

	private void PlayBounceAudio(ObstacleMould obstacle)
	{
		if (obstacle != null && !string.IsNullOrEmpty(obstacle.Definition._BounceFabricEvent))
		{
			InGameAudio.PostFabricEvent(obstacle.Definition._BounceFabricEvent);
		}
		else if (obstacle != null)
		{
			SizeForAudio audioSizeFromClaySizeObstacle = GetAudioSizeFromClaySizeObstacle(obstacle.Size);
			string parameter = "BounceObstacleSmall";
			switch (audioSizeFromClaySizeObstacle)
			{
			case SizeForAudio.Medium:
				parameter = "BounceObstacleMedium";
				break;
			case SizeForAudio.Large:
				parameter = "BounceObstacleLarge";
				break;
			}
			InGameAudio.PostFabricEvent(AudioEventObstacleBounce, EventAction.SetSwitch, parameter);
			InGameAudio.PostFabricEvent(AudioEventObstacleBounce, EventAction.PlaySound);
		}
		else
		{
			InGameAudio.PostFabricEvent(AudioEventObstacleBounce, EventAction.SetSwitch, "BounceObstacleLarge");
			InGameAudio.PostFabricEvent(AudioEventObstacleBounce, EventAction.PlaySound);
		}
	}

	public void PlaySquashAudio(ObstacleMould obstacle = null)
	{
		if (obstacle != null && !obstacle.PlayBespokeSquashAudio())
		{
			SizeForAudio audioSizeFromClaySizeObstacle = GetAudioSizeFromClaySizeObstacle(obstacle.Size);
			string parameter = "SquashObstacleSmall";
			switch (audioSizeFromClaySizeObstacle)
			{
			case SizeForAudio.Medium:
				parameter = "SquashObstacleMedium";
				break;
			case SizeForAudio.Large:
				parameter = "SquashObstacleLarge";
				break;
			}
			InGameAudio.PostFabricEvent(AudioEventObstacleSquash, EventAction.SetSwitch, parameter);
			InGameAudio.PostFabricEvent(AudioEventObstacleSquash, EventAction.PlaySound);
		}
		InGameAudio.PostFabricEvent(AudioEventPebbleSquash, EventAction.PlaySound);
	}

	private void PlayRollingAudio()
	{
		StopRollingAudio();
		SizeForAudio audioSizeFromClaySizePebble = GetAudioSizeFromClaySizePebble(Size);
		string parameter = "PebbleRollSmall";
		switch (audioSizeFromClaySizePebble)
		{
		case SizeForAudio.Medium:
			parameter = "PebbleRollMedium";
			break;
		case SizeForAudio.Large:
			parameter = "PebbleRollLarge";
			break;
		}
		InGameAudio.PostFabricEvent(AudioEventPebbleRoll, EventAction.SetSwitch, parameter);
		InGameAudio.PostFabricEvent(AudioEventPebbleRoll, EventAction.PlaySound);
	}

	private void StopRollingAudio()
	{
		InGameAudio.PostFabricEvent(AudioEventPebbleRoll, EventAction.PauseSound);
	}

	public void NewGougeStarted(Gouge gouge)
	{
		bool flag = true;
		if (!PowerUpManager.SplatFingerIsOn && flag)
		{
			GougeSectionCollider firstCollider = gouge.GetFirstCollider();
			if (!(firstCollider == null))
			{
				EnterGougeSection(firstCollider);
			}
		}
	}

	public void ResetToStartSize()
	{
		Size = Params._StartSize;
	}

	public void SetSizeToFirstZoomLevel()
	{
		Size = 100f;
	}

	public void Pulse()
	{
		_growAnimTimer = 0f;
		_growAnimDuration = GrowAnimDurationPulse;
		_growAnimMaxRadius = RadiusScale + RadiusScale * 0.5f;
	}

	public void StartPulsing()
	{
		_pulsing = true;
		Pulse();
	}

	internal void StopPulsing()
	{
		_pulsing = false;
		ResetScale();
	}

	private void UpdateInputsUsedInFixedUpdate()
	{
		if (ClayJamInput.FlickActionStarted)
		{
			_flickStarted = true;
		}
		if (ClayJamInput.FlickActionEnded)
		{
			_flickEnded = true;
			_flickEndSpeed = ClayJamInput.FlickActionSpeed_InchesPerSecond;
		}
	}

	private void ResetInputsUsedInFixedUpdate()
	{
		_flickStarted = false;
		_flickEnded = false;
		_flickEndSpeed = -1f;
	}
}
