using UnityEngine;

public class SystemPrompt
{
	private const string _RatingURL_IOS = "https://itunes.apple.com/us/app/clay-jam/id565997541?ls=1&mt=8";

	private const string _AndroidPackage = "com.zynga.fatpebble.clayjam";

	private static string RatingURL
	{
		get
		{
			return "com.zynga.fatpebble.clayjam";
		}
	}

	public static void AskForReview()
	{
		string text = Localization.instance.Get("PROMPT_05");
		string text2 = Localization.instance.Get("PROMPT_04");
		Debug.Log(string.Format("asking for review :\n{0}\n{1}\n{2}", text, text2, RatingURL));
		EtceteraAndroid.askForReviewNow(text, text2, RatingURL, BuildDetails.Instance._IsAmazonAppStore);
	}
}
