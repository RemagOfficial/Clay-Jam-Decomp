using UnityEngine;

public class CurrentHill : MonoBehaviour
{
	private int _id;

	public string _Iain;

	public static CurrentHill Instance { get; private set; }

	public int ID
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
			RefreshValues();
		}
	}

	public HillDefinition Definition { get; private set; }

	public HillData ProgressData { get; private set; }

	public int UpgradeLevel
	{
		get
		{
			return ProgressData._UpgradeLevel;
		}
	}

	public float Length { get; private set; }

	public float FinalMomentsProgress { get; private set; }

	public float ShowHorizonProgress { get; private set; }

	public float GougeBoostZoneProgress { get; private set; }

	public float GougeCapProgress { get; private set; }

	public string LocalisedName { get; private set; }

	public float ClayPerHeart
	{
		get
		{
			float clayPerHeart = Definition._ClayPerHeart;
			float cupidBonus = SaveData.Instance.Purchases.GetCupidBonus();
			return clayPerHeart * cupidBonus;
		}
	}

	public bool ProgressIsBeyondHorizon(float progress)
	{
		return CurrentGameMode.HasBottom && progress > ShowHorizonProgress;
	}

	public bool ProgressIsBeyondGougeBoostZone(float progress)
	{
		return CurrentGameMode.HasBottom && progress > GougeBoostZoneProgress;
	}

	public bool ProgressIsBeyondGougeCap(float progress)
	{
		return CurrentGameMode.HasBottom && progress > GougeCapProgress;
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one Hill instance", base.gameObject);
		}
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public bool Upgrade(bool debugCanLoop = false)
	{
		bool flag = ProgressData.Upgrade();
		if (!flag && debugCanLoop)
		{
			ProgressData._UpgradeLevel = 0;
		}
		RefreshValues();
		return flag;
	}

	public void RefreshValues()
	{
		Definition = HillDatabase.Instance.GetDefinitionFromID(ID);
		ProgressData = SaveData.Instance.Hills.GetHillByID(ID);
		Length = ((ProgressData._UpgradeLevel < 0) ? 0f : Definition.LenfthOfUpgradeLevel(ProgressData._UpgradeLevel));
		FinalMomentsProgress = Length - HillDatabase.Instance._FinalMomentsDistance;
		ShowHorizonProgress = Length - HillDatabase.Instance._ShowHorizonDistance;
		GougeBoostZoneProgress = Length - HillDatabase.Instance._GougeBoostZoneDistance;
		GougeCapProgress = Length - HillDatabase.Instance._GougeCapDistance;
		string key = "HILL_Title_" + Definition._ID;
		LocalisedName = Localization.instance.Get(key);
	}
}
