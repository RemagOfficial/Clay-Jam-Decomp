using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HatBrimItem : IComparable<HatBrimItem>
{
	public enum ItemType
	{
		None = 0,
		Cast = 1,
		Hill = 2,
		PowerPlay = 3
	}

	public ItemType Type { get; set; }

	public Texture2D Texture { get; set; }

	public string Name { get; set; }

	public string Info
	{
		get
		{
			if (!CanAfford && LockState == LockState.Unlocked)
			{
				return Localization.instance.GetFor3DText("JVP_NotEnoughClay");
			}
			switch (Type)
			{
			case ItemType.Cast:
			{
				string param = Name.Replace("\n", " ");
				if (CastData.StateOnCurrentHill == LockState.Locked)
				{
					return Localization.instance.GetFor3DText("JVP_Blackboard_UnlockMonster", CurrentHill.Instance.LocalisedName);
				}
				if (CastData.StateOnCurrentHill == LockState.Purchased)
				{
					return Localization.instance.GetFor3DText("JVP_Blackboard_MonsterOwnedNew", param);
				}
				return Localization.instance.GetFor3DText("JVP_Blackboard_Mould", param, CurrentHill.Instance.LocalisedName);
			}
			case ItemType.Hill:
				if (CurrentHill.Instance.UpgradeLevel == CurrentHill.Instance.Definition.MaxUpgradeLevel)
				{
					return Localization.instance.GetFor3DText("JVP_Blackboard_MaxUpgrade", CurrentHill.Instance.LocalisedName);
				}
				if (UpgradeLevel < 1)
				{
					return Localization.instance.GetFor3DText("JVP_Blackboard_UnlockHill", CurrentHill.Instance.LocalisedName);
				}
				return Localization.instance.GetFor3DText("JVP_Blackboard_ExpandHill", CurrentHill.Instance.LocalisedName, UpgradeLevel + 1, CurrentHill.Instance.Definition.MaxUpgradeLevel + 1);
			case ItemType.PowerPlay:
			{
				int num = PowerupDatabase.Instance.PowerPlaysInPack(PowerPlayPackIndex, CurrentHill.Instance.ID);
				if (num > 1)
				{
					return Localization.instance.GetFor3DText("JVP_Blackboard_PowerPlays", num, CurrentHill.Instance.LocalisedName);
				}
				return Localization.instance.GetFor3DText("JVP_Blackboard_PowerPlay", CurrentHill.Instance.LocalisedName);
			}
			default:
				return string.Empty;
			}
		}
	}

	public string UniqueName
	{
		get
		{
			switch (Type)
			{
			case ItemType.Cast:
				return string.Format("Creature_{0}_{1}", CastData.MouldName, CastData.ColourIndex);
			case ItemType.Hill:
				return string.Format("Hill_{0}_{1}", CurrentHill.Instance.ID, UpgradeLevel);
			case ItemType.PowerPlay:
				return string.Format("Hill_{0}_PowerPlay_{1}", CurrentHill.Instance.ID, PowerPlayPackIndex);
			default:
				return "unknown";
			}
		}
	}

	public LockState LockState
	{
		get
		{
			switch (Type)
			{
			case ItemType.Hill:
			{
				int upgradeLevel = CurrentHill.Instance.ProgressData._UpgradeLevel;
				LockState result = LockState.Locked;
				if (upgradeLevel >= UpgradeLevel)
				{
					result = LockState.Purchased;
				}
				else if (upgradeLevel == UpgradeLevel - 1)
				{
					result = LockState.Unlocked;
				}
				return result;
			}
			case ItemType.Cast:
				return CastData.StateOnCurrentHill;
			case ItemType.PowerPlay:
				if (CurrentHill.Instance.ProgressData._State == LockState.Locked)
				{
					return LockState.Locked;
				}
				return LockState.Unlocked;
			default:
				return LockState.Unlocked;
			}
		}
	}

	public CastData CastData { get; private set; }

	public int PowerPlayPackIndex { get; private set; }

	public int UpgradeLevel { get; private set; }

	public float Cost
	{
		get
		{
			switch (Type)
			{
			case ItemType.Cast:
				return CastData.Cost;
			case ItemType.Hill:
				return CurrentHill.Instance.Definition.CostOfUpgradeLevel(UpgradeLevel);
			case ItemType.PowerPlay:
				return PowerupDatabase.Instance.CostOfPack(PowerPlayPackIndex, CurrentHill.Instance.ID);
			default:
				return -1f;
			}
		}
	}

	public bool IsPurchased
	{
		get
		{
			return LockState == LockState.Purchased;
		}
	}

	public bool CanAfford
	{
		get
		{
			return SaveData.Instance.ClayCollected.CanSubtract(0, Cost);
		}
	}

	public HatBrimItem(ItemType type)
	{
		Type = type;
	}

	public void AddCastData(CastData cast)
	{
		if (Type != ItemType.Cast)
		{
			Debug.LogError("Bad hat brim item coding bum face!");
		}
		else
		{
			CastData = cast;
		}
	}

	public void AddHillData(int upgradeLevel)
	{
		if (Type != ItemType.Hill)
		{
			Debug.LogError("Bad hat brim item coding, bum face!");
		}
		else
		{
			UpgradeLevel = upgradeLevel;
		}
	}

	public void AddPowerPlayData(int powerPlayPackIndex)
	{
		if (Type != ItemType.PowerPlay)
		{
			Debug.LogError("Bad hat brim item coding, bum face!");
		}
		else
		{
			PowerPlayPackIndex = powerPlayPackIndex;
		}
	}

	public int CompareTo(HatBrimItem other)
	{
		if (Type == ItemType.PowerPlay && other.Type != ItemType.PowerPlay)
		{
			return 1;
		}
		if (Type != ItemType.PowerPlay && other.Type == ItemType.PowerPlay)
		{
			return -1;
		}
		if (Type == ItemType.PowerPlay && other.Type == ItemType.PowerPlay)
		{
			return PowerPlayPackIndex.CompareTo(other.PowerPlayPackIndex) * -1;
		}
		if (Type == ItemType.Hill && other.Type == ItemType.Hill)
		{
			return UpgradeLevel.CompareTo(other.UpgradeLevel);
		}
		if (Type == ItemType.Hill && other.Type == ItemType.Cast)
		{
			return CompareHillAndCast(this, other);
		}
		if (Type == ItemType.Cast && other.Type == ItemType.Hill)
		{
			return CompareHillAndCast(other, this) * -1;
		}
		return CompareCasts(this, other);
	}

	private int CompareHillAndCast(HatBrimItem hill, HatBrimItem cast)
	{
		int num = hill.UpgradeLevel.CompareTo(FindCastUpgradeLevel(cast));
		return (num != 0) ? num : (-1);
	}

	private int CompareCasts(HatBrimItem subjectCast, HatBrimItem otherCast)
	{
		int num = FindCastUpgradeLevel(subjectCast).CompareTo(FindCastUpgradeLevel(otherCast));
		if (num == 0)
		{
			num = subjectCast.CastData.Cost.CompareTo(otherCast.CastData.Cost);
		}
		return (num == 0) ? subjectCast.CastData.ColourIndex.CompareTo(otherCast.CastData.ColourIndex) : num;
	}

	private int FindCastUpgradeLevel(HatBrimItem subjectCast)
	{
		List<HillDefinition.UpgradeData> upgradeLevels = CurrentHill.Instance.Definition._UpgradeLevels;
		int result = 0;
		for (int i = 0; i < upgradeLevels.Count; i++)
		{
			if (upgradeLevels[i]._CreatureUnlock.Equals(subjectCast.CastData.MouldName, StringComparison.OrdinalIgnoreCase))
			{
				result = i;
				i = upgradeLevels.Count;
			}
		}
		return result;
	}

	public bool ShouldBeDisplayed()
	{
		switch (Type)
		{
		case ItemType.Cast:
			return CurrentHill.Instance.Definition.UsesCast(CastData);
		case ItemType.Hill:
			return true;
		case ItemType.PowerPlay:
			return true;
		default:
			return false;
		}
	}
}
