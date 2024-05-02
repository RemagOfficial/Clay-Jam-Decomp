using UnityEngine;

public class RewardedAdButton : MonoBehaviour
{
	public UISprite _ButtonVisuals;

	public Collider _ButtonCollider;

	public GameObject _ButtonText;

	private bool EverythingOn
	{
		get
		{
			return _ButtonVisuals.enabled && _ButtonCollider.enabled && _ButtonText.active;
		}
	}

	private bool SomethingOn
	{
		get
		{
			return _ButtonVisuals.enabled || _ButtonCollider.enabled || _ButtonText.active;
		}
	}

	private void OnEnable()
	{
		TurnOn(false);
		if (RewardedAds.ShouldEverShow)
		{
			RewardedAds.Instance.FetchAds();
		}
	}

	private void Update()
	{
		if (RewardedAds.Instance.HasAdToShow())
		{
			if (!EverythingOn)
			{
				TurnOn(true);
			}
		}
		else if (SomethingOn)
		{
			TurnOn(false);
		}
	}

	private void TurnOn(bool on)
	{
		_ButtonVisuals.enabled = on;
		_ButtonCollider.enabled = on;
		_ButtonText.SetActiveRecursively(on);
	}
}
