using System;
using System.Runtime.CompilerServices;
using Fabric;
using UnityEngine;

public class BossMonster : ManagedComponent
{
	private enum State
	{
		NotStarted = 0,
		Walking = 1,
		BeingHit = 2,
		Flying = 3,
		Dieing = 4,
		Landed = 5,
		Disabled = 6
	}

	public UnityEngine.Object _Visuals;

	public string _HorizonAnimationName;

	public string _HitAnimationName;

	public string _FlyingAnimationName;

	public string _AudioComponentName;

	public UnityEngine.Object _DeathSplat;

	private BossMonsterVisuals _visuals;

	public static Transform HitTransform;

	public static float ProgressToStartFlying;

	private bool _hitAnimStarted;

	private bool _hitAnimFinished;

	private static string HitBossAudioEventName = "HitBoss";

	private static string BossLandedAudioEventName = "BossLanded";

	private static string BossWalkAudioEventName = "BossWalk";

	private Vector3 _splatOffset = new Vector3(0f, 3.5f, 0f);

	private static string SplatParticleResourcePath = "BossMonster/BossSplat";

	private static UnityEngine.Object SplatPrefab;

	private Vector3 _splatPos;

	private float _startWalkingProgress;

	public static BossMonster Instance { get; private set; }

	private State CurrentState { get; set; }

	[method: MethodImpl(32)]
	public static event Action BossHit;

	[method: MethodImpl(32)]
	public static event Action BossHitComplete;

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one BossMonster instance", base.gameObject);
		}
		Instance = this;
		InGameController.StateChanged += OnStateChanged;
		InGameController.PausedEvent += OnGamePaused;
		InGameController.UnpausedEvent += OnGameUnpaused;
	}

	protected override bool DoInitialise()
	{
		LoadVisuals();
		LoadParticles();
		GetHitPoint();
		Reset();
		return true;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnStateChanged;
		InGameController.PausedEvent -= OnGamePaused;
		InGameController.UnpausedEvent -= OnGameUnpaused;
		Instance = null;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (CurrentState != State.Disabled)
		{
			DoHit();
		}
	}

	private void DoHit()
	{
		if (CurrentState == State.Walking)
		{
			_visuals.PlayAnimationWithCallBack(_HitAnimationName, OnHitAnimationFinished);
			CurrentState = State.BeingHit;
			_hitAnimFinished = false;
			FlyingGUIController.Instance.PlayBossHitAnim();
			InGameAudio.PostFabricEvent(BossWalkAudioEventName, EventAction.StopSound);
			InGameAudio.PostFabricEvent(HitBossAudioEventName);
			if (BossMonster.BossHit != null)
			{
				BossMonster.BossHit();
			}
		}
	}

	private void FixedUpdate()
	{
		if (CurrentState == State.NotStarted && Pebble.Instance.Progress > _startWalkingProgress)
		{
			StartWalking();
		}
		if (CurrentState == State.Walking)
		{
			Vector3 position = base.transform.position;
			position.x = Pebble.Position.x;
			base.transform.position = position;
			DoBillBoard();
		}
		else if (CurrentState == State.BeingHit)
		{
			Vector3 position2 = base.transform.position;
			if (position2.z <= Pebble.ProgressSafe)
			{
				position2.z = Pebble.ProgressSafe;
				if (_hitAnimFinished && position2.z >= ProgressToStartFlying)
				{
					CompleteHit();
				}
			}
			base.transform.position = position2;
			DoBillBoard();
		}
		else if (CurrentState == State.Flying)
		{
			float y = base.transform.position.y;
			PositionAndRotateForFlying();
			float y2 = base.transform.position.y;
			if (y2 < 0f && y >= 0f)
			{
				DoSplat();
			}
			DoBillBoard();
		}
		else if (CurrentState == State.Dieing)
		{
			DoBillBoard();
		}
	}

	public void OnHitAnimationFinished()
	{
		_hitAnimFinished = true;
	}

	private void GetHitPoint()
	{
		HitTransform = base.transform;
	}

	private void CompleteHit()
	{
		CurrentState = State.Flying;
		_visuals.SwitchToSide();
		_visuals.PlayAnimation(_FlyingAnimationName);
		BossMonster.BossHitComplete();
		PositionAndRotateForFlying();
	}

	private void OnStateChanged(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.ResettingForRun:
			Reset();
			break;
		case InGameController.State.Landed:
			CurrentState = State.Landed;
			break;
		case InGameController.State.ShowingResultsGameOver:
			InGameAudio.PostFabricEvent(BossWalkAudioEventName, EventAction.StopSound);
			break;
		}
	}

	private void Reset()
	{
		if (CurrentGameMode.HasBottom)
		{
			base.gameObject.SetActiveRecursively(true);
			CurrentState = State.NotStarted;
			_visuals.SwitchToFront();
			_startWalkingProgress = CurrentHill.Instance.ShowHorizonProgress - 50f;
			_startWalkingProgress = Mathf.Max(_startWalkingProgress, 10f);
			base.enabled = true;
		}
		else
		{
			base.gameObject.SetActiveRecursively(false);
			CurrentState = State.Disabled;
			base.enabled = false;
		}
	}

	private void StartWalking()
	{
		CurrentState = State.Walking;
		_visuals.ChooseMaterial();
		_visuals.PlayAnimation(_HorizonAnimationName);
		InGameAudio.PostFabricEvent(BossWalkAudioEventName, EventAction.PlaySound, null, base.gameObject);
		ProgressToStartFlying = HitTransform.position.z;
	}

	private void LoadVisuals()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_Visuals, base.transform.position, base.transform.rotation) as GameObject;
		_visuals = gameObject.GetComponent<BossMonsterVisuals>();
		_visuals.transform.parent = base.transform;
		_visuals.transform.position = base.transform.position;
		_visuals.transform.rotation = base.transform.rotation;
		_visuals.Initialise();
	}

	private void PositionAndRotateForFlying()
	{
		base.transform.position = Pebble.Position;
		DoBillBoard();
		Vector3 direction = Pebble.Instance.Direction;
		direction.x = 0f;
		direction.Normalize();
		Vector3 lhs = Vector3.Cross(direction, Vector3.right);
		Vector3 forward = base.transform.forward;
		forward.x = 0f;
		forward.Normalize();
		float value = Vector3.Dot(direction, forward);
		value = Mathf.Clamp(value, -1f, 1f);
		float num = Vector3.Dot(lhs, forward);
		float num2 = ((!(num > 0f)) ? (-1f) : 1f);
		float num3 = Mathf.Acos(value);
		num3 *= num2;
		base.transform.RotateAround(base.transform.right, num3);
	}

	private void DoSplat()
	{
		InGameAudio.PostFabricEvent(BossLandedAudioEventName);
		_splatPos = base.transform.position + _splatOffset;
		if (CurrentQuest.Instance.AllQuestsComplete)
		{
			ShowDeathSequence();
		}
		else
		{
			UnityEngine.Object.Instantiate(SplatPrefab, _splatPos, base.transform.rotation);
		}
	}

	private void LoadParticles()
	{
		SplatPrefab = Resources.Load(SplatParticleResourcePath);
	}

	private void DoBillBoard()
	{
		Vector3 position = Camera.main.transform.position;
		base.transform.LookAt(position, Vector3.up);
	}

	public void OnGamePaused()
	{
		if (CurrentState == State.Walking)
		{
			InGameAudio.PostFabricEvent(BossWalkAudioEventName, EventAction.PauseSound, null, base.gameObject);
		}
	}

	public void OnGameUnpaused()
	{
		if (CurrentState == State.Walking)
		{
			InGameAudio.PostFabricEvent(BossWalkAudioEventName, EventAction.UnpauseSound, null, base.gameObject);
		}
	}

	public void ShowDeathSequence()
	{
		UnityEngine.Object.Instantiate(_DeathSplat, _splatPos, Quaternion.identity);
		_visuals.SwitchToFront();
		_visuals.ChooseMaterial(true);
		_visuals.PlayDeathAnimPlusSprite(_HitAnimationName);
		CurrentState = State.Dieing;
	}

	public bool FinishedShowingDeathSequence()
	{
		return !_visuals.IsPlayingDeathAnimation();
	}
}
