using System.Collections.Generic;
using UnityEngine;

public class FlurryAndroid
{
	private static AndroidJavaClass _flurryAgent;

	private static AndroidJavaObject _plugin;

	static FlurryAndroid()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		_flurryAgent = new AndroidJavaClass("com.flurry.android.FlurryAgent");
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.prime31.FlurryPlugin"))
		{
			_plugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
		}
	}

	public static string getAndroidId()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return string.Empty;
		}
		return _plugin.Call<string>("getAndroidId", new object[0]);
	}

	public static void onStartSession(string apiKey, bool initializeAds, bool initializeWalletModule, bool enableTestAds)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("onStartSession", apiKey, initializeAds, initializeWalletModule, enableTestAds);
		}
	}

	public static void onEndSession()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("onEndSession");
		}
	}

	public static void addUserCookie(string key, string value)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("addUserCookie", key, value);
		}
	}

	public static void clearUserCookies()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("clearUserCookies");
		}
	}

	public static void setContinueSessionMillis(long milliseconds)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_flurryAgent.CallStatic("setContinueSessionMillis", milliseconds);
		}
	}

	public static void logEvent(string eventName)
	{
		logEvent(eventName, false);
	}

	public static void logEvent(string eventName, bool isTimed)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			if (isTimed)
			{
				_plugin.Call("logTimedEvent", eventName);
			}
			else
			{
				_plugin.Call("logEvent", eventName);
			}
		}
	}

	public static void logEvent(string eventName, Dictionary<string, string> parameters)
	{
		logEvent(eventName, parameters, false);
	}

	public static void logEvent(string eventName, Dictionary<string, string> parameters, bool isTimed)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			if (parameters == null)
			{
				Debug.LogError("attempting to call logEvent with null parameters");
			}
			else if (isTimed)
			{
				_plugin.Call("logTimedEventWithParams", eventName, parameters.toJson());
			}
			else
			{
				_plugin.Call("logEventWithParams", eventName, parameters.toJson());
			}
		}
	}

	public static void endTimedEvent(string eventName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("endTimedEvent", eventName);
		}
	}

	public static void onPageView()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_flurryAgent.CallStatic("onPageView");
		}
	}

	public static void onError(string errorId, string message, string errorClass)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_flurryAgent.CallStatic("onError", errorId, message, errorClass);
		}
	}

	public static void setUserID(string userId)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_flurryAgent.CallStatic("setUserId", userId);
		}
	}

	public static void setAge(int age)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_flurryAgent.CallStatic("setAge", age);
		}
	}

	public static void setLogEnabled(bool enable)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_flurryAgent.CallStatic("setLogEnabled", enable);
		}
	}

	public static void fetchAdsForSpace(string adSpace, FlurryAdPlacement adSize)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("fetchAdsForSpace", adSpace, (int)adSize);
		}
	}

	public static void displayAd(string adSpace, FlurryAdPlacement adSize, long timeout)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("displayAd", adSpace, (int)adSize, timeout);
		}
	}

	public static void removeAd(string adSpace)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("removeAd", adSpace);
		}
	}

	public static void checkIfAdIsAvailable(string adSpace, FlurryAdPlacement adSize, long timeout)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("isAdAvailable", adSpace, (int)adSize, timeout);
		}
	}

	public static void addObserverForCurrency(string currency)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("addObserverForCurrency", currency);
		}
	}

	public static void decrementCurrency(string currency, float amount)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("decrementCurrency", currency, amount);
		}
	}

	public static float getCurrencyAmount(string currency)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return 0f;
		}
		return _plugin.Call<float>("getCurrencyAmount", new object[1] { currency });
	}
}
