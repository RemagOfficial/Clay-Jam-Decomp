using UnityEngine;

public class StoreFactory
{
	public static IStore GetStoreImplementation()
	{
		if (!BuildDetails.Instance._HasIAP)
		{
			return null;
		}
		if (BuildDetails.Instance.IAPProvider == IAPGlobal.IAPProvider.DEBUG)
		{
			Debug.Log("StoreFactory.InstatiateStore(): Application set for Android but set to use DebugStore");
			return new DebugStore();
		}
		if (Application.platform == RuntimePlatform.Android)
		{
			return new GooglePlayStore();
		}
		Debug.Log("StoreFactory.InstatiateStore(): Application set for Android but not running on Android");
		return new DebugStore();
	}

	public static IStoreEventDispatcher GetStoreEventDispatcher()
	{
		return null;
	}
}
