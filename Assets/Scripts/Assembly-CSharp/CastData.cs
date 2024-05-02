using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[Serializable]
public class CastData
{
	public Dictionary<int, LockState> States = new Dictionary<int, LockState>();

	public CastId Id { get; private set; }

	public string MouldName
	{
		get
		{
			return Id.MouldName;
		}
	}

	public int ColourIndex
	{
		get
		{
			return Id.ColourIndex;
		}
	}

	public LockState StateOnCurrentHill
	{
		get
		{
			return States[CurrentHill.Instance.ID];
		}
		set
		{
			States[CurrentHill.Instance.ID] = value;
			CheckHillCompleted(CurrentHill.Instance.ID);
		}
	}

	public ObstacleDefinition MouldInfo { get; set; }

	public float Cost
	{
		get
		{
			return MouldInfo.CostForColour(ColourIndex);
		}
	}

	public string IconName
	{
		get
		{
			if (MouldInfo._IconTextureName == string.Empty)
			{
				return MouldName;
			}
			return MouldInfo._IconTextureName;
		}
	}

	public HSVColour Colour
	{
		get
		{
			return MouldInfo.GetColour(ColourIndex);
		}
	}

	[method: MethodImpl(32)]
	public static event Action CastPurchaseCompletesHillEvent;

	[method: MethodImpl(32)]
	public static event Action CastPurchaseCompletesAllHillsEvent;

	public CastData()
	{
		Id = new CastId();
	}

	public CastData(string mouldName, int colourIndex, LockState[] States)
	{
		Id = new CastId(mouldName, colourIndex);
		ArrayToDictionary(States);
	}

	public LockState GetStateOnHill(int hillId)
	{
		return States[hillId];
	}

	private void ArrayToDictionary(LockState[] states)
	{
		for (int i = 0; i < states.Length; i++)
		{
			States.Add(i + 1, states[i]);
		}
	}

	public Hashtable Encode()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Name", MouldName);
		dictionary.Add("Index", ColourIndex.ToString());
		dictionary.Add("States", StatesToString());
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hashtable)
	{
		States = new Dictionary<int, LockState>();
		string s = (string)hashtable["Index"];
		string mouldName = (string)hashtable["Name"];
		Id.ColourIndex = int.Parse(s);
		Id.MouldName = mouldName;
		StringToStates((string)hashtable["States"]);
	}

	public string StatesToString()
	{
		string text = string.Empty;
		foreach (KeyValuePair<int, LockState> state in States)
		{
			text += string.Format("{0}|{1}|", state.Key, state.Value);
		}
		return text;
	}

	private void StringToStates(string statelist)
	{
		if (statelist == null)
		{
			return;
		}
		string[] array = statelist.Split('|');
		for (int i = 0; i < array.Length - 1; i += 2)
		{
			if (!States.ContainsKey(int.Parse(array[i])))
			{
				States.Add(int.Parse(array[i]), GetLockStateFromString(array[i + 1]));
			}
		}
	}

	public LockState GetLockStateFromString(string lockstate)
	{
		LockState result = LockState.Locked;
		if (lockstate.ToString().Equals("Unlocked"))
		{
			result = LockState.Unlocked;
		}
		else if (lockstate.ToString().Equals("Purchased"))
		{
			result = LockState.Purchased;
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("Name : {0} - ColourIndex : {1} - State : {2} - Cost {3}", MouldName, ColourIndex, StateOnCurrentHill, Cost);
	}

	public void SetStateForAllHills(LockState lockState)
	{
		for (int i = 0; i < HillDatabase.NumHills; i++)
		{
			int iD = HillDatabase.Instance.GetDefintionFromIndex(i)._ID;
			States[iD] = lockState;
		}
	}

	public void CopyStates(CastData otherCast, bool adding)
	{
		for (int i = 0; i < HillDatabase.NumHills; i++)
		{
			int iD = HillDatabase.Instance.GetDefintionFromIndex(i)._ID;
			if (adding)
			{
				if (States[iD] < otherCast.States[iD])
				{
					States[iD] = otherCast.States[iD];
				}
			}
			else
			{
				States[iD] = otherCast.States[iD];
			}
		}
	}

	private void CheckHillCompleted(int hillID)
	{
		HillCollection hills = SaveData.Instance.Hills;
		HillData hillByID = hills.GetHillByID(hillID);
		if (!hillByID.Completed())
		{
			return;
		}
		bool flag = true;
		foreach (HillData hill in hills.Hills)
		{
			if (!hill.Completed())
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (CastData.CastPurchaseCompletesAllHillsEvent != null)
			{
				CastData.CastPurchaseCompletesAllHillsEvent();
			}
			foreach (HillData hill2 in hills.Hills)
			{
				hill2._PowerPlaysRemaining++;
			}
		}
		else
		{
			if (CastData.CastPurchaseCompletesHillEvent != null)
			{
				CastData.CastPurchaseCompletesHillEvent();
			}
			hillByID._PowerPlaysRemaining++;
		}
		InGameAudio.PostFabricEvent("HillComplete");
	}
}
