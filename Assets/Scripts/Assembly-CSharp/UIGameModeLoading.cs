using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/UIActivateOnGamemode")]
public class UIGameModeLoading : MonoBehaviour
{
	public GameModeType _Mode;

	public LocalisableText _ModeTitle;

	public void SetupForGame()
	{
		if (CurrentGameMode.Type != _Mode)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}

	public void SetupForFrontend()
	{
		if (_Mode != 0)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else
		{
			base.gameObject.SetActiveRecursively(true);
		}
		_ModeTitle.Activate(false);
	}
}
