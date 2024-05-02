using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObstacleCasts
{
	public List<ObstacleCast> List { get; set; }

	public int Capacity
	{
		get
		{
			return List.Capacity;
		}
	}

	public ObstacleCasts(int capacity)
	{
		List = new List<ObstacleCast>(capacity);
	}

	public int CountWithType(ObstacleType type)
	{
		int num = 0;
		foreach (ObstacleCast item in List)
		{
			if (item.Defintion._Type == type)
			{
				num++;
			}
		}
		return num;
	}

	public ObstacleCast RandomCastMatchingCriteria(ObstacleSpawnCriteriaBase criteria, int currentHillUpgradeLevel)
	{
		ObstacleCast result = null;
		int num = 0;
		foreach (ObstacleCast item in List)
		{
			if (criteria.Matches(item, currentHillUpgradeLevel))
			{
				num++;
				if (UnityEngine.Random.Range(0, num) == 0)
				{
					result = item;
				}
			}
		}
		return result;
	}

	public void Clear()
	{
		List.Clear();
	}
}
