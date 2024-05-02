using System.IO;
using UnityEngine;

public class EtceteraUIManager : MonoBehaviour
{
	public GameObject testPlane;

	private bool _showingMore;

	private void Start()
	{
		EtceteraAndroid.initTTS();
	}

	private void OnEnable()
	{
		EtceteraAndroidManager.albumChooserSucceededEvent += textureLoaded;
		EtceteraAndroidManager.photoChooserSucceededEvent += textureLoaded;
	}

	private void OnDisable()
	{
		EtceteraAndroidManager.albumChooserSucceededEvent -= textureLoaded;
		EtceteraAndroidManager.photoChooserSucceededEvent -= textureLoaded;
	}

	private string saveScreenshotToSDCard()
	{
		Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0, false);
		byte[] bytes = texture2D.EncodeToPNG();
		Object.Destroy(texture2D);
		string text = Path.Combine(Application.persistentDataPath, "myImage.png");
		File.WriteAllBytes(text, bytes);
		return text;
	}

	private void OnGUI()
	{
		float num = 5f;
		float left = 5f;
		float num2 = ((Screen.width < 800 && Screen.height < 800) ? 160 : 320);
		float num3 = ((Screen.width < 800 && Screen.height < 800) ? 30 : 55);
		float num4 = num3 + 3f;
		if (!_showingMore)
		{
			if (GUI.Button(new Rect(left, num, num2, num3), "Show Toast"))
			{
				EtceteraAndroid.showToast("Hi.  Something just happened in the game and I want to tell you but not interrupt you", true);
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Play Video"))
			{
				EtceteraAndroid.playMovie("http://www.daily3gp.com/vids/747.3gp", 16711680u, false, EtceteraAndroid.ScalingMode.AspectFit, true);
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Show Alert"))
			{
				EtceteraAndroid.showAlert("Alert Title Here", "Something just happened.  Do you want to have a snack?", "Yes", "Not Now");
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Single Field Prompt"))
			{
				EtceteraAndroid.showAlertPrompt("Enter Digits", "I'll call you if you give me your number", "phone number", "867-5309", "Send", "Not a Chance");
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Two Field Prompt"))
			{
				EtceteraAndroid.showAlertPromptWithTwoFields("Need Info", "Enter your credentials:", "username", "harry_potter", "password", string.Empty, "OK", "Cancel");
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Show Progress Dialog"))
			{
				EtceteraAndroid.showProgressDialog("Progress is happening", "it will be over in just a second...");
				Invoke("hideProgress", 1f);
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Text to Speech Speak"))
			{
				EtceteraAndroid.setPitch(Random.Range(0, 5));
				EtceteraAndroid.setSpeechRate(Random.Range(0.5f, 1.5f));
				EtceteraAndroid.speak("Howdy. Im a robot voice");
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Prompt for Video"))
			{
				EtceteraAndroid.promptToTakeVideo("fancyVideo");
			}
			left = (float)Screen.width - num2 - 5f;
			num = 5f;
			if (GUI.Button(new Rect(left, num, num2, num3), "Show Web View"))
			{
				EtceteraAndroid.showWebView("http://prime31.com");
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Email Composer"))
			{
				string attachmentFilePath = saveScreenshotToSDCard();
				EtceteraAndroid.showEmailComposer("noone@nothing.com", "Message subject", "click <a href='http://somelink.com'>here</a> for a present", true, attachmentFilePath);
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "SMS Composer"))
			{
				EtceteraAndroid.showSMSComposer("I did something really cool in this game!");
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Prompt to Take Photo"))
			{
				EtceteraAndroid.promptToTakePhoto(512, 512, "photo.jpg");
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Prompt for Album Image"))
			{
				EtceteraAndroid.promptForPictureFromAlbum(512, 512, "albumImage.jpg");
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Save Image to Gallery"))
			{
				string pathToPhoto = saveScreenshotToSDCard();
				bool flag = EtceteraAndroid.saveImageToGallery(pathToPhoto, "My image from Unity");
				Debug.Log("did save to gallery: " + flag);
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Ask For Review"))
			{
				EtceteraAndroid.resetAskForReview();
				EtceteraAndroid.askForReviewNow("Please rate my app!", "It will really make me happy if you do...", "com.yourgame.goeshere");
			}
		}
		else
		{
			if (GUI.Button(new Rect(left, num, num2, num3), "Enable Push"))
			{
				EtceteraAndroid.urbanAirshipEnablePush();
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Activity Started"))
			{
				EtceteraAndroid.urbanAirshipActivityStarted();
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Activity Stopped"))
			{
				EtceteraAndroid.urbanAirshipActivityStopped();
			}
			if (GUI.Button(new Rect(left, num += num4, num2, num3), "Get APID"))
			{
				string text = EtceteraAndroid.urbanAirshipGetAPID();
				Debug.Log("apid: " + text);
			}
		}
		string text2 = ((!_showingMore) ? "Show Urban Airship" : "Hide Urban Airship");
		if (GUI.Button(new Rect(left, num += num4, num2, num3), text2))
		{
			_showingMore = !_showingMore;
		}
	}

	private void hideProgress()
	{
		EtceteraAndroid.hideProgressDialog();
	}

	public void textureLoaded(string imagePath, Texture2D texture)
	{
		testPlane.renderer.material.mainTexture = texture;
	}
}
