using System;
using UnityEngine;

[Serializable]
public class ObstacleSpawnCriteria : ObstacleSpawnCriteriaBase
{
	public ObstacleType _Type = ObstacleType.Any;

	public string _Name = string.Empty;

	public int _MinimumHillUpgrade;

	public int _SlotIndex = -1;

	public int _RandomSlotRange = -1;

	public override bool Matches(ObstacleCast cast, int currentHillUpgradeLevel)
	{
		if (_Type != ObstacleType.Any && _Type != cast.Defintion._Type)
		{
			return false;
		}
		if (_MinimumHillUpgrade > currentHillUpgradeLevel)
		{
			return false;
		}
		if (_SlotIndex >= 0)
		{
			bool flag = false;
			if (_RandomSlotRange > 1)
			{
				for (int i = 0; i < _RandomSlotRange; i++)
				{
					if (CurrentHill.Instance.Definition.CastMatchesSlotIndex(cast, _SlotIndex + i))
					{
						flag = true;
						break;
					}
				}
			}
			else if (CurrentHill.Instance.Definition.CastMatchesSlotIndex(cast, _SlotIndex))
			{
				flag = true;
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public void Initialise()
	{
		if (!(_Name != string.Empty))
		{
			return;
		}
		ObstacleDefinition defitnion = ObstacleDatabase.Instance.GetDefitnion(_Name);
		if (defitnion == null)
		{
			Debug.LogError(string.Format("Obstacle name {0} not foiund in spawn criteria", _Name));
			return;
		}
		_Type = defitnion._Type;
		_SlotIndex = CurrentHill.Instance.Definition.GetSlotIndex(_Type, _Name);
		if (_SlotIndex == -1)
		{
			Debug.LogError(string.Format("Spawn criterai using name {0}, which is not a {1} on hill {2}", _Name, _Type, CurrentHill.Instance.ID));
		}
	}
}
