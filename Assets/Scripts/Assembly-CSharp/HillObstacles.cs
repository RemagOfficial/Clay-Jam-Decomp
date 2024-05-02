using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillObstacles : ManagedComponent
{
	private ObstacleCasts _availableCasts;

	private List<List<ObstacleMould>> _ObstaclePools = new List<List<ObstacleMould>>(64);

	private List<ObstacleMould> _spawnedObstacles = new List<ObstacleMould>(512);

	private Hashtable _PoolToCastMap = new Hashtable(64);

	public List<Object> _ObstaclePrefabs = new List<Object>();

	public List<ObstaclePoolInfo> _ObstaclePoolInfo;

	private int _numCreaturePoolsLoaded;

	private int _numRascalPoolsLoaded;

	private int _numTrapPoolsLoaded;

	private int _numNativePoolsLoaded;

	private int _numPowerPlayPoolsLoaded;

	private static List<ObstacleMould> squashed = new List<ObstacleMould>(64);

	private static List<ObstacleMould> squashedScreen = new List<ObstacleMould>(64);

	public static HillObstacles Instance { get; set; }

	private HillDefinition Defintion { get; set; }

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("Second insatnce of HillObstacle", base.gameObject);
		}
		Instance = this;
		_numCreaturePoolsLoaded = 0;
		_numRascalPoolsLoaded = 0;
		_numTrapPoolsLoaded = 0;
		_numNativePoolsLoaded = 0;
		InGameController.StateChanged += OnInGameStateChanged;
		InGameController.PausedEvent += OnGamePaused;
		InGameController.UnpausedEvent += OnUnpause;
	}

	private void OnDestroy()
	{
		Instance = null;
		InGameController.StateChanged -= OnInGameStateChanged;
		InGameController.PausedEvent -= OnGamePaused;
		InGameController.UnpausedEvent -= OnUnpause;
	}

	protected override bool DoInitialise()
	{
		Defintion = CurrentHill.Instance.Definition;
		if (!CreateNextObstaclePool())
		{
			return false;
		}
		return true;
	}

	public override void ResetForRun()
	{
		CalculateAvailableObstacles();
		UnspawnAllObstacles();
	}

	public ObstacleCast GetRandomCastByCriteria(ObstacleSpawnCriteriaBase criteria, int currentHillUpgradeLevel = -1)
	{
		return _availableCasts.RandomCastMatchingCriteria(criteria, currentHillUpgradeLevel);
	}

	public ObstacleCast GetCastFromID(CastId castID)
	{
		return _availableCasts.List.Find((ObstacleCast c) => c._Id.Equals(castID));
	}

	public void Add(ObstacleMould mould)
	{
		if (_spawnedObstacles == null)
		{
			_spawnedObstacles = new List<ObstacleMould>();
		}
		_spawnedObstacles.Add(mould);
	}

	public void Remove(ObstacleMould mould)
	{
		_spawnedObstacles.Remove(mould);
	}

	private bool ObstacleIsInScreenArea(ObstacleMould obstacle, Rect screenArea)
	{
		Vector3 vector = Camera.mainCamera.WorldToScreenPoint(obstacle.transform.position);
		Vector2 point = new Vector2(vector.x, vector.y);
		return screenArea.Contains(point);
	}

	public ObstacleMould GetSpawnedObstacleOnScreen_TooBigToSquash(Rect screenArea)
	{
		foreach (ObstacleMould spawnedObstacle in _spawnedObstacles)
		{
			if (Pebble.Instance.CanSquashObstacle(spawnedObstacle) || !ObstacleIsInScreenArea(spawnedObstacle, screenArea))
			{
				continue;
			}
			return spawnedObstacle;
		}
		return null;
	}

	public ObstacleMould GetSpawnedObstacleOnScreen(ObstacleType type, Rect screenArea)
	{
		foreach (ObstacleMould spawnedObstacle in _spawnedObstacles)
		{
			if (spawnedObstacle.Type != type || !ObstacleIsInScreenArea(spawnedObstacle, screenArea))
			{
				continue;
			}
			return spawnedObstacle;
		}
		return null;
	}

	private void CalculateAvailableObstacles()
	{
		if (_availableCasts == null)
		{
			_availableCasts = new ObstacleCasts(SaveData.Instance.Casts.ObjectList.Count);
		}
		_availableCasts.List.Clear();
		bool ignoreLockState = CurrentHill.Instance.Definition.DebugMode || BuildDetails.Instance._DemoMode;
		SaveData.Instance.GetCastsForLevel(_availableCasts, ignoreLockState);
	}

	private bool CreateNextObstaclePool()
	{
		if (_numCreaturePoolsLoaded < Defintion._CreatureSlots.Count)
		{
			string mouldName = Defintion._CreatureSlots[_numCreaturePoolsLoaded]._MouldName;
			CreateMouldPool(mouldName);
			_numCreaturePoolsLoaded++;
			return false;
		}
		if (_numRascalPoolsLoaded < Defintion._RascalSlots.Count)
		{
			string mouldName2 = Defintion._RascalSlots[_numRascalPoolsLoaded]._MouldName;
			CreateMouldPool(mouldName2);
			_numRascalPoolsLoaded++;
			return false;
		}
		if (_numTrapPoolsLoaded < Defintion._TrapSlots.Count)
		{
			string prefabName = Defintion._TrapSlots[_numTrapPoolsLoaded];
			CreateMouldPool(prefabName);
			_numTrapPoolsLoaded++;
			return false;
		}
		if (_numNativePoolsLoaded < Defintion._NativeSlots.Count)
		{
			string prefabName2 = Defintion._NativeSlots[_numNativePoolsLoaded];
			CreateMouldPool(prefabName2);
			_numNativePoolsLoaded++;
			return false;
		}
		if (_numPowerPlayPoolsLoaded < 1)
		{
			CreateMouldPool("PowerPlay");
			_numPowerPlayPoolsLoaded++;
			return false;
		}
		return true;
	}

	private Object GetPrefab(string prefabName)
	{
		return _ObstaclePrefabs.Find((Object p) => p.name == prefabName);
	}

	private void CreateMouldPool(string prefabName)
	{
		ObstaclePoolInfo obstaclePoolInfo = _ObstaclePoolInfo.Find((ObstaclePoolInfo info) => info._Prefab.name == prefabName);
		if (obstaclePoolInfo == null)
		{
			ObstaclePoolInfo obstaclePoolInfo2 = new ObstaclePoolInfo();
			obstaclePoolInfo2._Prefab = GetPrefab(prefabName);
			obstaclePoolInfo2._PoolSize = 1;
			obstaclePoolInfo = obstaclePoolInfo2;
			_ObstaclePoolInfo.Add(obstaclePoolInfo);
		}
		Object prefab = GetPrefab(prefabName);
		if (prefab == null)
		{
			Debug.LogError(string.Format("Trying to load obstacle {0}, that has not had prefab loaded", prefabName));
		}
		else if (!PoolExisits(prefabName))
		{
			int poolIndex = CreatePool(prefabName);
			for (int i = 0; i < obstaclePoolInfo._PoolSize; i++)
			{
				LoadMouldToPool(prefab, poolIndex);
			}
		}
	}

	private ObstacleMould LoadMouldToPool(Object prefab, int poolIndex)
	{
		GameObject gameObject = Object.Instantiate(prefab) as GameObject;
		gameObject.transform.parent = base.transform.parent;
		gameObject.name = prefab.name;
		ObstacleMould component = gameObject.GetComponent<ObstacleMould>();
		component.Initialise();
		_ObstaclePools[poolIndex].Add(component);
		return component;
	}

	public int GetPoolSize(string prefabName)
	{
		int index = PoolIndex(prefabName);
		return _ObstaclePools[index].Count;
	}

	public int GetUsedInPool(string prefabName)
	{
		int index = PoolIndex(prefabName);
		int num = 0;
		foreach (ObstacleMould item in _ObstaclePools[index])
		{
			if (!item.CanSpawn())
			{
				num++;
			}
		}
		return num;
	}

	public ObstacleMould GetObstacleFromPool(string castName)
	{
		if (!PoolExisits(castName))
		{
			Debug.LogError(string.Format("{0} requested from pool but it was never added", castName));
			return null;
		}
		int num = PoolIndex(castName);
		foreach (ObstacleMould item in _ObstaclePools[num])
		{
			if (!item.CanSpawn())
			{
				continue;
			}
			return item;
		}
		Object prefab = GetPrefab(castName);
		return LoadMouldToPool(prefab, num);
	}

	private bool PoolExisits(string name)
	{
		return _PoolToCastMap.ContainsKey(name);
	}

	private int PoolIndex(string name)
	{
		return (int)_PoolToCastMap[name];
	}

	private int CreatePool(string name)
	{
		if (PoolExisits(name))
		{
			Debug.LogError("Creating pool when one already exists");
			return PoolIndex(name);
		}
		int count = _ObstaclePools.Count;
		_ObstaclePools.Add(new List<ObstacleMould>(32));
		_PoolToCastMap[name] = count;
		return count;
	}

	private void OnInGameStateChanged(InGameController.State newState)
	{
		if (newState == InGameController.State.Flying || newState == InGameController.State.ShowingResults || newState == InGameController.State.ShowingResultsGameOver)
		{
			UnspawnAllObstacles();
		}
	}

	public void UnspawnAllObstacles()
	{
		foreach (ObstacleMould spawnedObstacle in _spawnedObstacles)
		{
			spawnedObstacle.Unspawn();
		}
		_spawnedObstacles.Clear();
	}

	public void MakeAllCastsForLevelAvailable()
	{
		_availableCasts.List.Clear();
		SaveData.Instance.GetCastsForLevel(_availableCasts, true);
	}

	private void OnGamePaused()
	{
		foreach (ObstacleMould spawnedObstacle in _spawnedObstacles)
		{
			spawnedObstacle.OnGamePaused();
		}
	}

	private void OnUnpause()
	{
		foreach (ObstacleMould spawnedObstacle in _spawnedObstacles)
		{
			spawnedObstacle.OnGameUnpaused();
		}
	}

	public bool SquashObstaclesWithRay(Ray ray)
	{
		squashed.Clear();
		bool result = false;
		foreach (ObstacleMould spawnedObstacle in _spawnedObstacles)
		{
			if (spawnedObstacle.TouchedByRay(ray))
			{
				result = true;
				squashed.Add(spawnedObstacle);
			}
		}
		foreach (ObstacleMould item in squashed)
		{
			Pebble.Instance.SquashObstacle(item, true);
		}
		squashed.Clear();
		return result;
	}

	public void SquashEveryObstacleOnScreen()
	{
		squashedScreen.Clear();
		foreach (ObstacleMould spawnedObstacle in _spawnedObstacles)
		{
			if (!Pebble.Instance.CanSquashObstacle(spawnedObstacle) && !squashed.Contains(spawnedObstacle))
			{
				squashedScreen.Add(spawnedObstacle);
			}
		}
		foreach (ObstacleMould item in squashedScreen)
		{
			item.OnSquashed();
		}
		squashedScreen.Clear();
	}
}
