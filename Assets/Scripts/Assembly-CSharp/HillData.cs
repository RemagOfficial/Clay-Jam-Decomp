using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class HillData
{
	public int _ID;

	public LockState _State;

	public int _UpgradeLevel;

	public int _UpgradeLevelAnimated;

	public float _BestTime;

	public float _BestScore;

	public int _PowerPlaysRemaining;

	public int _LastJVPItem;

	public int _LastSeenNumberAffordableItems;

	public int _NumPlays;

	public int _CurrentQuestIndex;

	public int _BeastDefeatedCount;

	public float _BestScoreMonsterLove;

	public bool OnLastQuest
	{
		get
		{
			int num = QuestDatabase.Instance.QuestCount(_ID);
			if (_CurrentQuestIndex >= num - 1)
			{
				return true;
			}
			return false;
		}
	}

	[method: MethodImpl(32)]
	public static event Action<int, int> HillUpgradedEvent;

	public HillData(HillDefinition definition)
	{
		_ID = definition._ID;
		_State = definition._InitialState;
		_UpgradeLevel = ((definition._InitialState == LockState.Locked) ? (-1) : 0);
		_UpgradeLevelAnimated = _UpgradeLevel;
		_LastJVPItem = 0;
		_LastSeenNumberAffordableItems = 0;
		_NumPlays = 0;
		_CurrentQuestIndex = 0;
		_BeastDefeatedCount = 0;
		_BestScoreMonsterLove = 0f;
		if (BuildDetails.Instance._DemoMode)
		{
			_CurrentQuestIndex = 6;
			_UpgradeLevel = 4;
		}
	}

	public Hashtable Encode()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		int state = (int)_State;
		dictionary.Add("State", state.ToString());
		dictionary.Add("Level", _UpgradeLevel.ToString());
		dictionary.Add("LevelAnimated", _UpgradeLevelAnimated.ToString());
		dictionary.Add("BestTime", _BestTime.ToString());
		dictionary.Add("BestScore", _BestScore.ToString());
		dictionary.Add("PowerPlays", _PowerPlaysRemaining.ToString());
		dictionary.Add("LastJVP", _LastJVPItem.ToString());
		dictionary.Add("JVPItems", _LastSeenNumberAffordableItems.ToString());
		dictionary.Add("NumPlays", _NumPlays.ToString());
		dictionary.Add("CurrentQuest", _CurrentQuestIndex.ToString());
		dictionary.Add("Beast", _BeastDefeatedCount.ToString());
		dictionary.Add("MLBest", _BestScoreMonsterLove.ToString());
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hash, Version version, bool adding)
	{
		string s = (string)hash["State"];
		LockState lockState = (LockState)int.Parse(s);
		string s2 = (string)hash["Level"];
		int num = int.Parse(s2);
		string s3 = (string)hash["LevelAnimated"];
		int upgradeLevelAnimated = int.Parse(s3);
		string s4 = (string)hash["BestTime"];
		float num2 = float.Parse(s4);
		string s5 = (string)hash["BestScore"];
		float num3 = float.Parse(s5);
		string s6 = (string)hash["PowerPlays"];
		int num4 = int.Parse(s6);
		string s7 = (string)hash["LastJVP"];
		int lastJVPItem = int.Parse(s7);
		string s8 = (string)hash["JVPItems"];
		int lastSeenNumberAffordableItems = int.Parse(s8);
		string s9 = (string)hash["NumPlays"];
		int num5 = int.Parse(s9);
		if (adding)
		{
			if (lockState > _State)
			{
				_State = lockState;
			}
			if (num > _UpgradeLevel)
			{
				_UpgradeLevel = num;
			}
			if (num2 > 0f && num2 < _BestTime)
			{
				_BestTime = num2;
			}
			if (num3 > _BestScore)
			{
				_BestScore = num3;
			}
			_PowerPlaysRemaining += num4;
			_LastJVPItem = lastJVPItem;
			_LastSeenNumberAffordableItems = lastSeenNumberAffordableItems;
			_NumPlays += num5;
		}
		else
		{
			_State = lockState;
			_UpgradeLevel = num;
			_UpgradeLevelAnimated = upgradeLevelAnimated;
			_BestTime = num2;
			_BestScore = num3;
			_PowerPlaysRemaining = num4;
			_LastJVPItem = lastJVPItem;
			_LastSeenNumberAffordableItems = lastSeenNumberAffordableItems;
			_NumPlays = num5;
		}
		if (version >= new Version("29.2.000"))
		{
			string s10 = (string)hash["CurrentQuest"];
			int num6 = int.Parse(s10);
			string s11 = (string)hash["Beast"];
			int num7 = int.Parse(s11);
			if (adding)
			{
				_CurrentQuestIndex = Mathf.Max(num6, _CurrentQuestIndex);
				_BeastDefeatedCount = Mathf.Max(num7, _BeastDefeatedCount);
			}
			else
			{
				_CurrentQuestIndex = num6;
				_BeastDefeatedCount = num7;
			}
		}
		if (hash.Contains("MLBest"))
		{
			string s12 = (string)hash["MLBest"];
			_BestScoreMonsterLove = float.Parse(s12);
		}
	}

	public override string ToString()
	{
		return string.Format("Id : {0} - State : {1} - Level {2} - Defeated {3} - Score {4} - _PowerPlays {5} ", _ID, _State, _UpgradeLevel, _BeastDefeatedCount, _BestScore, _PowerPlaysRemaining);
	}

	public float CostOfUpgrade()
	{
		if (_State == LockState.Purchased)
		{
			return float.MaxValue;
		}
		HillDefinition definitionFromID = HillDatabase.Instance.GetDefinitionFromID(_ID);
		int upgradeLevel = _UpgradeLevel + 1;
		return definitionFromID.CostOfUpgradeLevel(upgradeLevel);
	}

	public bool CanAffordUpgrade()
	{
		if (_State == LockState.Purchased)
		{
			return false;
		}
		ClayCollection clayCollected = SaveData.Instance.ClayCollected;
		return clayCollected.CanSubtract(0, CostOfUpgrade());
	}

	public bool Upgrade(bool forFree = false)
	{
		if (_State == LockState.Purchased)
		{
			return false;
		}
		if (!forFree && !CanAffordUpgrade())
		{
			return false;
		}
		if (!forFree)
		{
			ClayCollection clayCollected = SaveData.Instance.ClayCollected;
			clayCollected.Subtract(0, CostOfUpgrade());
		}
		if (_State == LockState.Locked)
		{
			if (_UpgradeLevel != -1)
			{
				Debug.LogError(string.Format("Hill {0} is locked and not at upgrade level -1 (its {1})", _ID, _UpgradeLevel));
			}
			_UpgradeLevel = 0;
			_State = LockState.Unlocked;
		}
		else
		{
			_UpgradeLevel++;
			if (HillData.HillUpgradedEvent != null)
			{
				HillData.HillUpgradedEvent(_ID, _UpgradeLevel);
			}
		}
		HillDefinition definitionFromID = HillDatabase.Instance.GetDefinitionFromID(_ID);
		int maxUpgradeLevel = definitionFromID.MaxUpgradeLevel;
		if (_UpgradeLevel > maxUpgradeLevel)
		{
			Debug.LogError("Hill was upgraded one too many times without being marked as purchsed");
			_UpgradeLevel = maxUpgradeLevel;
			_State = LockState.Purchased;
			return false;
		}
		if (_UpgradeLevel == maxUpgradeLevel)
		{
			_State = LockState.Purchased;
		}
		UnlockCreaturesForCurrentUpgrade();
		return true;
	}

	public float CostOfPowerPack(int packIndex)
	{
		return PowerupDatabase.Instance.CostOfPack(packIndex, _ID);
	}

	public bool CanAffordPowerplayPack(int packIndex)
	{
		ClayCollection clayCollected = SaveData.Instance.ClayCollected;
		return clayCollected.CanSubtract(0, CostOfPowerPack(packIndex));
	}

	public bool PurchasePowerplays(int packIndex, bool forFree = false)
	{
		if (!forFree && !CanAffordPowerplayPack(packIndex))
		{
			return false;
		}
		if (!forFree)
		{
			ClayCollection clayCollected = SaveData.Instance.ClayCollected;
			clayCollected.Subtract(0, CostOfPowerPack(packIndex));
		}
		_PowerPlaysRemaining += PowerupDatabase.Instance.PowerPlaysInPack(packIndex, _ID);
		return true;
	}

	public void PowerPlayUsed()
	{
		if (_PowerPlaysRemaining > 0)
		{
			_PowerPlaysRemaining--;
		}
		else
		{
			Debug.Log("Using up non existent powerplay");
		}
	}

	private void UnlockCreaturesForCurrentUpgrade()
	{
		HillDefinition definitionFromID = HillDatabase.Instance.GetDefinitionFromID(_ID);
		string creatureUnlock = definitionFromID._UpgradeLevels[_UpgradeLevel]._CreatureUnlock;
		SaveData.Instance.Casts.UnlockCastsForMould(creatureUnlock);
	}

	public bool HasSomethingNewInShop()
	{
		if (_State != LockState.Unlocked)
		{
			return false;
		}
		return JVPController.Instance.NumAffordableItems > _LastSeenNumberAffordableItems;
	}

	private static string ItemKeyFromIndex(int index)
	{
		return string.Format("jvpItem{0}", index);
	}

	public bool Completed()
	{
		if (_State != LockState.Purchased)
		{
			return false;
		}
		HillDefinition definitionFromID = HillDatabase.Instance.GetDefinitionFromID(_ID);
		foreach (CastId creatureSlot in definitionFromID._CreatureSlots)
		{
			if (SaveData.Instance.Casts.GetCast(creatureSlot).GetStateOnHill(_ID) != LockState.Purchased)
			{
				return false;
			}
		}
		return true;
	}

	public bool OnQuestCompleted()
	{
		_CurrentQuestIndex++;
		int num = QuestDatabase.Instance.QuestCount(_ID);
		if (_CurrentQuestIndex >= num)
		{
			OnAllQuestsCompleted();
			return true;
		}
		return false;
	}

	private void OnAllQuestsCompleted()
	{
		_CurrentQuestIndex = 0;
		_BeastDefeatedCount++;
	}

	public void ResetQuests()
	{
		_CurrentQuestIndex = 0;
		_BeastDefeatedCount = 0;
	}

	public string FullQuestIDString()
	{
		return string.Format("Quest_Hill{0}_Iteration{1}_Quest{2}", _ID, _BeastDefeatedCount, (!SaveData.Instance.Progress._QuestsStarted.Set) ? (-1) : _CurrentQuestIndex);
	}
}
