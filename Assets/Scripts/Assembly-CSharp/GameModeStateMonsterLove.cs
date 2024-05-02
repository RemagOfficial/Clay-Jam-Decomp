using System.Runtime.CompilerServices;
using UnityEngine;

public class GameModeStateMonsterLove : GameModeState
{
	private enum State
	{
		Vulnerable = 0,
		Invulnerable = 1
	}

	public delegate void HeartCollectedHandler(int colourIndex, int amount);

	public delegate void HeartBrokenHandler(int colourIndex);

	public delegate void HeartsMatchedHandler(int value);

	public delegate void ObstacleHitHandler();

	public delegate void MultiplierDownHandler();

	private State _currentState;

	private float _nextStateTime;

	private float _timeToEndInvulnerabilityWarning;

	public int _heartMultiplier = 1;

	private int _maxHeartMultiplier = 5;

	public static GameModeStateMonsterLove Instance { get; private set; }

	public int Hearts { get; private set; }

	public bool[] ColoursCollected { get; private set; }

	public ObstacleDefinition[] ObstacleDefintionsCollected { get; private set; }

	public ClayCollection ClayCollected { get; private set; }

	public bool HasColourToMatch
	{
		get
		{
			return ColoursCollected[0] || ColoursCollected[1] || ColoursCollected[2];
		}
	}

	private bool MatchComplete
	{
		get
		{
			return ColoursCollected[0] && ColoursCollected[1] && ColoursCollected[2];
		}
	}

	[method: MethodImpl(32)]
	public static event HeartCollectedHandler HeartCollectedEvent;

	[method: MethodImpl(32)]
	public static event HeartBrokenHandler HeartBrokenEvent;

	[method: MethodImpl(32)]
	public static event HeartsMatchedHandler HeartsMatchedEvent;

	[method: MethodImpl(32)]
	public static event ObstacleHitHandler ObstalceHitEvent;

	[method: MethodImpl(32)]
	public static event MultiplierDownHandler MultiplierDownEvent;

	public bool HasAHeart(ObstacleMould obstacle)
	{
		if (!HasColourToMatch)
		{
			return true;
		}
		return CanBeCollected(obstacle.ColourIndex);
	}

	public static bool IsMatchable(ObstacleDefinition defintion)
	{
		return defintion.IsAMonster;
	}

	public void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of GameModeStateMonsterLove", base.gameObject);
		}
		Instance = this;
		ColoursCollected = new bool[3];
		ObstacleDefintionsCollected = new ObstacleDefinition[3];
		ClayCollected = new ClayCollection(ColourDatabase.NumCollectableColours);
	}

	private void Update()
	{
		State currentState = _currentState;
		if (currentState == State.Invulnerable)
		{
			UpdateInvulnerability();
		}
	}

	public override void OnResetForRun()
	{
		_currentState = State.Vulnerable;
		Hearts = 0;
		ClayCollected.Clear();
		ClearMatch();
		ClearMultiplier();
	}

	public override void OnObstacleSquash(ObstacleMould obstacle)
	{
		if (IsMatchable(obstacle.Definition))
		{
			if (!CanBeCollected(obstacle.ColourIndex))
			{
				BreakHearts();
			}
			AddMatch(obstacle);
			if (MatchComplete)
			{
				MatchHearts();
			}
		}
	}

	public override void OnObstacleBounce(ObstacleMould obstacle)
	{
		if (_currentState != State.Invulnerable)
		{
			BreakHearts();
			StartInvulnerability(0.5f);
			if (GameModeStateMonsterLove.ObstalceHitEvent != null)
			{
				GameModeStateMonsterLove.ObstalceHitEvent();
			}
		}
	}

	public override bool CanSquashObstacle(Pebble pebble, ObstacleMould obstacle)
	{
		if (!obstacle.Definition._Squashable)
		{
			return false;
		}
		if (obstacle.Definition._Type == ObstacleType.PowerUp)
		{
			return true;
		}
		if (pebble.PowerUpManager.SquashIsOn)
		{
			return true;
		}
		return IsMatchable(obstacle.Definition);
	}

	public override float GetCurrentMaxSpeed(Pebble pebble)
	{
		return pebble.MaxSpeedForMonsterLove;
	}

	public override void GiveClayBoost(Pebble pebble)
	{
		ClayData jacketClay = PowerupDatabase.Instance.GetJacketClay(1f);
		float f = jacketClay._Amount * 0.5f;
		int num = Mathf.CeilToInt(f);
		Hearts += num;
	}

	private bool CanBeCollected(int colourIndex)
	{
		return !ColoursCollected[colourIndex];
	}

	private void AddMatch(ObstacleMould obstacle)
	{
		int colourIndex = obstacle.ColourIndex;
		ColoursCollected[colourIndex] = true;
		ObstacleDefintionsCollected[colourIndex] = obstacle.Definition;
		int heartValue = obstacle.Definition.HeartValue;
		heartValue *= _heartMultiplier;
		heartValue *= Pebble.Instance.PowerUpManager.FlameMultiplier;
		Hearts += heartValue;
		if (GameModeStateMonsterLove.HeartCollectedEvent != null)
		{
			GameModeStateMonsterLove.HeartCollectedEvent(colourIndex, heartValue);
		}
	}

	private void BreakHearts()
	{
		if (GameModeStateMonsterLove.HeartBrokenEvent != null)
		{
			for (int i = 0; i < 3; i++)
			{
				if (ColoursCollected[i])
				{
					GameModeStateMonsterLove.HeartBrokenEvent(i);
				}
			}
		}
		ClearMatch();
		ClearMultiplier();
		if (GameModeStateMonsterLove.MultiplierDownEvent != null)
		{
			GameModeStateMonsterLove.MultiplierDownEvent();
		}
	}

	private void ClearMatch()
	{
		for (int i = 0; i < 3; i++)
		{
			ColoursCollected[i] = false;
			ObstacleDefintionsCollected[i] = null;
		}
	}

	public void MatchHeartsDebug()
	{
		IncreaseMultiplier();
		if (GameModeStateMonsterLove.HeartsMatchedEvent != null)
		{
			GameModeStateMonsterLove.HeartsMatchedEvent(_heartMultiplier);
		}
		SmartBomb();
		ClearMatch();
	}

	private void MatchHearts()
	{
		IncreaseMultiplier();
		if (GameModeStateMonsterLove.HeartsMatchedEvent != null)
		{
			GameModeStateMonsterLove.HeartsMatchedEvent(_heartMultiplier);
		}
		SmartBomb();
		ClearMatch();
	}

	private void IncreaseMultiplier()
	{
		if (_heartMultiplier < _maxHeartMultiplier)
		{
			_heartMultiplier++;
		}
	}

	private void ClearMultiplier()
	{
		_heartMultiplier = 1;
	}

	private void SmartBomb()
	{
		HillObstacles.Instance.SquashEveryObstacleOnScreen();
		Pebble.Instance.Pulse();
	}

	private void StartInvulnerability(float time)
	{
		_nextStateTime = Time.time + time;
		_currentState = State.Invulnerable;
	}

	private void UpdateInvulnerability()
	{
		if (_nextStateTime <= Time.time)
		{
			StopInvulnerability();
		}
	}

	private void StopInvulnerability()
	{
		_currentState = State.Vulnerable;
	}

	public ClayCollection ConvertHeartsToClayCollection()
	{
		float f = (float)Hearts * CurrentHill.Instance.ClayPerHeart;
		f = Mathf.Ceil(f);
		ClayCollected.AddSingleColour(0, f);
		return ClayCollected;
	}
}
