using System;
using UnityEngine;

public class RewardedAds : MonoBehaviour
{
	public delegate void RewardCallback(int clayToAward);

	public static Action FreeClayPressedEvent;

	public static Action<int> FreeClayAwardedEvent;

	public static Action<string> FreeClayErrorEvent;

	public string AdSpace_Android = "Clay Jam Android Rewards";

	public string AdSpace_IOS = "Clay Jam iOS Client-side Rewarded";

	public int FixedClayReward = 100;

	private bool _adAvailable;

	private bool _adForcedExit;

	private int _clayEarnedThisTime;

	public static RewardedAds Instance { get; set; }

	public static bool ShouldEverShow
	{
		get
		{
			if (!BuildDetails.Instance._HasRewardedAds)
			{
				return false;
			}
			return true;
		}
	}

	public void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of RewardedAds", base.gameObject);
		}
		Instance = this;
		if (ShouldEverShow)
		{
			ListenForAdEvents();
		}
		_adForcedExit = false;
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause && _adForcedExit)
		{
			_adForcedExit = false;
		}
	}

	private void OnDestroy()
	{
		if (ShouldEverShow)
		{
			StopListenignForAdEvents();
		}
	}

	public void FetchAds()
	{
		_adAvailable = false;
		FlurryAndroid.fetchAdsForSpace(Instance.AdSpace_Android, FlurryAdPlacement.FullScreen);
		FlurryAndroid.checkIfAdIsAvailable(Instance.AdSpace_Android, FlurryAdPlacement.FullScreen, 1000L);
	}

	public bool HasAdToShow()
	{
		if (!ShouldEverShow)
		{
			return false;
		}
		return _adAvailable;
	}

	public void ShowAd()
	{
		if (ShouldEverShow)
		{
			_clayEarnedThisTime = 0;
			if (!HasAdToShow())
			{
				FetchAds();
			}
			FlurryAndroid.displayAd(AdSpace_Android, FlurryAdPlacement.FullScreen, 1000L);
			if (FreeClayPressedEvent != null)
			{
				FreeClayPressedEvent();
			}
		}
	}

	private void ListenForAdEvents()
	{
		FlurryAndroidManager.onAdClosedEvent += OnAdClosed;
		FlurryAndroidManager.adAvailableForSpaceEvent += OnAdAvailable;
		FlurryAndroidManager.onApplicationExitEvent += OnForcedExit;
		FlurryAndroidManager.onVideoCompletedEvent += OnVideoComplete;
	}

	private void StopListenignForAdEvents()
	{
		FlurryAndroidManager.onAdClosedEvent -= OnAdClosed;
		FlurryAndroidManager.adAvailableForSpaceEvent -= OnAdAvailable;
		FlurryAndroidManager.onApplicationExitEvent -= OnForcedExit;
		FlurryAndroidManager.onVideoCompletedEvent -= OnVideoComplete;
	}

	private void OnAdAvailable(string adSpace)
	{
		_adAvailable = true;
	}

	private void OnAdClosed(string adSpace)
	{
		if (_clayEarnedThisTime > 0)
		{
			FreeClayComponent.Instance.ShowRewardMessagePanel(_clayEarnedThisTime);
		}
		FetchAds();
	}

	private void OnForcedExit(string adSpace)
	{
		_adForcedExit = true;
	}

	private void OnVideoComplete(string adSpace)
	{
		GetAdReward(FixedClayReward);
	}

	public void GetAdReward(int clayAmount)
	{
		if (clayAmount > 0)
		{
			_clayEarnedThisTime += clayAmount;
			SaveData.Instance.AddClayToCollection(new ClayData(0, clayAmount));
			SaveData.Instance.Save();
			if (FreeClayAwardedEvent != null)
			{
				FreeClayAwardedEvent(clayAmount);
			}
		}
	}
}
