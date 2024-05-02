using System;
using UnityEngine;

[Serializable]
public class PrizeDefinitionData
{
	public PrizeType _Type;

	public string _Name;

	public int _ID;

	public float _Value;

	public void Award()
	{
		switch (_Type)
		{
		case PrizeType.Jigger:
			AwardJVP();
			break;
		case PrizeType.Clay:
			AwardClay();
			break;
		case PrizeType.Hill:
			Debug.LogError("HILL PRIZE DEPRECATED");
			break;
		case PrizeType.Mould:
			AwardMould();
			break;
		case PrizeType.PowerupMould:
			Debug.LogError("POWER UP MOULD DEPRECATED");
			break;
		case PrizeType.PowerPlay:
			AwardPowerPlay();
			break;
		case PrizeType.HillUpgrade:
			Debug.LogError("HILL UPGRADE DEPRECATED");
			break;
		default:
			Debug.LogError(string.Format("Unhandled prize {0}", _Type));
			break;
		}
	}

	private bool AwardMould()
	{
		string name = _Name;
		bool flag = SaveData.Instance.Casts.UnlockCastsForMould(name);
		Debug.Log(string.Format("Attempt to unlock Casts with MouldName {0}, {1}", name, (!flag) ? "Failed" : "Succeeded"));
		return flag;
	}

	private bool AwardPowerPlay()
	{
		int iD = _ID;
		int num = 0;
		SaveData.Instance.Hills.GetHillByID(iD).PurchasePowerplays(num, true);
		Debug.Log(string.Format("Awarded power play pack index {0} on hill {1}", num, iD));
		return true;
	}

	private void AwardClay()
	{
		ClayData clayToAdd = new ClayData(0, _Value);
		SaveData.Instance.ClayCollected.AddSingleColour(clayToAdd);
		Debug.Log(string.Format("Clay awarded {0}", GetDebugString()));
	}

	private void AwardJVP()
	{
		if (SaveData.Instance.Progress.UnlockJVP())
		{
			Debug.Log(string.Format("JVP awarded"));
		}
		else
		{
			Debug.Log(string.Format("JVP award failed - already unlocked"));
		}
	}

	public string GetDebugString()
	{
		string empty = string.Empty;
		switch (_Type)
		{
		case PrizeType.Mould:
			empty = string.Format("Mould {0} unlocked", _Name);
			break;
		case PrizeType.Clay:
			empty = string.Format("{0} of Clay Colour {1}", _Value, _ID);
			break;
		case PrizeType.PowerPlay:
			empty = string.Format("PowerPlay for hill {0}", _ID);
			break;
		case PrizeType.PowerupMould:
			empty = "DEPRECATED";
			break;
		case PrizeType.Jigger:
			empty = string.Format(string.Empty);
			break;
		case PrizeType.Hill:
			empty = "DEPRECATED";
			break;
		case PrizeType.HillUpgrade:
			empty = "DEPRECATED";
			break;
		default:
			empty = "ERROR - UNKNOWN PRIZE TYPE";
			break;
		}
		return string.Format("Type - {0}, Data - {1}", _Type, empty);
	}
}
