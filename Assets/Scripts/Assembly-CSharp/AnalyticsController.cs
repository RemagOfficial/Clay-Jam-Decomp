using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AnalyticsController : MonoBehaviour
{
	[Serializable]
	public class Config
	{
		public bool _UseGoogle;

		public UnityEngine.Object _GooglePrefab;

		public bool _UseFlurry;

		public string _FlurryAppID = string.Empty;
	}

	public enum AnalyticsType
	{
		GOOGLE = 0,
		FLURRY = 1,
		ALL = 2
	}

	private const string CategoryGameEvent = "GameEvent";

	private const string CategoryRunCompletedClay = "RunCompletedClay";

	private const string CategoryRunCompletedDistance = "RunCompletedDistance";

	private const string CategoryRunCompletedHearts = "RunCompletedHearts";

	private const string CategoryBurstly = "BurstlyResponse";

	private const string CategoryItemPurchased = "ItemPurchased";

	private const string CategoryIAPProducts = "IAPProducts";

	private const string CategoryQuest = "QuestComplete";

	private const string CategoryQuestFail = "QuestFail";

	private const string CategoryPowerUps = "PowerUps";

	private const string CategoryEndSession = "EndSession";

	private const string CategoryAwardPowerplay = "AwardPowerPlay";

	private const string CategoryFreeClay = "FreeClay";

	public string _ClayJamSubPage = "ClayJam";

	private float _StartTime;

	private float _EndTime;

	private static bool _IAPPurchaseJustCompleted;

	public static AnalyticsController Instance { get; set; }

	private bool UseGoogle
	{
		get
		{
			return BuildDetails.Instance._Analytics._UseGoogle;
		}
	}

	private bool UseFlurry
	{
		get
		{
			return BuildDetails.Instance._Analytics._UseFlurry;
		}
	}

	private string CategoryIAPPurchaseRequest
	{
		get
		{
			return string.Format("{0}{1}", "IAPPurchaseRequest", (!StaticIAPItems.Instance.InRestoreMode) ? string.Empty : "_R");
		}
	}

	private string CategoryIAPPurchaseFailed
	{
		get
		{
			return string.Format("{0}{1}", "IAPPurchaseFailed", (!StaticIAPItems.Instance.InRestoreMode) ? string.Empty : "_R");
		}
	}

	private string CategoryIAPCancelled
	{
		get
		{
			return string.Format("{0}{1}", "IAPCancelled", (!StaticIAPItems.Instance.InRestoreMode) ? string.Empty : "_R");
		}
	}

	private string CategoryIAPPurchaseComplete
	{
		get
		{
			return string.Format("{0}{1}", "IAPPurchaseComplete", (!StaticIAPItems.Instance.InRestoreMode) ? string.Empty : "_R");
		}
	}

	private string ModeDetails
	{
		get
		{
			if (CurrentGameMode.Type == GameModeType.MonsterLove)
			{
				return string.Format("ML_H{0}", CurrentHill.Instance.ID);
			}
			return string.Format("PA_{0}", CurrentHill.Instance.ProgressData.FullQuestIDString());
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one AnalyticsController instance");
		}
		Instance = this;
		MetaGameController.GameLoadedEvent += OnGameLoaded;
		InGameController.RunCompletedSuccess += DoRunCompleteAnalytic;
		JVPController.SuccessfulPurchaseItemEvent += DoPurchaseItemAnalytic;
		BurstlyInstallTracker.BurstlyEvent += DoBurstlyAnalytic;
		CurrentQuest.QuestCompleteEvent += DoQuestCompleteAnalytic;
		CurrentQuest.QuestFailedEvent += DoQuestFailAnalytic;
		PrizeGUIController.AwardedPowerPlayEvent += DoAwardPowerplayAnalytic;
		PowerUpPickerGUIController.InGamePoweplayBought += DoBuyPowerplayInGameAnalytic;
		IAPProductRequester.IAPProductsFailed += DoIAPProductsFailedAnalytic;
		IAPPanelController.IAPProductsSeen += DoIAPProductsSeenAnalytic;
		IAPPanelController.IAPPurchaseRequest += DoIAPPurchaseRequestAnalytic;
		IAPPanelController.IAPPurchaseFailed += DoIAPPurchaseFailedAnalytic;
		IAPPanelController.IAPPurchaseCancelled += DoIAPPurchaseCancelledAnalytic;
		IAPPanelController.IAPPurchaseComplete += DoIAPPurchaseCompleteAnalytic;
		RewardedAds.FreeClayPressedEvent = (Action)Delegate.Combine(RewardedAds.FreeClayPressedEvent, new Action(DoFreeClayPressedAnalytic));
		RewardedAds.FreeClayAwardedEvent = (Action<int>)Delegate.Combine(RewardedAds.FreeClayAwardedEvent, new Action<int>(DoFreeClayAwardedAnalytic));
		RewardedAds.FreeClayErrorEvent = (Action<string>)Delegate.Combine(RewardedAds.FreeClayErrorEvent, new Action<string>(DoFreeClayErrorAnalytic));
		_IAPPurchaseJustCompleted = false;
	}

	private void Start()
	{
		_StartTime = Time.time;
		if (UseGoogle)
		{
			StartGoogle();
		}
		if (UseFlurry)
		{
			StartFlurry();
		}
	}

	private void StartGoogle()
	{
		UnityEngine.Object.Instantiate(BuildDetails.Instance._Analytics._GooglePrefab);
	}

	private void StartFlurry()
	{
		if (!string.IsNullOrEmpty(BuildDetails.Instance._Analytics._FlurryAppID))
		{
			FlurryAndroid.onStartSession(BuildDetails.Instance._Analytics._FlurryAppID, BuildDetails.Instance._HasRewardedAds, false, false);
		}
	}

	private void EndFlurry()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			FlurryAndroid.onEndSession();
		}
	}

	private void OnDestroy()
	{
		if (UseFlurry)
		{
			EndFlurry();
		}
		MetaGameController.GameLoadedEvent -= OnGameLoaded;
		InGameController.RunCompletedSuccess -= DoRunCompleteAnalytic;
		JVPController.SuccessfulPurchaseItemEvent -= DoPurchaseItemAnalytic;
		BurstlyInstallTracker.BurstlyEvent -= DoBurstlyAnalytic;
		CurrentQuest.QuestCompleteEvent -= DoQuestCompleteAnalytic;
		CurrentQuest.QuestFailedEvent -= DoQuestFailAnalytic;
		PrizeGUIController.AwardedPowerPlayEvent -= DoAwardPowerplayAnalytic;
		PowerUpPickerGUIController.InGamePoweplayBought -= DoBuyPowerplayInGameAnalytic;
		IAPProductRequester.IAPProductsFailed -= DoIAPProductsFailedAnalytic;
		IAPPanelController.IAPProductsSeen -= DoIAPProductsSeenAnalytic;
		IAPPanelController.IAPPurchaseRequest -= DoIAPPurchaseRequestAnalytic;
		IAPPanelController.IAPPurchaseFailed -= DoIAPPurchaseFailedAnalytic;
		IAPPanelController.IAPPurchaseCancelled -= DoIAPPurchaseCancelledAnalytic;
		IAPPanelController.IAPPurchaseComplete -= DoIAPPurchaseCompleteAnalytic;
		RewardedAds.FreeClayPressedEvent = (Action)Delegate.Remove(RewardedAds.FreeClayPressedEvent, new Action(DoFreeClayPressedAnalytic));
		RewardedAds.FreeClayAwardedEvent = (Action<int>)Delegate.Remove(RewardedAds.FreeClayAwardedEvent, new Action<int>(DoFreeClayAwardedAnalytic));
		RewardedAds.FreeClayErrorEvent = (Action<string>)Delegate.Remove(RewardedAds.FreeClayErrorEvent, new Action<string>(DoFreeClayErrorAnalytic));
	}

	private void OnGameLoaded()
	{
		if (!SaveData.Instance.Progress._HasBeenLoaded.Set)
		{
			DoGameLoadedFirstTimeAnalytic();
			SaveData.Instance.Progress._HasBeenLoaded.Set = true;
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			_EndTime = Time.time;
			DoEndSessionAnalytic();
		}
		else
		{
			_StartTime = Time.time;
		}
	}

	private void DoEndSessionAnalytic()
	{
		if (CurrentHill.Instance != null)
		{
			string analyticDescription = string.Format("StoppedOn{0}_", CurrentHill.Instance.ProgressData.FullQuestIDString());
			if (UseGoogle)
			{
				SendGoogleAnalytic("EndSession", analyticDescription, "PlayTime", Mathf.FloorToInt(_EndTime - _StartTime));
			}
		}
	}

	private void DoGameLoadedFirstTimeAnalytic()
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("GameEvent", "FirstTimeLoad", BuildDetails.Instance.Description);
		}
		if (UseFlurry)
		{
			SendFlurryAnalytic("FirstTimeLoad");
		}
	}

	private void DoRunCompleteAnalytic()
	{
		if (UseGoogle)
		{
			DoRunCompleteAnalytic_Google();
		}
		if (UseFlurry)
		{
			DoRunCompleteAnalytic_Flurry();
		}
	}

	private void DoRunCompleteAnalytic_Google()
	{
		string analyticDescription = string.Format("{0}ClayWon_Hill_{1}_{2}", (CurrentGameMode.Type != GameModeType.MonsterLove) ? string.Empty : "ML_", CurrentHill.Instance.ID, CurrentHill.Instance.UpgradeLevel);
		string stat = "ClayCollected";
		int statValue = (int)InGameController.Instance.ClayCollectedThisRun.Amount(0);
		SendGoogleAnalytic("RunCompletedClay", analyticDescription, stat, statValue);
		analyticDescription = string.Format("{0}Flown_Hill_{1}_{2}", (CurrentGameMode.Type != GameModeType.MonsterLove) ? string.Empty : "ML_", CurrentHill.Instance.ID, CurrentHill.Instance.UpgradeLevel);
		stat = "Distance";
		statValue = ((CurrentGameMode.Type != GameModeType.MonsterLove) ? InGameController.Instance.MetersFlown : Mathf.CeilToInt(Pebble.Instance.MaxProgress));
		SendGoogleAnalytic("RunCompletedDistance", analyticDescription, stat, statValue);
	}

	private void DoRunCompleteAnalytic_Flurry()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string value = string.Format("{0}_{1}", CurrentHill.Instance.ID, CurrentHill.Instance.UpgradeLevel);
		dictionary.Add("Level", value);
		dictionary.Add("Mode", ModeDetails);
		dictionary.Add("ClayWon", ClayAmountGroup(InGameController.Instance.ClayCollectedThisRun.Amount(0)));
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			dictionary.Add("Distance", "0");
			dictionary.Add("DistanceML", DistanceGroup(Pebble.Instance.MaxProgress));
		}
		else
		{
			dictionary.Add("Distance", DistanceGroup(InGameController.Instance.MetersFlown));
			dictionary.Add("DistanceML", "0");
		}
		SendFlurryAnalytic("RunCompleted", dictionary);
	}

	private void DoPurchaseItemAnalytic(HatBrimItem item)
	{
		string uniqueName = item.UniqueName;
		string analyticDescription = string.Format("Purchased_{0}", uniqueName);
		if (UseGoogle)
		{
			SendGoogleAnalytic("ItemPurchased", analyticDescription);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Item", uniqueName);
			SendFlurryAnalytic("ItemPurchased", dictionary);
		}
		DoPurchaseAfterIAPAnalytic(uniqueName);
	}

	private void DoPurchaseAfterIAPAnalytic(string item)
	{
		if (_IAPPurchaseJustCompleted)
		{
			string analyticDescription = string.Format("PostIAPItem_{0}", item);
			if (UseGoogle)
			{
				SendGoogleAnalytic("PostIAPItem", analyticDescription);
			}
			if (UseFlurry)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("Item", item);
				SendFlurryAnalytic("PostIAPItem", dictionary);
			}
			_IAPPurchaseJustCompleted = false;
		}
	}

	private void DoBurstlyAnalytic(string burstlyResponse)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("BurstlyResponse", burstlyResponse);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Response", burstlyResponse);
			SendFlurryAnalytic("BurstlyResponse", dictionary);
		}
	}

	private void DoQuestCompleteAnalytic(string questID)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("QuestComplete", questID);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Quest", questID);
			SendFlurryAnalytic("QuestComplete", dictionary);
		}
	}

	private void DoQuestFailAnalytic(string questID)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("QuestFail", questID);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Quest", questID);
			SendFlurryAnalytic("QuestFail", dictionary);
		}
	}

	private void DoAwardPowerplayAnalytic()
	{
		string analyticDescription = string.Format("PPAward{0}", CurrentHill.Instance.ID);
		if (UseGoogle)
		{
			SendGoogleAnalytic("AwardPowerPlay", analyticDescription);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Hill", CurrentHill.Instance.ID.ToString());
			SendFlurryAnalytic("PPAward", dictionary);
		}
	}

	private void DoIAPProductsSeenAnalytic()
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("IAPProducts", "ProductsSeen");
		}
		if (UseFlurry)
		{
			SendFlurryAnalytic("ProductsSeen");
		}
	}

	private void DoIAPProductsFailedAnalytic(string response)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("IAPProducts", response);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("response", response);
			SendFlurryAnalytic("ProductsFailed", dictionary);
		}
	}

	private void DoIAPPurchaseRequestAnalytic(string product)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic(CategoryIAPPurchaseRequest, product);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("product", product.ToString());
			SendFlurryAnalytic(CategoryIAPPurchaseRequest, dictionary);
		}
	}

	private void DoIAPPurchaseFailedAnalytic(string reason)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic(CategoryIAPPurchaseFailed, reason);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Reason", reason.ToString());
			SendFlurryAnalytic(CategoryIAPPurchaseFailed, dictionary);
		}
	}

	private void DoIAPPurchaseCancelledAnalytic(string product)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic(CategoryIAPCancelled, product);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Product", product.ToString());
			SendFlurryAnalytic(CategoryIAPCancelled, dictionary);
		}
	}

	private void DoIAPPurchaseCompleteAnalytic(string product)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic(CategoryIAPPurchaseComplete, product);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Product", product.ToString());
			SendFlurryAnalytic(CategoryIAPPurchaseComplete, dictionary);
		}
		_IAPPurchaseJustCompleted = true;
	}

	private void DoFreeClayPressedAnalytic()
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("FreeClay", "FreeClayPressed");
		}
		if (UseFlurry)
		{
			SendFlurryAnalytic("FreeClayPressed");
		}
	}

	private void DoFreeClayAwardedAnalytic(int amount)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("FreeClay", "FreeClayAwarded", "FreeClay", amount);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Amount", string.Format("{0}", amount));
			SendFlurryAnalytic("FreeClayAwarded", dictionary);
		}
	}

	private void DoFreeClayErrorAnalytic(string error)
	{
		if (UseGoogle)
		{
			SendGoogleAnalytic("FreeClay", error);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Error", error);
			SendFlurryAnalytic("FreeClayError", dictionary);
		}
	}

	public void SkipQuest()
	{
		string text = string.Format("SkipQ{0}_{1}_{2}", CurrentHill.Instance.ProgressData._ID, CurrentHill.Instance.ProgressData._BeastDefeatedCount, CurrentHill.Instance.ProgressData._CurrentQuestIndex);
		if (UseGoogle)
		{
			SendGoogleAnalytic("SkipQuest", text);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Quest", CurrentHill.Instance.ProgressData.FullQuestIDString());
			SendFlurryAnalytic("SkipQuest", dictionary);
		}
		DoPurchaseAfterIAPAnalytic(text);
	}

	public void DoBuyPowerplayInGameAnalytic()
	{
		string text = string.Format("IGPowerPlay_Hill_{0}", CurrentHill.Instance.ID);
		string analyticDescription = string.Format("Purchased_{0}", text);
		if (UseGoogle)
		{
			SendGoogleAnalytic("ItemPurchased", analyticDescription);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Item", text);
			SendFlurryAnalytic("ItemPurchased", dictionary);
		}
		DoPurchaseAfterIAPAnalytic(text);
	}

	public void UsePowerUp()
	{
		if (UseGoogle)
		{
			int numUpgraded = PowerupDatabase.Instance.NumUpgraded;
			string text = string.Format("_UPx{0}_", numUpgraded);
			string powerPlayPickerString = SaveData.Instance.Progress.GetPowerPlayPickerString();
			string text2 = ((CurrentGameMode.Type != GameModeType.MonsterLove) ? string.Empty : "_ML");
			string analyticDescription = string.Format("UsePowerPlay_Hill{0}{1}{2}{3}", CurrentHill.Instance.ID, text, powerPlayPickerString, text2);
			SendGoogleAnalytic("PowerUps", analyticDescription);
		}
		if (UseFlurry)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string value = string.Format("{0}_{1}", CurrentHill.Instance.ID, CurrentHill.Instance.UpgradeLevel);
			dictionary.Add("Level", value);
			dictionary.Add("Mode", ModeDetails);
			dictionary.Add("EnabledSet", SaveData.Instance.Progress.GetPowerPlayPickerString());
			dictionary.Add("UpgradedBoost", (!PowerupDatabase.Instance.IsBoostUpgraded) ? "N" : "Y");
			dictionary.Add("UpgradedFlame", (!PowerupDatabase.Instance.IsFlameUpgraded) ? "N" : "Y");
			dictionary.Add("UpgradedShrink", (!PowerupDatabase.Instance.IsShrinkUpgraded) ? "N" : "Y");
			dictionary.Add("UpgradedSplat", (!PowerupDatabase.Instance.IsSplatUpgraded) ? "N" : "Y");
			SendFlurryAnalytic("UsePowerPlay", dictionary);
		}
	}

	private void SendGoogleAnalytic(string category, string analyticDescription, string stat = null, int statValue = 0)
	{
		string page = string.Format("{0}/{1}/{2}", _ClayJamSubPage, category, analyticDescription);
		if (Application.isEditor)
		{
			Debug.Log(string.Format("Google Analytics = {0},{1},{2},{3}", category, analyticDescription, stat, statValue));
		}
		else
		{
			if (string.IsNullOrEmpty(stat))
			{
				Google.analytics.logEvent(page, category, analyticDescription);
			}
			else
			{
				Google.analytics.logEvent(page, category, analyticDescription, stat, statValue);
			}
			Google.analytics.logPageView(page);
		}
	}

	private void SendFlurryAnalytic(string eventID, Dictionary<string, string> parameters = null)
	{
		if (Application.isEditor)
		{
			StringBuilder stringBuilder = new StringBuilder(string.Format("Flurry Analytics = {0} : (", eventID), 256);
			if (parameters != null)
			{
				foreach (KeyValuePair<string, string> parameter in parameters)
				{
					stringBuilder.Append("(");
					stringBuilder.Append(parameter.Key);
					stringBuilder.Append(":");
					stringBuilder.Append(parameter.Value);
					stringBuilder.Append(")");
				}
			}
			stringBuilder.Append(")");
			Debug.Log(stringBuilder);
		}
		else if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform == RuntimePlatform.Android)
		{
			if (parameters == null)
			{
				FlurryAndroid.logEvent(eventID);
			}
			else
			{
				FlurryAndroid.logEvent(eventID, parameters, false);
			}
		}
	}

	private string ClayAmountGroup(float clayAmount)
	{
		if (clayAmount < 10f)
		{
			return "0to9";
		}
		if (clayAmount < 25f)
		{
			return "10to24";
		}
		if (clayAmount < 50f)
		{
			return "25to49";
		}
		if (clayAmount < 100f)
		{
			return "50to99";
		}
		if (clayAmount < 250f)
		{
			return "100to249";
		}
		if (clayAmount < 500f)
		{
			return "250to499";
		}
		return "500+";
	}

	private string DistanceGroup(float distance)
	{
		if (distance < 10f)
		{
			return "0to9";
		}
		if (distance < 25f)
		{
			return "10to24";
		}
		if (distance < 50f)
		{
			return "25to49";
		}
		if (distance < 100f)
		{
			return "50to99";
		}
		if (distance < 250f)
		{
			return "100to249";
		}
		if (distance < 500f)
		{
			return "250to499";
		}
		if (distance < 750f)
		{
			return "500to749";
		}
		if (distance < 999f)
		{
			return "750to998";
		}
		if (distance == 999f)
		{
			return "999";
		}
		return "1000+";
	}
}
