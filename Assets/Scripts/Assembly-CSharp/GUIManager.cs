using UnityEngine;

public class GUIManager : MonoBehaviour
{
	private void OnGUI()
	{
		if (GUI.Button(new Rect(0f, 0f, 150f, 40f), "Track 'oneOne' variants"))
		{
			Google.analytics.logPageView("/oneOne/partTwo/partThree/partFour", "Really long url");
			Google.analytics.logEvent("/oneOne/partTwo", "twoCategory", "twoAction");
			Google.analytics.logPageView("/oneOne/partTwo", "This is the partTwo page title");
			Google.analytics.logEvent("/oneOne/partTwo/partThree/partFour", "oneCategory", "oneAction");
			Google.analytics.logEvent("/pageView/part2", "categoryName", "actionName", "labelName", 100);
		}
		if (GUI.Button(new Rect(0f, 55f, 150f, 40f), "Track 'twoTwo' variants"))
		{
			Google.analytics.logPageView("/twoTwo");
			Google.analytics.logEvent("/twoTwo", "twoCategory", "twoAction", "twoLabel", 669);
		}
		if (GUI.Button(new Rect(0f, 110f, 150f, 40f), "Track 'threeThree' variants"))
		{
			Google.analytics.logPageView("/threeThree", "Im the page title");
			Google.analytics.logPageView("/threeThree/subElement");
			Google.analytics.logEvent("/threeThree/subElement", "twoCategory", "twoAction", "twoLabel", 669);
		}
	}
}
