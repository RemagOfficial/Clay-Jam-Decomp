using Fabric;
using UnityEngine;

[AddComponentMenu("Obstacle Creation/Obstacle")]
public class ObstacleMould : MonoBehaviour
{
	private enum State
	{
		Unspawned = 0,
		Active = 1,
		Bounced = 2,
		Offscreen = 3
	}

	private const int CollectableClayIndex = 0;

	public GameObject _VisualsPrefab;

	private ObstacleMovement _movement;

	private ObstacleParticles _particles;

	private ClayData _clay = new ClayData(0, 0f);

	private bool DoSquash = true;

	private bool _canBeSquashed;

	private bool _doLifeTimeSound;

	private bool _playingLifeTimeSound;

	private bool _doEmergeSound;

	private Pebble _pebble;

	private bool _touchingPebble;

	private bool _closeEnoughForAudio;

	private int _touchingColliderIndex;

	private ObstacleBounceBehaviour _bounceBehaviour;

	public ObstacleDefinition Definition { get; private set; }

	public float ActualSize
	{
		get
		{
			return Definition._Size;
		}
	}

	public float Size
	{
		get
		{
			if (DoSquash)
			{
				return Mathf.Min(Definition._Size, _pebble.PowerUpManager.SquashSize);
			}
			return Definition._Size;
		}
	}

	private float VisualScale
	{
		get
		{
			if (DoSquash && Definition._Squashable && Type != ObstacleType.PowerUp)
			{
				return _pebble.PowerUpManager.SquashVisuals * Definition._VisualScale;
			}
			return Definition._VisualScale;
		}
	}

	public float ClaySize
	{
		get
		{
			float result = Definition.ClayValueForColour(ColourIndex);
			if (DoSquash)
			{
				return result;
			}
			return result;
		}
	}

	public ObstacleType Type
	{
		get
		{
			return Definition._Type;
		}
	}

	public float CollisionRadiusMetres { get; private set; }

	public HSVColour Colour { get; private set; }

	public int ColourIndex { get; private set; }

	public ClayData ClayCollectedWhenSquashed
	{
		get
		{
			_clay._Amount = ClaySize;
			return _clay;
		}
	}

	public ObstacleVisuals Visuals { get; private set; }

	private bool PebbleSquashIsOn
	{
		get
		{
			return _pebble.PowerUpManager.SquashIsOn;
		}
	}

	private State CurrentState { get; set; }

	public bool TouchesPebble
	{
		get
		{
			return IsActive() && _touchingPebble;
		}
	}

	public int TouchColliderIndex
	{
		get
		{
			return _touchingColliderIndex;
		}
	}

	public bool IsActive()
	{
		return CurrentState == State.Active;
	}

	public bool CanSpawn()
	{
		return CurrentState == State.Unspawned;
	}

	private void Update()
	{
		if (!InGameController.Instance.Paused && !CheckForDestruction())
		{
			UpdateSize();
			UpdateSquashableState();
			UpdateState();
		}
	}

	private void FixedUpdate()
	{
		if (!InGameController.Instance.Paused && IsActive())
		{
			UpdateFromMovementComponent();
			CheckPebbleProximity();
		}
	}

	private bool CurrentSpriteAnimationIsCollidable()
	{
		if (Definition._NonCollideableAnimTiles.Count == 0)
		{
			return true;
		}
		int currentTileIndex = Visuals.AnimatedSprite.CurrentTileIndex;
		if (Definition._NonCollideableAnimTiles.Contains(currentTileIndex))
		{
			return false;
		}
		return true;
	}

	private void CheckPebbleProximity()
	{
		if (!CurrentSpriteAnimationIsCollidable())
		{
			return;
		}
		UpdateProximityFlags();
		if (_doLifeTimeSound)
		{
			if (_closeEnoughForAudio && !_playingLifeTimeSound)
			{
				PlayLifetimeAudio();
			}
			else if (!_closeEnoughForAudio && _playingLifeTimeSound)
			{
				StopLifetimeAudio();
			}
		}
		if (_touchingPebble)
		{
			_pebble.HitObstacle(this);
		}
	}

	private void ResetProximityFlags()
	{
		_touchingPebble = false;
		_closeEnoughForAudio = false;
		_touchingColliderIndex = -1;
	}

	private void UpdateProximityFlags()
	{
		ResetProximityFlags();
		int num = Mathf.Max(Definition._CollisionPoints.Count, 1);
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < num; i++)
		{
			if (Definition._CollisionPoints.Count > 0)
			{
				vector = Definition._CollisionPoints[i] * base.transform.localScale.x;
			}
			Vector3 vector2 = base.transform.position + vector + Vector3.forward * CollisionRadiusMetres;
			Vector3 position = _pebble.transform.position;
			float radiusMeters = _pebble.RadiusMeters;
			float num2 = CollisionRadiusMetres + radiusMeters;
			float num3 = ((Type != ObstacleType.Rascal) ? 7f : 4f);
			if (vector2.z > position.z + num3 || vector2.x > position.x + num3 || vector2.x < position.x - num3 || vector2.z < position.z - num3)
			{
				continue;
			}
			_closeEnoughForAudio = true;
			if (!(vector2.z > position.z + num2) && !(vector2.x > position.x + num2) && !(vector2.x < position.x - num2) && !(vector2.z < position.z - num2))
			{
				float num4 = num2 * num2;
				vector2.y = 0f;
				position.y = 0f;
				float sqrMagnitude = (position - vector2).sqrMagnitude;
				if (sqrMagnitude <= num4)
				{
					_touchingPebble = true;
					_touchingColliderIndex = i;
					break;
				}
			}
		}
	}

	public void Initialise()
	{
		_pebble = Pebble.Instance;
		GetDefinition();
		LoadVisuals();
		Visuals.Deactivate();
		LoadParticles();
		SetSquashForWake();
		SetSize();
		if (!string.IsNullOrEmpty(Definition._LifeTimeFabricEvent))
		{
			_doLifeTimeSound = true;
		}
		else
		{
			_doLifeTimeSound = false;
		}
		if (!string.IsNullOrEmpty(Definition._EmergeFabricEvent))
		{
			_doEmergeSound = true;
		}
		else
		{
			_doEmergeSound = false;
		}
		_movement = GetComponent<ObstacleMovement>();
		base.gameObject.SetActiveRecursively(false);
	}

	public void Spawn(Vector3 position)
	{
		base.transform.position = position;
		base.gameObject.SetActiveRecursively(true);
		CurrentState = State.Offscreen;
		if ((bool)_movement)
		{
			_movement.SetStartPosition(position);
		}
		AddToManager();
		InitialiseSquashableState();
		ResetProximityFlags();
		Visuals.OnSpawn();
	}

	public void Unspawn()
	{
		StopLifetimeAudio();
		Visuals.Deactivate();
		base.gameObject.SetActiveRecursively(false);
		CurrentState = State.Unspawned;
	}

	private void RemoveFromHill()
	{
		RemoveFromManager();
		Unspawn();
	}

	private void UpdateState()
	{
		if (CurrentState == State.Active)
		{
			if (!Visuals.IsVisible())
			{
				OnBecomeInvisible();
				CurrentState = State.Offscreen;
			}
		}
		else if (CurrentState == State.Offscreen && Visuals.IsVisible())
		{
			OnBecomeVisible();
			CurrentState = State.Active;
		}
	}

	public void SetMovementAnimation(string movementAnimName)
	{
		Visuals.SetMovementAnimation(movementAnimName);
	}

	public void OnBounce()
	{
		if (Definition._PauseAudioOnBounce)
		{
			PauseLifetimeAudio();
		}
		CurrentState = State.Bounced;
		if ((bool)base.collider)
		{
			base.collider.enabled = false;
		}
		if ((bool)_movement)
		{
			_movement.OnBounce();
		}
		if (Visuals.AnimatedSprite.HasAnim("Bounce"))
		{
			Visuals.PlayAnimationWithCallBack("Bounce", OnBounceFinished);
		}
		else
		{
			OnBounceFinished();
		}
	}

	public void OnBounceFinished()
	{
		if (Definition._PauseAudioOnBounce)
		{
			UnPauseLifetimeAudio();
		}
		CurrentState = State.Active;
	}

	private void OnBecomeVisible()
	{
		Visuals.Activate();
		PlayEmergeAudio();
	}

	private void OnBecomeInvisible()
	{
		Visuals.Deactivate();
	}

	public void OnSquashed()
	{
		if ((bool)_particles)
		{
			if (Definition._Type == ObstacleType.PowerUp)
			{
				_particles.DoPowerupSplat();
			}
			else
			{
				float value = Size / 200f;
				value = Mathf.Clamp01(value);
				_particles.DoSplat(value);
			}
			if (_pebble.PowerUpManager.HeavyIsOn)
			{
				if (PowerupDatabase.Instance.IsFlameUpgraded)
				{
					_particles.DoX4();
				}
				else
				{
					_particles.DoX2();
				}
			}
		}
		Pebble.Instance.PlaySquashAudio(this);
		RemoveFromHill();
	}

	private void InitialiseSquashableState()
	{
		_canBeSquashed = _pebble.CanSquashObstacle(this);
		Visuals.MarkSquashable(_canBeSquashed);
	}

	private void UpdateSquashableState(bool doParticles = true)
	{
		bool flag = _pebble.CanSquashObstacle(this);
		if (flag != _canBeSquashed)
		{
			Visuals.MarkSquashable(flag);
		}
		if (!_canBeSquashed && flag && (bool)_particles && doParticles)
		{
			_particles.MarkSquashable();
		}
		_canBeSquashed = flag;
	}

	private bool CheckForDestruction()
	{
		if (OffBottomOfScreen())
		{
			RemoveFromHill();
			return true;
		}
		return false;
	}

	private void SetSquashForWake()
	{
		DoSquash = PebbleSquashIsOn;
	}

	private void UpdateSize()
	{
		UpdateSizeForSquashPowerup();
	}

	private void UpdateSizeForSquashPowerup()
	{
		if (PebbleSquashIsOn && !DoSquash)
		{
			DoSquash = true;
			UpdateSquashableState(false);
			if (_particles != null)
			{
				_particles.DoBling();
			}
		}
		else if (!PebbleSquashIsOn && DoSquash)
		{
			DoSquash = false;
			UpdateSquashableState(false);
			SetSize();
		}
		if (DoSquash)
		{
			SetSize();
		}
	}

	private void UpdateFromMovementComponent()
	{
		if ((bool)_movement && IsActive())
		{
			_movement.UpdateRegularMovement();
		}
	}

	private bool BelowTopOfScreen()
	{
		return base.transform.position.z < CameraDirector.ScreenTop - 2f;
	}

	private bool OffBottomOfScreen()
	{
		return base.transform.position.z < _pebble.transform.position.z - 10f;
	}

	private bool BeyondHorizon()
	{
		return _pebble.Progress + 15f < base.transform.position.z;
	}

	private void LoadVisuals()
	{
		InstantiateVisualsPrefab();
		if ((bool)Visuals)
		{
			InitialiseVisuals();
		}
	}

	private void InstantiateVisualsPrefab()
	{
		if (_VisualsPrefab == null)
		{
			Debug.LogError(string.Format("VisualsPrefab is missing from Obstacle {0}", base.name), base.gameObject);
			return;
		}
		GameObject gameObject = Object.Instantiate(_VisualsPrefab) as GameObject;
		gameObject.transform.localScale = base.transform.localScale;
		Visuals = gameObject.GetComponent<ObstacleVisuals>();
		if (Visuals == null)
		{
			Debug.LogError(string.Format("VisualsPrefab {0} does not have an ObjectVisuals component on object {1}", _VisualsPrefab.name, base.name), base.gameObject);
		}
	}

	private void InitialiseVisuals()
	{
		Visuals.transform.parent = base.transform;
		Visuals.transform.position = base.transform.position;
		Visuals.transform.rotation = base.transform.rotation;
		Visuals.Initialise(this);
	}

	private void StartEmergeAnim()
	{
		if (Visuals.AnimatedSprite.HasAnim("Emerge"))
		{
			Visuals.PlayAnimation("Emerge");
			Visuals.PauseAnimation();
		}
	}

	private void SetSize()
	{
		if (CurrentHill.Instance.Definition._PebbleHandlingParams != null)
		{
			if (!Definition._Squashable)
			{
				SetScale(1f);
				return;
			}
			float num = CurrentHill.Instance.Definition._PebbleHandlingParams.PebbleRadiusMetersAtSize(Size);
			float scale = num * 2f;
			SetScale(scale);
		}
	}

	private void SetScale(float scale)
	{
		base.transform.localScale = Vector3.one * VisualScale * scale;
		CollisionRadiusMetres = base.transform.localScale.x * 0.5f * Definition._CollisionRadius;
	}

	public void SetColour(int colourIndex)
	{
		if (colourIndex >= Definition._Colours.Count && Definition._Type != ObstacleType.Native && Definition._Type != ObstacleType.PowerUp)
		{
			Debug.LogWarning("Colour " + colourIndex + " not set for " + base.name);
		}
		ColourIndex = colourIndex;
		Colour = Definition.GetColour(colourIndex);
		if (Definition._ClayColoured)
		{
			Visuals.SetColour(Colour);
		}
		else if (Definition._Type == ObstacleType.Native)
		{
			Visuals.SetColour(CurrentHill.Instance.Definition._Colour);
		}
		else if (Definition._Type == ObstacleType.PowerUp)
		{
			Visuals.SetColour(CurrentHill.Instance.Definition._PowerPlayColour);
		}
		if ((bool)_particles)
		{
			_particles.Colour = Colour;
		}
	}

	private void LoadParticles()
	{
		_particles = base.gameObject.GetComponent<ObstacleParticles>();
		if (_particles == null)
		{
			_particles = base.gameObject.AddComponent<ObstacleParticles>();
		}
		if ((bool)_particles)
		{
			_particles.Initialise();
		}
	}

	private void PlayEmergeAudio()
	{
		if (_doEmergeSound)
		{
			InGameAudio.PostFabricEvent(Definition._EmergeFabricEvent, EventAction.PlaySound, null, base.gameObject);
		}
	}

	private void PlayLifetimeAudio()
	{
		if (_doLifeTimeSound)
		{
			InGameAudio.PostFabricEvent(Definition._LifeTimeFabricEvent, EventAction.PlaySound, null, base.gameObject);
			_playingLifeTimeSound = true;
		}
	}

	private void StopLifetimeAudio()
	{
		if (_doLifeTimeSound)
		{
			InGameAudio.PostFabricEvent(Definition._LifeTimeFabricEvent, EventAction.StopSound, null, base.gameObject);
			_playingLifeTimeSound = false;
		}
	}

	private void PauseLifetimeAudio()
	{
		if (_playingLifeTimeSound)
		{
			InGameAudio.PostFabricEvent(Definition._LifeTimeFabricEvent, EventAction.PauseSound, null, base.gameObject);
		}
	}

	private void UnPauseLifetimeAudio()
	{
		if (_playingLifeTimeSound)
		{
			InGameAudio.PostFabricEvent(Definition._LifeTimeFabricEvent, EventAction.UnpauseSound, null, base.gameObject);
		}
	}

	private void AddToManager()
	{
		if (HillObstacles.Instance != null)
		{
			HillObstacles.Instance.Add(this);
		}
	}

	private void RemoveFromManager()
	{
		if (HillObstacles.Instance != null)
		{
			HillObstacles.Instance.Remove(this);
		}
	}

	private void GetDefinition()
	{
		Definition = ObstacleDatabase.Instance.GetDefitnion(base.name);
		if (Definition == null)
		{
			Debug.LogError(string.Format("Defintion not found for Obstacle {0}", base.name), base.gameObject);
		}
	}

	public void SetBounceBehaviour(ObstacleBounceBehaviour bounceBehaviour)
	{
		_bounceBehaviour = bounceBehaviour;
	}

	public void StartBounce(Pebble pebble)
	{
		if (_bounceBehaviour != null)
		{
			_bounceBehaviour.StartBounce(pebble);
		}
		else
		{
			ObstacleBounceBehaviour.DefaultStartBounce(pebble, this);
		}
	}

	public bool UpdateBounce(Pebble pebble)
	{
		if (_bounceBehaviour != null)
		{
			return _bounceBehaviour.UpdateBounce(pebble);
		}
		return ObstacleBounceBehaviour.DefaultBounceUpdate(pebble, this);
	}

	public void OnDrawGizmos()
	{
		Vector3 center = base.transform.position + Vector3.forward * CollisionRadiusMetres;
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(center, CollisionRadiusMetres);
	}

	public bool PlayBespokeSquashAudio()
	{
		if (string.IsNullOrEmpty(Definition._SquashAudio))
		{
			return false;
		}
		InGameAudio.PostFabricEvent(Definition._SquashAudio, EventAction.PlaySound);
		return true;
	}

	public void OnGamePaused()
	{
		PauseLifetimeAudio();
	}

	public void OnGameUnpaused()
	{
		UnPauseLifetimeAudio();
	}

	public bool TouchedByRay(Ray ray)
	{
		return Visuals.TouchedByRay(ray);
	}
}
