using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObstacleSpawner : MonoBehaviour
{
	public ObstacleSpawnCriteria _Criteria;

	[SerializeField]
	public List<Vector3> _Positions;

	public void SpawnAll(Vector3 spawnPoint)
	{
		ObstacleCast randomCastByCriteria = HillObstacles.Instance.GetRandomCastByCriteria(_Criteria, CurrentHill.Instance.UpgradeLevel);
		if (randomCastByCriteria == null)
		{
			return;
		}
		foreach (Vector3 position in _Positions)
		{
			Spawn(position + spawnPoint, randomCastByCriteria);
		}
	}

	public void SpawnAllForced(Vector3 spawnPoint)
	{
		ObstacleCast randomCastByCriteria = HillObstacles.Instance.GetRandomCastByCriteria(_Criteria, int.MaxValue);
		if (randomCastByCriteria == null)
		{
			return;
		}
		foreach (Vector3 position in _Positions)
		{
			Spawn(position + spawnPoint, randomCastByCriteria);
		}
	}

	public static void Spawn(Vector3 spawnPoint, ObstacleCast obstacleCast, bool respectLength = false, int prefferedColour = -1)
	{
		if ((respectLength && CurrentHill.Instance.ProgressIsBeyondHorizon(spawnPoint.z)) || (obstacleCast.Defintion._Type == ObstacleType.PowerUp && !Pebble.Instance._PowerPlayActive))
		{
			return;
		}
		ObstacleMould obstacleFromPool = HillObstacles.Instance.GetObstacleFromPool(obstacleCast.Name);
		if (obstacleFromPool == null)
		{
			Debug.LogError(string.Format("No obstacles available in pool for {0} colour {1}", obstacleCast.Name, obstacleCast._ColourIndex));
			return;
		}
		int colour = obstacleCast._ColourIndex;
		if (!obstacleCast.Defintion._CanBePurchased)
		{
			colour = ((prefferedColour == -1 || obstacleCast.Defintion._Colours.Count <= prefferedColour) ? UnityEngine.Random.Range(0, obstacleCast.Defintion._Colours.Count) : prefferedColour);
		}
		obstacleFromPool.SetColour(colour);
		obstacleFromPool.Spawn(spawnPoint);
	}

	public void UpdateSpawnPositions()
	{
		_Positions.Clear();
		ObstacleSpawnPosition[] componentsInChildren = GetComponentsInChildren<ObstacleSpawnPosition>();
		ObstacleSpawnPosition[] array = componentsInChildren;
		foreach (ObstacleSpawnPosition obstacleSpawnPosition in array)
		{
			_Positions.Add(obstacleSpawnPosition.transform.localPosition);
		}
	}

	public void DeleteSpawnPositions()
	{
		ObstacleSpawnPosition[] componentsInChildren = GetComponentsInChildren<ObstacleSpawnPosition>();
		ObstacleSpawnPosition[] array = componentsInChildren;
		foreach (ObstacleSpawnPosition obstacleSpawnPosition in array)
		{
			UnityEngine.Object.DestroyImmediate(obstacleSpawnPosition.gameObject);
		}
	}

	public void RegenerateSpawnPositions()
	{
		foreach (Vector3 position in _Positions)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.parent = base.transform;
			gameObject.AddComponent<ObstacleSpawnPosition>();
			gameObject.transform.position = base.transform.position + position;
		}
	}

	public void InitDebugPosition()
	{
		_Positions = new List<Vector3>();
		_Positions.Add(Vector3.zero);
	}

	public void Initialise()
	{
		_Criteria.Initialise();
	}
}
