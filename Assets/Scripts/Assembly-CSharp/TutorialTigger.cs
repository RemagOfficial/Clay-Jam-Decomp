using System;

public static class TutorialTigger
{
	public enum Type
	{
		PlayButtonShown = 0,
		PressPlay = 1,
		ShopButtonShown = 2,
		ShopButtonHidden = 3,
		JVPActivated = 4,
		JVPSpinnerSpun = 5,
		TapScreen = 6,
		PlayButtonHidden = 7,
		HighlightPurchasableItem = 8,
		HighlightUnpurchasableItem = 9,
		GameStarted = 10,
		FlickZoneEntered = 11,
		PowerPlayWon = 12,
		ItemPurchased = 13,
		PowerPlayLaunched = 14,
		PauseMenuShown = 15,
		JVPDeactivated = 16,
		HillCompleted = 17,
		AllHillsCompleted = 18
	}

	public static void RegisterHandler(Type type, Action handler)
	{
		switch (type)
		{
		case Type.PlayButtonShown:
			FrontendWorldController.FirstPlayButtonShown += handler;
			break;
		case Type.PressPlay:
			FrontendWorldController.StartGamePressed += handler;
			InGameController.PlayPressedEvent += handler;
			break;
		case Type.ShopButtonShown:
			JVPController.ShopButtonShownEvent += handler;
			FrontendWorldController.NewHillSelectedEvent += handler;
			break;
		case Type.ShopButtonHidden:
			JVPController.ShopButtonHiddenEvent += handler;
			FrontendWorldController.HillAboutToBeDeselected += handler;
			break;
		case Type.JVPActivated:
			JVPController.JVPBecameActiveEvent += handler;
			break;
		case Type.JVPSpinnerSpun:
			HatBrim.NewHighlightChosenEvent += handler;
			break;
		case Type.TapScreen:
			TutorialDatabase.ScreenTapped += handler;
			break;
		case Type.PlayButtonHidden:
			FrontendWorldController.PlayButtonHidden += handler;
			break;
		case Type.HighlightPurchasableItem:
			JVPController.GoodHighlightEvent += handler;
			break;
		case Type.HighlightUnpurchasableItem:
			JVPController.BadHighlightEvent += handler;
			break;
		case Type.GameStarted:
			InGameController.RunStarted += handler;
			break;
		case Type.FlickZoneEntered:
			InGameController.FlickZoneEntered += handler;
			break;
		case Type.PowerPlayWon:
			PrizeGUIController.AwardedPowerPlayEvent += handler;
			break;
		case Type.ItemPurchased:
			JVPController.SuccessfulPurchaseEvent += handler;
			break;
		case Type.PowerPlayLaunched:
			PowerPlayLauncher.PowerPlayLaunched += handler;
			break;
		case Type.PauseMenuShown:
			InGameNGUI.PauseMenuShownEvent += handler;
			break;
		case Type.JVPDeactivated:
			JVPController.JVPBecameInactiveEvent += handler;
			break;
		case Type.HillCompleted:
			CastData.CastPurchaseCompletesHillEvent += handler;
			break;
		case Type.AllHillsCompleted:
			CastData.CastPurchaseCompletesAllHillsEvent += handler;
			break;
		}
	}

	public static void UnregisterHandler(Type type, Action handler)
	{
		switch (type)
		{
		case Type.PlayButtonShown:
			FrontendWorldController.FirstPlayButtonShown -= handler;
			break;
		case Type.PressPlay:
			FrontendWorldController.StartGamePressed -= handler;
			InGameController.PlayPressedEvent -= handler;
			break;
		case Type.ShopButtonShown:
			JVPController.ShopButtonShownEvent -= handler;
			FrontendWorldController.NewHillSelectedEvent -= handler;
			break;
		case Type.ShopButtonHidden:
			JVPController.ShopButtonHiddenEvent -= handler;
			FrontendWorldController.HillAboutToBeDeselected -= handler;
			break;
		case Type.JVPActivated:
			JVPController.JVPBecameActiveEvent -= handler;
			break;
		case Type.JVPSpinnerSpun:
			HatBrim.NewHighlightChosenEvent -= handler;
			break;
		case Type.TapScreen:
			TutorialDatabase.ScreenTapped -= handler;
			break;
		case Type.PlayButtonHidden:
			FrontendWorldController.PlayButtonHidden -= handler;
			break;
		case Type.HighlightPurchasableItem:
			JVPController.GoodHighlightEvent -= handler;
			break;
		case Type.HighlightUnpurchasableItem:
			JVPController.BadHighlightEvent -= handler;
			break;
		case Type.GameStarted:
			InGameController.RunStarted -= handler;
			break;
		case Type.FlickZoneEntered:
			InGameController.FlickZoneEntered -= handler;
			break;
		case Type.PowerPlayWon:
			PrizeGUIController.AwardedPowerPlayEvent -= handler;
			break;
		case Type.ItemPurchased:
			JVPController.SuccessfulPurchaseEvent -= handler;
			break;
		case Type.PowerPlayLaunched:
			PowerPlayLauncher.PowerPlayLaunched -= handler;
			break;
		case Type.PauseMenuShown:
			InGameNGUI.PauseMenuShownEvent -= handler;
			break;
		case Type.JVPDeactivated:
			JVPController.JVPBecameInactiveEvent -= handler;
			break;
		case Type.HillCompleted:
			CastData.CastPurchaseCompletesHillEvent -= handler;
			break;
		case Type.AllHillsCompleted:
			CastData.CastPurchaseCompletesAllHillsEvent -= handler;
			break;
		}
	}
}
