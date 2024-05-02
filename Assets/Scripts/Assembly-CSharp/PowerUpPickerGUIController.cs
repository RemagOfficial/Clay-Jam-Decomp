using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PowerUpPickerGUIController : MonoBehaviour
{
	public GameObject _ParentPanel;

	public GameObject _BuyPanel;

	public GameObject _PickerPanel;

	public PowerupToggle[] _PowerupToggles;

	public LocalisableText _BuyText;

	public UILabel _BuyCostText;

	public GameObject _BuyButton;

	public GameObject _CantAffordIcon;

	private UISpriteAnimationOnEvent _buyButtonAnim;

	private Collider[] _buyPanelButtons;

	private Collider[] _pickerPanelButtons;

	private bool ShouldShow
	{
		get
		{
			if (BuildDetails.Instance._DemoMode)
			{
				return false;
			}
			if (!SaveData.Instance.Progress._optionTutorialsOn.Set)
			{
				return true;
			}
			TutorialData tutorialData = SaveData.Instance.Tutorials.GetTutorialData("tutorial_powerplay");
			return tutorialData == null || tutorialData.NumPlays > 0;
		}
	}

	[method: MethodImpl(32)]
	public static event Action InGamePoweplayBought;

	private void Awake()
	{
		_BuyPanel.SetActiveRecursively(true);
		_buyPanelButtons = _BuyPanel.GetComponentsInChildren<Collider>();
		_buyButtonAnim = _BuyButton.GetComponent<UISpriteAnimationOnEvent>();
		_BuyPanel.SetActiveRecursively(false);
		_PickerPanel.SetActiveRecursively(true);
		_pickerPanelButtons = _PickerPanel.GetComponentsInChildren<Collider>();
		_PickerPanel.SetActiveRecursively(false);
	}

	private void OnEnable()
	{
		UIEvents.ButtonPressed_WithTarget += ButtonPressed;
		IAPPanelController.IAPPurchaseComplete += OnIAPPurchaseComplete;
	}

	private void OnDisable()
	{
		UIEvents.ButtonPressed_WithTarget -= ButtonPressed;
		IAPPanelController.IAPPurchaseComplete -= OnIAPPurchaseComplete;
	}

	public void TurnOn()
	{
		if (!ShouldShow)
		{
			TurnOff();
			return;
		}
		_ParentPanel.SetActiveRecursively(true);
		if (CurrentHill.Instance.ProgressData._PowerPlaysRemaining > 0)
		{
			bool showHintAnim = !SaveData.Instance.Progress._hasUsedPowerPlayPicker.Set;
			TurnOnPowerPlayPicker(true, showHintAnim);
			TurnOnPowerPlayBuy(false);
		}
		else
		{
			TurnOnPowerPlayPicker(false);
			TurnOnPowerPlayBuy(true);
		}
	}

	public void TurnOff()
	{
		_ParentPanel.SetActiveRecursively(false);
		TurnOnPowerPlayPicker(false);
		TurnOnPowerPlayBuy(false);
		EnableToggles(false);
	}

	public void EnableToggles(bool shouldBeEnabled)
	{
		PowerupToggle[] powerupToggles = _PowerupToggles;
		foreach (PowerupToggle powerupToggle in powerupToggles)
		{
			powerupToggle.enabled = shouldBeEnabled;
		}
	}

	private void TurnOnPowerPlayBuy(bool on)
	{
		_BuyPanel.SetActiveRecursively(on);
		_BuyText.text = GetBuyText();
		_BuyCostText.text = GetBuyCostText();
		if (on)
		{
			UpdateBuyButtonAffordability();
		}
		else
		{
			EnableButtons(_buyPanelButtons, false);
		}
	}

	public void UpdateBuyButtonAffordability()
	{
		if (_BuyButton.activeSelf)
		{
			bool flag = BuildDetails.Instance._HasIAP || CanAffordPowerUp();
			if (flag)
			{
				_buyButtonAnim.PlayOnEnableAnim();
				EnableButtons(_buyPanelButtons, true);
			}
			else
			{
				_buyButtonAnim.PlayOnReleaseAnim();
				EnableButtons(_buyPanelButtons, false);
			}
			if (_CantAffordIcon != null)
			{
				_CantAffordIcon.SetActive(!flag);
			}
		}
	}

	private void TurnOnPowerPlayPicker(bool on, bool showHintAnim = false)
	{
		_PickerPanel.SetActiveRecursively(on);
		if (on)
		{
			EnableToggles(true);
			if (showHintAnim)
			{
				TurnOnToggles();
				_PickerPanel.animation.Play();
			}
			else
			{
				SetToggles();
			}
			SetUpgrades();
		}
		EnableButtons(_pickerPanelButtons, on);
	}

	private void EnableButtons(Collider[] colliders, bool enabled)
	{
		foreach (Collider collider in colliders)
		{
			collider.enabled = enabled;
		}
	}

	private void SetToggles()
	{
		PowerupToggle[] powerupToggles = _PowerupToggles;
		foreach (PowerupToggle powerupToggle in powerupToggles)
		{
			powerupToggle.ShowUserSetting();
		}
	}

	private void SetUpgrades()
	{
		PowerupToggle[] powerupToggles = _PowerupToggles;
		foreach (PowerupToggle powerupToggle in powerupToggles)
		{
			powerupToggle.ShowUpgraded();
		}
	}

	private void TurnOnToggles()
	{
		PowerupToggle[] powerupToggles = _PowerupToggles;
		foreach (PowerupToggle powerupToggle in powerupToggles)
		{
			powerupToggle.TurnOn();
		}
	}

	private void ButtonPressed(GameObject buttonTarget)
	{
		if (_PickerPanel.active)
		{
			CheckPowerPlayToggle(buttonTarget);
		}
		else if (_BuyPanel.active)
		{
			CheckBuy(buttonTarget);
		}
	}

	private void CheckPowerPlayToggle(GameObject buttonTarget)
	{
		PowerupToggle[] powerupToggles = _PowerupToggles;
		foreach (PowerupToggle powerupToggle in powerupToggles)
		{
			powerupToggle.DoToggle(buttonTarget);
		}
		SetToggles();
	}

	private void CheckBuy(GameObject buttonTarget)
	{
		if (buttonTarget == _BuyButton)
		{
			BuyPowerUpPressed();
		}
	}

	private void BuyPowerUpPressed()
	{
		if (CanAffordPowerUp())
		{
			BuyPowerUp();
			TurnOnPowerPlayBuy(false);
			TurnOnPowerPlayPicker(true);
		}
		else if (BuildDetails.Instance._HasIAP)
		{
			StaticIAPItems._forcedUserToStore = true;
			UIEvents.SendEvent(UIEventType.ResetToPanel, InGameNGUI.Instance._IAPPanel);
		}
	}

	private bool CanAffordPowerUp()
	{
		return CurrentHill.Instance.ProgressData.CanAffordPowerplayPack(0);
	}

	private void BuyPowerUp()
	{
		CurrentHill.Instance.ProgressData.PurchasePowerplays(0);
		SaveData.Instance.Save();
		if (PowerUpPickerGUIController.InGamePoweplayBought != null)
		{
			PowerUpPickerGUIController.InGamePoweplayBought();
		}
	}

	private string GetBuyText()
	{
		return Localization.instance.Get("POWERPLAY_PICKER_buy");
	}

	private string GetBuyCostText()
	{
		string format = Localization.instance.Get("POWERPLAY_PICKER_cost");
		string arg = Localization.PunctuatedNumber(CurrentHill.Instance.ProgressData.CostOfPowerPack(0), int.MaxValue);
		return string.Format(format, arg);
	}

	private void OnIAPPurchaseComplete(string product)
	{
		SetUpgrades();
	}
}
