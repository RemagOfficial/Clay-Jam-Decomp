using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SaveData : MonoBehaviour
{
	private static string CurrentVersion = "29.8.000";

	public bool Loaded;

	private bool _needToSave;

	public static string SavePath
	{
		get
		{
			return Application.persistentDataPath;
		}
	}

	public static SaveData Instance { get; private set; }

	public CastCollection Casts { get; private set; }

	public ClayCollection ClayCollected { get; set; }

	public HillCollection Hills { get; private set; }

	public PrizeCollection Prizes { get; private set; }

	public ProgressFlags Progress { get; private set; }

	public TutorialCollection Tutorials { get; private set; }

	public PurchasesFlags Purchases { get; private set; }

	public GameModeData GameMode { get; private set; }

	[method: MethodImpl(32)]
	public static event Action SaveEvent;

	[method: MethodImpl(32)]
	public static event Action LoadEvent;

	private void Awake()
	{
		if ((bool)Instance)
		{
			Debug.LogError("SaveData created twice", base.gameObject);
		}
		Instance = this;
		Loaded = false;
	}

	private void Start()
	{
		LoadData();
	}

	public void Save()
	{
		_needToSave = false;
		if (!BuildDetails.Instance._DemoMode)
		{
			string arg = "hills";
			string path = string.Format("{0}/{1}", SavePath, arg);
			Hashtable hashtable = new Hashtable();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("key", CurrentVersion);
			hashtable.Add("version", new Hashtable(dictionary));
			hashtable.Add("clay", ClayCollected.GetHash());
			hashtable.Add("casts", Casts.GetHash());
			hashtable.Add("hills", Hills.GetHash());
			hashtable.Add("prizes", Prizes.GetHash());
			hashtable.Add("meta", Progress.GetHash());
			hashtable.Add("tutorials", Tutorials.GetHash());
			hashtable.Add("purchases", Purchases.GetHash());
			hashtable.Add("gamemode", GameMode.GetHash());
			hashtable.Add("CurrentHill", CurrentHill.Instance.ID);
			string unencrypted = MiniJSON.jsonEncode(hashtable);
			SimplerAES simplerAES = new SimplerAES();
			string s = simplerAES.Encrypt(unencrypted);
			File.WriteAllBytes(path, Convert.FromBase64String(s));
			arg = "Clay";
			path = string.Format("{0}/{1}", SavePath, arg);
			string contents = string.Format("Last Clay Loaded From FatPebble.com= {0}", ClayCollected.Amount(0));
			File.WriteAllText(path, contents);
		}
	}

	public void WipeSave()
	{
		string arg = "hills";
		string path = string.Format("{0}/{1}", SavePath, arg);
		File.Delete(path);
	}

	public bool Load(string filename, bool adding, string versionForValidAdd)
	{
		if (!File.Exists(filename))
		{
			Debug.Log(string.Format("Path {0} not found. No data will be loaded", filename));
			return false;
		}
		Debug.Log(string.Format("Path {0} being loaded {1}", filename, (!adding) ? "as default" : "for adding"));
		byte[] inArray = File.ReadAllBytes(filename);
		string encrypted = Convert.ToBase64String(inArray);
		SimplerAES simplerAES = new SimplerAES();
		string json = simplerAES.Decrypt(encrypted);
		Hashtable hashtable = (Hashtable)MiniJSON.jsonDecode(json);
		string text = string.Empty;
		foreach (DictionaryEntry item in hashtable)
		{
			string text2 = (string)item.Key;
			if (text2 == "version")
			{
				Hashtable hashtable2 = (Hashtable)item.Value;
				text = (string)hashtable2["key"];
			}
		}
		Version version = new Version(text);
		Version version2 = new Version(CurrentVersion);
		if (text != CurrentVersion)
		{
			Debug.LogWarning(string.Format("NOT THE SAME VERSION OF SAVE, {0} instead of {1}", version.ToString(), version2.ToString()));
			if (version.Major != version2.Major)
			{
				if (!adding)
				{
					Debug.LogWarning("Losing progress");
					WipeSave();
				}
				return false;
			}
			Debug.LogWarning(string.Format("Progress will be maintained"));
		}
		if (adding && !string.IsNullOrEmpty(versionForValidAdd))
		{
			Version version3 = new Version(versionForValidAdd);
			if (version != version3)
			{
				Debug.LogWarning(string.Format("Additional data file {0} is wrong version (valid {1}, found {2}) to add progress", filename, version3.ToString(), version.ToString()));
				return false;
			}
		}
		foreach (DictionaryEntry item2 in hashtable)
		{
			switch ((string)item2.Key)
			{
			case "hills":
				Hills.Decode(item2.Value as Hashtable, version, adding);
				break;
			case "clay":
				ClayCollected.Decode(item2.Value as Hashtable, version, adding);
				break;
			case "casts":
				Casts.Decode(item2.Value as Hashtable, version, adding);
				break;
			case "prizes":
				Prizes.Decode(item2.Value as Hashtable, version, adding);
				break;
			case "meta":
				Progress.Decode(item2.Value as Hashtable, version, adding);
				break;
			case "tutorials":
				Tutorials.Decode(item2.Value as Hashtable, version, adding);
				break;
			case "purchases":
				Purchases.Decode(item2.Value as Hashtable, version, adding);
				break;
			case "gamemode":
				GameMode.Decode(item2.Value as Hashtable, version, adding);
				break;
			}
		}
		return true;
	}

	public void AddClay(ClayCollection clayCollectionToAdd)
	{
		ClayCollected.AddCollection(clayCollectionToAdd);
	}

	private void LoadData()
	{
		CreateCastData();
		CreateHillData();
		CreateClayData();
		CreatePrizeData();
		CreateProgressFlagData();
		CreateTutorialsData();
		CreatePurchasesFlagData();
		CreateGameModeData();
		if (!BuildDetails.Instance._DemoMode)
		{
			LoadMainFile();
		}
		Loaded = true;
		if (SaveData.LoadEvent != null)
		{
			SaveData.LoadEvent();
		}
	}

	private void CreateClayData()
	{
		ClayCollected = new ClayCollection(ColourDatabase.NumCollectableColours);
		ClayCollected.AddSingleColour(new ClayData(0, 5f));
	}

	private void CreateCastData()
	{
		Casts = new CastCollection();
		Casts.CreateCollectionData();
	}

	private void CreateHillData()
	{
		Hills = new HillCollection();
		Hills.CreateHillData();
	}

	private void CreatePrizeData()
	{
		Prizes = new PrizeCollection();
		Prizes.CreatePrizeData();
	}

	private void CreateProgressFlagData()
	{
		Progress = new ProgressFlags();
		Progress.SetInitialState();
	}

	private void CreatePurchasesFlagData()
	{
		Purchases = new PurchasesFlags();
		Purchases.SetInitialState();
	}

	private void CreateTutorialsData()
	{
		Tutorials = new TutorialCollection();
		Tutorials.CreateTutorialData();
	}

	private void CreateGameModeData()
	{
		GameMode = new GameModeData();
		GameMode.SetInitialState();
	}

	public void GetCastsForLevel(ObstacleCasts casts, bool ignoreLockState)
	{
		HillDefinition definition = CurrentHill.Instance.Definition;
		foreach (CastData @object in Casts.ObjectList)
		{
			if ((!@object.MouldInfo._CanBePurchased || ignoreLockState || @object.StateOnCurrentHill == LockState.Purchased) && definition.UsesCast(@object))
			{
				ObstacleCast obstacleCast = new ObstacleCast();
				obstacleCast._ColourIndex = @object.ColourIndex;
				obstacleCast.Name = @object.MouldName;
				casts.List.Add(obstacleCast);
			}
		}
	}

	public void AddClayToCollection(ClayData amountOfClay)
	{
		ClayCollected.AddSingleColour(amountOfClay);
	}

	public void MarkAsNeedToSave(bool notify)
	{
		_needToSave = true;
		if (notify && SaveData.SaveEvent != null)
		{
			SaveData.SaveEvent();
		}
	}

	public void SaveIfNeeded()
	{
		if (_needToSave)
		{
			Save();
		}
	}

	private void LoadMainFile()
	{
		string arg = "hills";
		string filename = string.Format("{0}/{1}", SavePath, arg);
		Load(filename, false, string.Empty);
	}
}
