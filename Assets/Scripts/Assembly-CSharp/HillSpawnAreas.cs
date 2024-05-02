using System.Collections.Generic;
using UnityEngine;

public class HillSpawnAreas : ManagedComponent
{
	private List<ObstacleSpawnArea>[] _spawnAreaListsPerGameMode;

	private GameObject[] _objectParentsPerGameMode;

	protected override bool DoInitialise()
	{
		if (_spawnAreaListsPerGameMode != null)
		{
			return true;
		}
		AllocateListForEachGameMode();
		if (CurrentHill.Instance.Definition.DebugMode)
		{
			CreateDebugSpawnAres();
		}
		else
		{
			GetSceneSpawnAreas();
		}
		InitialiseSpawnAreas();
		return false;
	}

	private void AllocateListForEachGameMode()
	{
		_spawnAreaListsPerGameMode = new List<ObstacleSpawnArea>[2];
		for (int i = 0; i < 2; i++)
		{
			_spawnAreaListsPerGameMode[i] = new List<ObstacleSpawnArea>(256);
		}
	}

	public override void ResetForRun()
	{
		for (int i = 0; i < 2; i++)
		{
			if (i == CurrentGameMode.TypeInt)
			{
				ResetForRunGameMode(i);
			}
			else
			{
				TurnOffGameMode(i);
			}
		}
	}

	private void ResetForRunGameMode(int gameMode)
	{
		_objectParentsPerGameMode[gameMode].SetActiveRecursively(true);
		foreach (ObstacleSpawnArea item in _spawnAreaListsPerGameMode[gameMode])
		{
			item.ResetForRun();
		}
	}

	private void TurnOffGameMode(int gameMode)
	{
		_objectParentsPerGameMode[gameMode].SetActiveRecursively(false);
	}

	private void GetSceneSpawnAreas()
	{
		_objectParentsPerGameMode = new GameObject[2];
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int i = 0; i < 2; i++)
		{
			string text = GameModeTypeNames.GetName(i);
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				if (transform.name == text)
				{
					_objectParentsPerGameMode[i] = transform.gameObject;
					break;
				}
			}
			if (_objectParentsPerGameMode[i] != null)
			{
				AddChildSpawnAreasFromObject(_objectParentsPerGameMode[i], _spawnAreaListsPerGameMode[i]);
			}
		}
	}

	private void AddChildSpawnAreasFromObject(GameObject parent, List<ObstacleSpawnArea> areaList)
	{
		ObstacleSpawnArea[] componentsInChildren = parent.GetComponentsInChildren<ObstacleSpawnArea>();
		ObstacleSpawnArea[] array = componentsInChildren;
		foreach (ObstacleSpawnArea item in array)
		{
			areaList.Add(item);
		}
	}

	private void InitialiseSpawnAreas()
	{
		for (int i = 0; i < 2; i++)
		{
			foreach (ObstacleSpawnArea item in _spawnAreaListsPerGameMode[i])
			{
				item.Initialise();
			}
		}
	}

	private void CreateDebugSpawnAres()
	{
		if (CurrentHill.Instance.Definition._AutoSpawnLots)
		{
			AddDebugSpawnAreasLots();
		}
		else if (CurrentHill.Instance.Definition._AutoSpawn)
		{
			AddDebugSpawnAreas();
		}
	}

	private void AddDebugSpawnAreasLots()
	{
		HillDefinition definition = CurrentHill.Instance.Definition;
		float length = CurrentHill.Instance.Length;
		float num = 0f;
		int num2 = 0;
		num = 5f * (float)definition._RascalSlots.Count;
		num2 = 6;
		for (int i = 0; i < definition._RascalSlots.Count; i++)
		{
			AddNewSpawnArea(ObstacleType.Rascal, i, 0f, length, 20f, num, num2);
		}
		num = 15f * (float)definition._CreatureSlots.Count;
		num2 = 8;
		for (int j = 0; j < definition._CreatureSlots.Count; j++)
		{
			AddNewSpawnArea(ObstacleType.Creature, j, 0f, length, 20f, num, num2);
		}
		num = 15f * (float)definition._TrapSlots.Count;
		num2 = 4;
		for (int k = 0; k < definition._TrapSlots.Count; k++)
		{
			AddNewSpawnArea(ObstacleType.Trap, k, 0f, length, 20f, num, num2);
		}
		num = 30f * (float)definition._NativeSlots.Count;
		num2 = 4;
		for (int l = 0; l < definition._NativeSlots.Count; l++)
		{
			AddNewSpawnArea(ObstacleType.Native, l, 0f, length, 20f, num, num2);
		}
	}

	private void AddDebugSpawnAreas()
	{
		HillDefinition definition = CurrentHill.Instance.Definition;
		float num = 0f;
		AddNewSpawnArea(ObstacleType.Rascal, -1, 0f, 500f, 20f, 5f, 6);
		for (int i = 0; i < definition._NativeSlots.Count; i++)
		{
			AddNewSpawnArea(ObstacleType.Native, i, num, num);
			num += 4f;
		}
		for (int j = 0; j < definition._CreatureSlots.Count; j++)
		{
			AddNewSpawnArea(ObstacleType.Creature, j, num, num);
			num += 6f;
		}
		for (int k = 0; k < definition._TrapSlots.Count; k++)
		{
			AddNewSpawnArea(ObstacleType.Trap, k, num, num);
			num += 4f;
		}
	}

	private void AddNewSpawnArea(ObstacleType type, int slotIndex, float startDistance, float endDistance, float spread = 0f, float length = 0f, int numSections = 1, int distBetweenSpawns = 0)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = string.Format("SpawnArea_{0}_slot{1}", type, slotIndex);
		gameObject.transform.parent = base.transform;
		ObstacleSpawnAreaRandom obstacleSpawnAreaRandom = gameObject.AddComponent<ObstacleSpawnAreaRandom>();
		obstacleSpawnAreaRandom._Length = length;
		obstacleSpawnAreaRandom._DistBetweenSpawns = distBetweenSpawns;
		obstacleSpawnAreaRandom._LeftEdge = (0f - spread) / 2f;
		obstacleSpawnAreaRandom._RightEdge = spread / 2f;
		obstacleSpawnAreaRandom._StartDistance = startDistance;
		obstacleSpawnAreaRandom._EndDistance = endDistance;
		obstacleSpawnAreaRandom._NumberOfSections = numSections;
		obstacleSpawnAreaRandom._Spawners = new List<ObstacleSpawner>(1);
		GameObject gameObject2 = new GameObject();
		gameObject2.transform.parent = gameObject.transform;
		ObstacleSpawner obstacleSpawner = gameObject2.AddComponent<ObstacleSpawner>();
		obstacleSpawner.InitDebugPosition();
		obstacleSpawner._Criteria = new ObstacleSpawnCriteria();
		obstacleSpawner._Criteria._Type = type;
		obstacleSpawner._Criteria._SlotIndex = slotIndex;
		obstacleSpawnAreaRandom._Spawners.Add(obstacleSpawner);
		for (int i = 0; i < 2; i++)
		{
			_spawnAreaListsPerGameMode[i].Add(obstacleSpawnAreaRandom);
		}
	}

	public void OnDrawGizmos()
	{
		float hillHalfWidth = CurrentHill.Instance.Definition._HillHalfWidth;
		for (int i = 0; i <= CurrentHill.Instance.Definition.MaxUpgradeLevel; i++)
		{
			float num = CurrentHill.Instance.Definition.LenfthOfUpgradeLevel(i);
			num -= HillDatabase.Instance._ShowHorizonDistance;
			float num2 = num * 0.5f;
			Vector3 center = new Vector3(0f, 0f, num2);
			Vector3 size = new Vector3(hillHalfWidth * 2f, 0f, num2 * 2f);
			Gizmos.DrawWireCube(center, size);
		}
	}
}
