using System;
using UnityEngine;

public class PowerupDatabase : MonoBehaviour
{
	[Serializable]
	public class PowerupSettings
	{
		public float _SquashTime;

		public float _HeavyMultiplier;

		public float _MinJacketClaySize;

		public float _MaxJacketClaySize;

		public float _JacketClayMultiplier;
	}

	public float _FlameTime = 8.01f;

	public float _FlameFalloffTime = 0.51f;

	public float _FlameAccelerationMultiplier = 1.2f;

	public float _FlameVelocityMultiplier = 1.2f;

	public float _HeavyTime = 8.25f;

	public float _SplatTime = 15f;

	public PowerupSettings[] _PowerupValues;

	public string _FlameProductID;

	public string _ShrinkProductID;

	public string _ClayBoostProductID;

	public string _SplatFingerProductID;

	private ClayData _jacketClay;

	public static PowerupDatabase Instance { get; private set; }

	public bool PowerupsAreUpgradeable
	{
		get
		{
			return BuildDetails.Instance._HasIAP;
		}
	}

	public bool IsFlameUpgraded
	{
		get
		{
			return IsPowerupUpgraded(_FlameProductID);
		}
	}

	public bool IsShrinkUpgraded
	{
		get
		{
			return IsPowerupUpgraded(_ShrinkProductID);
		}
	}

	public bool IsBoostUpgraded
	{
		get
		{
			return IsPowerupUpgraded(_ClayBoostProductID);
		}
	}

	public bool IsSplatUpgraded
	{
		get
		{
			return IsPowerupUpgraded(_SplatFingerProductID);
		}
	}

	public float _SquashTime
	{
		get
		{
			if (IsShrinkUpgraded)
			{
				return _PowerupValues[1]._SquashTime;
			}
			return _PowerupValues[0]._SquashTime;
		}
	}

	public float _HeavyMultiplier
	{
		get
		{
			if (IsFlameUpgraded)
			{
				return _PowerupValues[1]._HeavyMultiplier;
			}
			return _PowerupValues[0]._HeavyMultiplier;
		}
	}

	public float _MinJacketClaySize
	{
		get
		{
			if (IsBoostUpgraded)
			{
				return _PowerupValues[1]._MinJacketClaySize;
			}
			return _PowerupValues[0]._MinJacketClaySize;
		}
	}

	public float _MaxJacketClaySize
	{
		get
		{
			if (IsBoostUpgraded)
			{
				return _PowerupValues[1]._MaxJacketClaySize;
			}
			return _PowerupValues[0]._MaxJacketClaySize;
		}
	}

	public float _JacketClayMultiplier
	{
		get
		{
			if (IsBoostUpgraded)
			{
				return _PowerupValues[1]._JacketClayMultiplier;
			}
			return _PowerupValues[0]._JacketClayMultiplier;
		}
	}

	public int NumUpgraded
	{
		get
		{
			int num = 0;
			if (IsBoostUpgraded)
			{
				num++;
			}
			if (IsFlameUpgraded)
			{
				num++;
			}
			if (IsShrinkUpgraded)
			{
				num++;
			}
			if (IsSplatUpgraded)
			{
				num++;
			}
			return num;
		}
	}

	private bool IsPowerupUpgraded(string productID)
	{
		if (!PowerupsAreUpgradeable)
		{
			if (productID == _FlameProductID)
			{
				return false;
			}
			if (productID == _ShrinkProductID)
			{
				return true;
			}
			if (productID == _ClayBoostProductID)
			{
				return false;
			}
			if (productID == _SplatFingerProductID)
			{
				return true;
			}
			return true;
		}
		return SaveData.Instance.Purchases.IsPurchased(productID);
	}

	public ClayData GetJacketClay(float pebbleSize)
	{
		float value = pebbleSize * _JacketClayMultiplier;
		value = Mathf.Clamp(value, _MinJacketClaySize, _MaxJacketClaySize);
		_jacketClay._Amount = value;
		return _jacketClay;
	}

	public int NumPowerPlayPacks(int hillIndex)
	{
		HillDefinition defintionFromIndex = HillDatabase.Instance.GetDefintionFromIndex(hillIndex - 1);
		return defintionFromIndex._PowerPlayPacks.Count;
	}

	private void Start()
	{
		if (Instance != null)
		{
			Debug.LogError("Attempted to reinstantiate PowerupDatabase");
			return;
		}
		Instance = this;
		_jacketClay = new ClayData(0, _MinJacketClaySize);
	}

	public float CostOfPack(int packIndex, int hillID)
	{
		HillDefinition definitionFromID = HillDatabase.Instance.GetDefinitionFromID(hillID);
		if (packIndex < 0 || packIndex >= definitionFromID._PowerPlayPacks.Count)
		{
			Debug.LogError(string.Format("Requesting nonexistent powerplay pack with index", packIndex));
			return float.MaxValue;
		}
		float num = definitionFromID._PowerPlayPacks[packIndex]._Cost;
		if (!BuildDetails.Instance._HasIAP)
		{
			num *= 0.5f;
			num = Mathf.Floor(num);
		}
		return num;
	}

	public int PowerPlaysInPack(int packIndex, int hillID)
	{
		HillDefinition definitionFromID = HillDatabase.Instance.GetDefinitionFromID(hillID);
		if (packIndex < 0 || packIndex >= definitionFromID._PowerPlayPacks.Count)
		{
			Debug.LogError(string.Format("Requesting nonexistent powerplay pack with index", packIndex));
			return 0;
		}
		return definitionFromID._PowerPlayPacks[packIndex]._NumPowerplays;
	}
}
