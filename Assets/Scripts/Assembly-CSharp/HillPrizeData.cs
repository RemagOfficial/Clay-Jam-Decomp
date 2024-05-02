using System.Collections;
using System.Collections.Generic;

public class HillPrizeData
{
	public int _HillID;

	public int _PrizeIndex;

	public int _MetersFlownSinceLastPrize;

	private ProceduralPrize _proceduralPrize;

	private HillPrizeDefintions _database;

	public PrizeDefinition NextPrize { get; private set; }

	public HillPrizeData(int hillID)
	{
		InitialiseFields();
		SetHillID(hillID);
		SetNextPrize();
		HillData.HillUpgradedEvent += OnHillUpgraded;
	}

	public void InitialiseFields()
	{
		_HillID = -1;
		_PrizeIndex = 0;
		_proceduralPrize = new ProceduralPrize();
		NextPrize = null;
		_database = null;
	}

	~HillPrizeData()
	{
		HillData.HillUpgradedEvent -= OnHillUpgraded;
	}

	public void SetHillID(int hillID)
	{
		_HillID = hillID;
		_proceduralPrize.SetHillID(_HillID);
		_database = PrizeDatabase.Instance.PrizesForHill(_HillID);
	}

	public void AwardPrize()
	{
		NextPrize.AwardPrize();
		_PrizeIndex++;
		_MetersFlownSinceLastPrize = 0;
		SetNextPrize();
	}

	public void AddMetersTowardsCurrentPrize()
	{
		if (NextPrize != null)
		{
			_MetersFlownSinceLastPrize++;
		}
	}

	public bool PrizeShouldBeAwarded()
	{
		if (NextPrize == null)
		{
			return false;
		}
		return _MetersFlownSinceLastPrize >= NextPrize.Beans;
	}

	public int MetersToNextPrize()
	{
		if (NextPrize == null)
		{
			return -1;
		}
		return NextPrize.Beans - _MetersFlownSinceLastPrize;
	}

	private bool RunOutOfDatabasePrizes()
	{
		return _database.NumPrizes <= _PrizeIndex;
	}

	private void SetNextPrize()
	{
		if (RunOutOfDatabasePrizes())
		{
			_proceduralPrize.Generate(_PrizeIndex);
			NextPrize = _proceduralPrize.Definition;
			return;
		}
		NextPrize = _database.GetPrize(_PrizeIndex);
		HillData hillByID = SaveData.Instance.Hills.GetHillByID(_HillID);
		if (hillByID._UpgradeLevel < NextPrize._HillUpgrade)
		{
			NextPrize = null;
		}
	}

	private void OnHillUpgraded(int hill, int upgradeLevel)
	{
		if (hill == _HillID)
		{
			SetNextPrize();
		}
	}

	public Hashtable Encode()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("HillID", _HillID.ToString());
		dictionary.Add("PrizeIndex", _PrizeIndex.ToString());
		dictionary.Add("MetersFlownSincePrize", _MetersFlownSinceLastPrize.ToString());
		return new Hashtable(dictionary);
	}

	public void Decode(Hashtable hashtable, bool adding)
	{
		string s = (string)hashtable["PrizeIndex"];
		int num = int.Parse(s);
		string s2 = (string)hashtable["MetersFlownSincePrize"];
		int metersFlownSinceLastPrize = int.Parse(s2);
		if (adding)
		{
			if (num < _PrizeIndex)
			{
				_PrizeIndex = num;
				_MetersFlownSinceLastPrize = metersFlownSinceLastPrize;
			}
		}
		else
		{
			_PrizeIndex = num;
			_MetersFlownSinceLastPrize = metersFlownSinceLastPrize;
		}
		SetNextPrize();
	}

	public override string ToString()
	{
		return string.Format("HillID : {0} - PrizeIndex : {1}", _HillID, _PrizeIndex);
	}
}
