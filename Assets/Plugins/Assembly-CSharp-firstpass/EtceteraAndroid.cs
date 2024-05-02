using UnityEngine;

public class EtceteraAndroid
{
	public enum ScalingMode
	{
		None = 0,
		AspectFit = 1,
		Fill = 2
	}

	private static AndroidJavaObject _plugin;

	static EtceteraAndroid()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.prime31.EtceteraPlugin"))
		{
			_plugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
		}
	}

	public static void playMovie(string pathOrUrl, uint bgColor, bool showControls, ScalingMode scalingMode, bool closeOnTouch)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("playMovie", pathOrUrl, (int)bgColor, showControls, (int)scalingMode, closeOnTouch);
		}
	}

	public static void showToast(string text, bool useShortDuration)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showToast", text, useShortDuration);
		}
	}

	public static void showAlert(string title, string message, string positiveButton)
	{
		showAlert(title, message, positiveButton, string.Empty);
	}

	public static void showAlert(string title, string message, string positiveButton, string negativeButton)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showAlert", title, message, positiveButton, negativeButton);
		}
	}

	public static void showAlertPrompt(string title, string message, string promptHint, string promptText, string positiveButton, string negativeButton)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showAlertPrompt", title, message, promptHint, promptText, positiveButton, negativeButton);
		}
	}

	public static void showAlertPromptWithTwoFields(string title, string message, string promptHintOne, string promptTextOne, string promptHintTwo, string promptTextTwo, string positiveButton, string negativeButton)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showAlertPromptWithTwoFields", title, message, promptHintOne, promptTextOne, promptHintTwo, promptTextTwo, positiveButton, negativeButton);
		}
	}

	public static void showProgressDialog(string title, string message)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showProgressDialog", title, message);
		}
	}

	public static void hideProgressDialog()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("hideProgressDialog");
		}
	}

	public static void showWebView(string url)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showWebView", url);
		}
	}

	public static void showCustomWebView(string url, bool disableTitle, bool disableBackButton)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showCustomWebView", url, disableTitle, disableBackButton);
		}
	}

	public static void showEmailComposer(string toAddress, string subject, string text, bool isHTML)
	{
		showEmailComposer(toAddress, subject, text, isHTML, string.Empty);
	}

	public static void showEmailComposer(string toAddress, string subject, string text, bool isHTML, string attachmentFilePath)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showEmailComposer", toAddress, subject, text, isHTML, attachmentFilePath);
		}
	}

	public static void showSMSComposer(string text)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showSMSComposer", text);
		}
	}

	public static void promptToTakePhoto(int desiredWidth, int desiredHeight, string name)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("promptToTakePhoto", desiredWidth, desiredHeight, name);
		}
	}

	public static void promptForPictureFromAlbum(int desiredWidth, int desiredHeight, string name)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("promptForPictureFromAlbum", desiredWidth, desiredHeight, name);
		}
	}

	public static void promptToTakeVideo(string name)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("promptToTakeVideo", name);
		}
	}

	public static bool saveImageToGallery(string pathToPhoto, string title)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		return _plugin.Call<bool>("saveImageToGallery", new object[2] { pathToPhoto, title });
	}

	public static void scaleImageAtPath(string pathToImage, float scale)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("scaleImageAtPath", pathToImage, scale);
		}
	}

	public static void initTTS()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("initTTS");
		}
	}

	public static void teardownTTS()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("teardownTTS");
		}
	}

	public static void speak(string text)
	{
		speak(text, TTSQueueMode.Add);
	}

	public static void speak(string text, TTSQueueMode queueMode)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("speak", text, (int)queueMode);
		}
	}

	public static void stop()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("stop");
		}
	}

	public static void playSilence(long durationInMs, TTSQueueMode queueMode)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("playSilence", durationInMs, (int)queueMode);
		}
	}

	public static void setPitch(float pitch)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("setPitch", pitch);
		}
	}

	public static void setSpeechRate(float rate)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("setSpeechRate", rate);
		}
	}

	public static void askForReview(int launchesUntilPrompt, int hoursUntilFirstPrompt, int hoursBetweenPrompts, string title, string message, string appPackageName, bool isAmazonAppStore = false)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			if (isAmazonAppStore)
			{
				_plugin.Set("isAmazonAppStore", true);
			}
			_plugin.Call("askForReview", launchesUntilPrompt, hoursUntilFirstPrompt, hoursBetweenPrompts, title, message, appPackageName);
		}
	}

	public static void askForReviewNow(string title, string message, string appPackageName, bool isAmazonAppStore = false)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			if (isAmazonAppStore)
			{
				_plugin.Set("isAmazonAppStore", true);
			}
			_plugin.Call("askForReviewNow", title, message, appPackageName);
		}
	}

	public static void resetAskForReview()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("resetAskForReview");
		}
	}

	public static void urbanAirshipEnablePush()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("urbanAirshipEnablePush");
		}
	}

	public static void urbanAirshipDisablePush()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("urbanAirshipDisablePush");
		}
	}

	public static void urbanAirshipActivityStarted()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("urbanAirshipActivityStarted");
		}
	}

	public static void urbanAirshipActivityStopped()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("urbanAirshipActivityStopped");
		}
	}

	public static string urbanAirshipGetAPID()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return string.Empty;
		}
		return _plugin.Call<string>("urbanAirshipGetAPID", new object[0]);
	}
}
