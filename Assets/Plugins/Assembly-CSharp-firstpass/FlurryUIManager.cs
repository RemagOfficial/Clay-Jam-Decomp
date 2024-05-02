using System.Collections.Generic;
using Prime31;
using UnityEngine;

public class FlurryUIManager : MonoBehaviourGUI
{
	private void OnGUI()
	{
		beginColumn();
		if (GUILayout.Button("Start Flurry Session"))
		{
			FlurryAndroid.onStartSession("RPQYDGBDSQ7Z3DPM7XVU", true, true, true);
		}
		if (GUILayout.Button("Fetch Ads"))
		{
			FlurryAndroid.fetchAdsForSpace("space", FlurryAdPlacement.BannerBottom);
			FlurryAndroid.fetchAdsForSpace("bigAd", FlurryAdPlacement.FullScreen);
		}
		if (GUILayout.Button("Display Ad"))
		{
			FlurryAndroid.displayAd("space", FlurryAdPlacement.BannerBottom, 1000L);
		}
		if (GUILayout.Button("Display bigAd Ad"))
		{
			FlurryAndroid.displayAd("bigAd", FlurryAdPlacement.FullScreen, 1000L);
		}
		if (GUILayout.Button("Remove Ad"))
		{
			FlurryAndroid.removeAd("space");
		}
		endColumn(true);
		if (GUILayout.Button("Log Timed Event"))
		{
			FlurryAndroid.logEvent("timed", true);
		}
		if (GUILayout.Button("End Timed Event"))
		{
			FlurryAndroid.endTimedEvent("timed");
		}
		if (GUILayout.Button("Log Event"))
		{
			FlurryAndroid.logEvent("myFancyEvent");
		}
		if (GUILayout.Button("Log Event with Params"))
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("akey1", "value1");
			dictionary.Add("bkey2", "value2");
			dictionary.Add("ckey3", "value3");
			dictionary.Add("dkey4", "value4");
			FlurryAndroid.logEvent("EventWithParams", dictionary);
		}
		if (GUILayout.Button("Log Page View"))
		{
			FlurryAndroid.onPageView();
		}
		if (GUILayout.Button("Log Error"))
		{
			FlurryAndroid.onError("666", "bad things happend", "Exception");
		}
		endColumn();
	}
}
