using UnityEngine;

public class CurrentRunStats : MonoBehaviour
{
	private GameStats _stats;

	public static CurrentRunStats Instance { get; private set; }

	public GameStats Stats
	{
		get
		{
			return _stats;
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("more than one instance of CurrentRunStats", base.gameObject);
		}
		Instance = this;
		_stats = new GameStats();
		_stats.Initialise();
		InGameController.StateChanged += OnStateChanged;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnStateChanged;
	}

	public void ObstacleSquashed(string name, int colourIndex, bool withFlame, bool withShrink)
	{
		_stats.ObstacleSquashed(name, colourIndex, withFlame, withShrink);
	}

	public void ObstacleBounced(string name)
	{
		_stats.ObstacleBounced(name);
	}

	public void OnPoweUpCollected()
	{
		_stats.OnPoweUpCollected();
	}

	private void OnStateChanged(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.ResettingForRun:
			_stats.Clear();
			break;
		case InGameController.State.Flying:
			_stats.SetMetersFlown(Pebble.Instance.DistanceToFly);
			break;
		}
	}
}
