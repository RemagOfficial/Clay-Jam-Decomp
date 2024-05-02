using Prime31;
using UnityEngine;

public class RewardedAdsTestGUI : MonoBehaviourGUI
{
	private static int _clayWon;

	private void OnGUI()
	{
		float num = Screen.height / 8;
		Rect position = new Rect(0f, 0f, Screen.width, num);
		if (GUI.Button(position, "show ad"))
		{
			_clayWon = 0;
			RewardedAds.Instance.ShowAd();
		}
		position.y += num;
		GUI.Label(position, "CLAY " + _clayWon);
		position.y += num;
	}

	private static void OnRewardResponse(int ClayWon)
	{
		_clayWon = ClayWon;
		RewardedAds.Instance.GetAdReward(_clayWon);
	}
}
