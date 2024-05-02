using System;
using System.Collections.Generic;
using UnityEngine;

public class HillDefinition : MonoBehaviour
{
	[Serializable]
	public class UpgradeData
	{
		public float _Length;

		public float _Cost;

		public string _CreatureUnlock;
	}

	public bool _AutoSpawn;

	public bool _AutoSpawnLots;

	public int _ID;

	public string _Name;

	public List<UpgradeData> _UpgradeLevels;

	public List<CastId> _CreatureSlots;

	public List<CastId> _RascalSlots;

	public List<string> _TrapSlots;

	public List<string> _NativeSlots;

	public LockState _InitialState;

	public ClayCollection _CostCollection;

	public HSVColour _Colour;

	public HSVColour _ColourFromOrange;

	public HSVColour _JVPColour;

	public HSVColour _JVPFeatureColour;

	public HSVColour _PowerPlayColour;

	public float _HillHalfWidth;

	public int _RascalMultiplier = 1;

	public List<HSVColour> _RascalColours;

	public List<PowerplayPack> _PowerPlayPacks;

	[SerializeField]
	private HillCollapseParams _CollapseParams;

	[SerializeField]
	private HillCollapseParams _CollapseParamsMonsterLove;

	public PebbleHandlingParams _PebbleHandlingParams;

	public string _BossMonsterName;

	public float _ClayPerHeart = 1f;

	public bool DebugMode
	{
		get
		{
			return _AutoSpawn || _AutoSpawnLots;
		}
	}

	public HillCollapseParams CollapseParams
	{
		get
		{
			switch (CurrentGameMode.Type)
			{
			case GameModeType.Quest:
				return _CollapseParams;
			case GameModeType.MonsterLove:
				return _CollapseParamsMonsterLove;
			default:
				return _CollapseParams;
			}
		}
	}

	public int MaxUpgradeLevel
	{
		get
		{
			return _UpgradeLevels.Count - 1;
		}
	}

	private void Enable()
	{
		_PebbleHandlingParams.CalculateConstants();
	}

	public float CostOfUpgradeLevel(int upgradeLevel)
	{
		if (upgradeLevel < 0 || upgradeLevel >= _UpgradeLevels.Count)
		{
			Debug.LogError(string.Format("Costing at level {0} not setup for hill {1}", upgradeLevel, _Name, base.gameObject));
			return 0f;
		}
		float num = _UpgradeLevels[upgradeLevel]._Cost;
		if (!BuildDetails.Instance._HasIAP)
		{
			num *= 0.5f;
			num = Mathf.Floor(num);
		}
		return num;
	}

	public float LenfthOfUpgradeLevel(int upgradeLevel)
	{
		if (upgradeLevel < -1 || upgradeLevel >= _UpgradeLevels.Count)
		{
			Debug.LogError(string.Format("Getting length at level {0} not setup for hill {1}", upgradeLevel, _Name, base.gameObject));
			return 0f;
		}
		return _UpgradeLevels[upgradeLevel]._Length;
	}

	public void GetObstacleNameAndColour(ObstacleType type, int slotIndex, out string name, out int colourIndex)
	{
		name = string.Empty;
		colourIndex = -1;
		switch (type)
		{
		case ObstacleType.Creature:
			if (slotIndex < _CreatureSlots.Count)
			{
				name = _CreatureSlots[slotIndex]._MouldName;
				colourIndex = _CreatureSlots[slotIndex]._ColourIndex;
			}
			break;
		case ObstacleType.Rascal:
			if (slotIndex < _RascalSlots.Count)
			{
				name = _RascalSlots[slotIndex]._MouldName;
				colourIndex = _RascalSlots[slotIndex]._ColourIndex;
			}
			break;
		case ObstacleType.Trap:
			if (slotIndex < _TrapSlots.Count)
			{
				name = _TrapSlots[slotIndex];
				colourIndex = 0;
			}
			break;
		case ObstacleType.Native:
			if (slotIndex < _NativeSlots.Count)
			{
				name = _NativeSlots[slotIndex];
				colourIndex = -1;
			}
			break;
		case ObstacleType.PowerUp:
			name = "PowerPlay";
			colourIndex = -1;
			break;
		}
	}

	public int GetSlotIndex(ObstacleType type, string name)
	{
		switch (type)
		{
		case ObstacleType.Creature:
			return _CreatureSlots.FindIndex((CastId s) => s._MouldName == name);
		case ObstacleType.Rascal:
			return _RascalSlots.FindIndex((CastId s) => s._MouldName == name);
		case ObstacleType.Trap:
			return _TrapSlots.FindIndex((string s) => s == name);
		case ObstacleType.Native:
			return _NativeSlots.FindIndex((string s) => s == name);
		case ObstacleType.PowerUp:
			return 0;
		default:
			return -1;
		}
	}

	public bool UsesCast(CastData castData)
	{
		switch (castData.MouldInfo._Type)
		{
		case ObstacleType.Creature:
			return _CreatureSlots.Contains(castData.Id);
		case ObstacleType.Rascal:
			return _RascalSlots.Contains(castData.Id);
		case ObstacleType.Trap:
			return _TrapSlots.Contains(castData.MouldName);
		case ObstacleType.Native:
			return _NativeSlots.Contains(castData.MouldName);
		case ObstacleType.PowerUp:
			return true;
		default:
			return false;
		}
	}

	public bool CastMatchesSlotIndex(ObstacleCast cast, int _SlotIndex)
	{
		switch (cast.Defintion._Type)
		{
		case ObstacleType.Creature:
			if (_SlotIndex < _CreatureSlots.Count)
			{
				return _CreatureSlots[_SlotIndex].Equals(cast._Id);
			}
			Debug.LogError(string.Concat("Looking for Slot ", _SlotIndex + 1, "/", _CreatureSlots.Count, " in ", base.name, " ", cast.Defintion._Type, "s"));
			return false;
		case ObstacleType.Rascal:
			if (_SlotIndex < _RascalSlots.Count)
			{
				return _RascalSlots[_SlotIndex].Equals(cast._Id);
			}
			Debug.LogError(string.Concat("Looking for Slot ", _SlotIndex + 1, "/", _RascalSlots.Count, " in ", base.name, " ", cast.Defintion._Type, "s"));
			return false;
		case ObstacleType.Trap:
			if (_SlotIndex < _TrapSlots.Count)
			{
				return _TrapSlots[_SlotIndex] == cast.Name;
			}
			Debug.LogError(string.Concat("Looking for Slot ", _SlotIndex + 1, "/", _TrapSlots.Count, " in ", base.name, " ", cast.Defintion._Type, "s"));
			return false;
		case ObstacleType.Native:
			if (_SlotIndex < _NativeSlots.Count)
			{
				return _NativeSlots[_SlotIndex] == cast.Name;
			}
			Debug.LogError(string.Concat("Looking for Slot ", _SlotIndex + 1, "/", _NativeSlots.Count, " in ", base.name, " ", cast.Defintion._Type, "s"));
			return false;
		default:
			return false;
		}
	}
}
