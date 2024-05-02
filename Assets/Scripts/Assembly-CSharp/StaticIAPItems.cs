using System.Collections.Generic;
using UnityEngine;

public class StaticIAPItems : IAPItemSelector
{
	private enum IAPTabType
	{
		CLAY = 0,
		UPGRADES = 1
	}

	private const string ServerTransInAnim = "ServerPanelTransIn";

	private const string ServerTransOutAnim = "ServerPanelTransOut";

	public GameObject _ClayTab;

	public GameObject _UpgradeTab;

	public GameObject _ClayTabButton;

	public GameObject _UpgradeTabButton;

	public GameObject _RestoreButton;

	public List<GameObject> _ClayItemPanels;

	public List<GameObject> _UpgradeItemPanels;

	public Animation _ServerPanelAnim;

	public LocalisableText _ConnectingText;

	public LocalisableText _ConnectionFailedText;

	public LocalisableText _PurchasedFailedText;

	public LocalisableText _PurchasedCompleteText;

	public GameObject _BackButton;

	public static bool _forcedUserToStore;

	private List<StoreProductVO> _products;

	private bool _isAwake;

	private bool _readyToPurchase;

	private bool _restoringTransactions;

	private IAPTabType _currentTab;

	public static StaticIAPItems Instance { get; private set; }

	public bool InRestoreMode
	{
		get
		{
			return _restoringTransactions;
		}
	}

	private bool CanRestore
	{
		get
		{
			return false;
		}
	}

	public override bool IsAwake
	{
		get
		{
			return _isAwake;
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("Second instance of StaticIAPItems", base.gameObject);
		}
		Instance = this;
		HidePanels();
		_isAwake = true;
		FireAwakeEvent();
		_currentTab = IAPTabType.CLAY;
	}

	private void Start()
	{
		HookUpBuyButtonTriggers();
		UIEvents.ButtonPressed_WithTarget += ButtonPressed;
		_restoringTransactions = false;
	}

	private void OnDisable()
	{
		_readyToPurchase = false;
		_restoringTransactions = false;
	}

	private void OnDestroy()
	{
		UIEvents.ButtonPressed_WithTarget -= ButtonPressed;
	}

	private void ButtonPressed(GameObject buttonTarget)
	{
		if (_ServerPanelAnim.gameObject.active)
		{
			return;
		}
		if (_currentTab == IAPTabType.CLAY)
		{
			if (_UpgradeTabButton != null && buttonTarget.name == _UpgradeTabButton.name)
			{
				_currentTab = IAPTabType.UPGRADES;
				InitItemPanels();
			}
		}
		else if (_ClayTabButton != null && buttonTarget.name == _ClayTabButton.name)
		{
			_currentTab = IAPTabType.CLAY;
			InitItemPanels();
		}
		else if (_RestoreButton != null && buttonTarget.name == _RestoreButton.name && CanRestore)
		{
			IAPGlobal.Instance.Store.restoreCompletedTransactions();
			ShowMakingPurchaseState();
			_restoringTransactions = true;
		}
	}

	public override void ShowConnecting()
	{
		_readyToPurchase = false;
		HidePanels();
		ShowConnectingMessage();
	}

	public override void Init(List<StoreProductVO> storeProducts)
	{
		if (!_isAwake)
		{
			_readyToPurchase = false;
			return;
		}
		_products = storeProducts;
		ShowPanels();
		_readyToPurchase = true;
		_restoringTransactions = false;
	}

	public override void FailedToConnect()
	{
		ShowConnectionFailedMessage();
	}

	public override void FailedToPurchase()
	{
		ShowFailedToPurchaseMessage();
	}

	public override void PurchaseCancelled()
	{
		ShowPanels();
	}

	public override void PurchaseComplete(int clayBought)
	{
		ShowPurchaseCompleteMessage(clayBought);
	}

	public override void RestoreTransactionsFailed()
	{
		ShowPanels();
		_restoringTransactions = false;
	}

	public override void RestoreTransactionsComplete()
	{
		_ServerPanelAnim.gameObject.active = false;
		_ConnectingText.Activate(false);
		ShowPanels();
		_restoringTransactions = false;
	}

	private void HidePanels()
	{
		ActivateCurrentTab();
		foreach (GameObject clayItemPanel in _ClayItemPanels)
		{
			clayItemPanel.SetActiveRecursively(false);
		}
		foreach (GameObject upgradeItemPanel in _UpgradeItemPanels)
		{
			upgradeItemPanel.SetActiveRecursively(false);
		}
		if (_RestoreButton != null)
		{
			_RestoreButton.gameObject.transform.parent.gameObject.SetActiveRecursively(false);
		}
	}

	private void ActivateCurrentTab()
	{
		if (_currentTab == IAPTabType.UPGRADES)
		{
			if (_ClayTab != null)
			{
				_ClayTab.SetActiveRecursively(false);
			}
			if (_UpgradeTab != null)
			{
				_UpgradeTab.SetActiveRecursively(true);
			}
			if (_RestoreButton != null)
			{
				_RestoreButton.gameObject.transform.parent.gameObject.SetActiveRecursively(CanRestore);
			}
		}
		else
		{
			if (_UpgradeTab != null)
			{
				_UpgradeTab.SetActiveRecursively(false);
			}
			if (_ClayTab != null)
			{
				_ClayTab.SetActiveRecursively(true);
			}
		}
	}

	private void InitItemPanels()
	{
		ActivateCurrentTab();
		int i = 0;
		int j = 0;
		int productIndex;
		for (productIndex = 0; productIndex < IAPGlobal.Instance._Products.Count; productIndex++)
		{
			StoreProductVO storeProductVO = _products.Find((StoreProductVO p) => p.productIdentifier == IAPGlobal.Instance._Products[productIndex]._StoreId);
			if (storeProductVO == null)
			{
				continue;
			}
			ProductVO productVO = IAPGlobal.Instance._Products[productIndex];
			bool useCupidPanel = productVO._UseCupidPanel;
			if (productVO._Type == ProductTypeEnum.ProductType.NonConsumable)
			{
				if (_currentTab == IAPTabType.UPGRADES && SetupUpgradePanelWithProduct(_UpgradeItemPanels[j], storeProductVO, productVO, useCupidPanel))
				{
					j++;
				}
			}
			else if (_currentTab == IAPTabType.CLAY && SetupClayPanelWithProduct(_ClayItemPanels[i], storeProductVO, productVO))
			{
				i++;
			}
		}
		for (; i < _ClayItemPanels.Count; i++)
		{
			_ClayItemPanels[i].SetActiveRecursively(false);
		}
		for (; j < _UpgradeItemPanels.Count; j++)
		{
			_UpgradeItemPanels[j].SetActiveRecursively(false);
		}
	}

	private bool SetupUpgradePanelWithProduct(GameObject panel, StoreProductVO storeProduct, ProductVO clayJamProduct, bool fixedIcon = false)
	{
		if (clayJamProduct == null)
		{
			Debug.LogWarning(string.Format("ProductIdentifier: {0}, not reconised. Something has been entered wrong on the server", storeProduct.productIdentifier));
			return false;
		}
		LocalisableText componentByName = GetComponentByName<LocalisableText>(panel, "Description");
		if (componentByName != null)
		{
			componentByName.text = storeProduct.description.ToUpper();
		}
		LocalisableText componentByName2 = GetComponentByName<LocalisableText>(panel, "Title");
		if (componentByName2 != null)
		{
			componentByName2.text = storeProduct.title.ToUpper();
		}
		if (!fixedIcon)
		{
			UISprite componentByName3 = GetComponentByName<UISprite>(panel, "Icon");
			if (componentByName3 != null)
			{
				componentByName3.spriteName = clayJamProduct._IconName;
			}
		}
		if (SaveData.Instance.Purchases.IsPurchased(clayJamProduct._StoreId))
		{
			LocalisableText componentByName4 = GetComponentByName<LocalisableText>(panel, "Cost");
			if (componentByName4 != null)
			{
				componentByName4.gameObject.SetActiveRecursively(false);
			}
			Transform componentByName5 = GetComponentByName<Transform>(panel, "SaleBadge");
			if (componentByName5 != null)
			{
				componentByName5.gameObject.SetActiveRecursively(false);
			}
			UIButtonEvent componentByName6 = GetComponentByName<UIButtonEvent>(panel, "BuyButton");
			if (componentByName6 != null)
			{
				componentByName6.gameObject.SetActiveRecursively(false);
			}
			MeshRenderer componentByName7 = GetComponentByName<MeshRenderer>(panel, "Purchased");
			if (componentByName7 != null)
			{
				componentByName7.gameObject.SetActiveRecursively(true);
			}
		}
		else
		{
			MeshRenderer componentByName8 = GetComponentByName<MeshRenderer>(panel, "Purchased");
			if (componentByName8 != null)
			{
				componentByName8.gameObject.SetActiveRecursively(false);
			}
			LocalisableText componentByName9 = GetComponentByName<LocalisableText>(panel, "Cost");
			if (componentByName9 != null)
			{
				componentByName9.text = storeProduct.formattedPrice;
			}
			Transform componentByName10 = GetComponentByName<Transform>(panel, "SaleBadge");
			if (componentByName10 != null)
			{
				componentByName10.gameObject.SetActiveRecursively(clayJamProduct._IsSaleItem);
			}
			UIButtonEvent componentByName11 = GetComponentByName<UIButtonEvent>(panel, "BuyButton");
			if ((bool)componentByName11)
			{
				componentByName11.SetCallbackParam(storeProduct.productIdentifier);
				componentByName11.collider.enabled = true;
			}
		}
		return true;
	}

	private bool SetupClayPanelWithProduct(GameObject panel, StoreProductVO storeProduct, ProductVO clayJamProduct)
	{
		if (clayJamProduct == null)
		{
			Debug.LogWarning(string.Format("ProductIdentifier: {0}, not reconised. Something has been entered wrong on the server", storeProduct.productIdentifier));
			return false;
		}
		LocalisableText componentByName = GetComponentByName<LocalisableText>(panel, "Description");
		if (componentByName != null)
		{
			componentByName.text = storeProduct.title.ToUpper();
		}
		UISprite componentByName2 = GetComponentByName<UISprite>(panel, "Icon");
		if (componentByName2 != null)
		{
			componentByName2.spriteName = clayJamProduct._IconName;
		}
		UILabel componentByName3 = GetComponentByName<UILabel>(panel, "Amount");
		if (componentByName3 != null)
		{
			componentByName3.text = clayJamProduct.QuantityText;
		}
		LocalisableText componentByName4 = GetComponentByName<LocalisableText>(panel, "Cost");
		if (componentByName4 != null)
		{
			componentByName4.text = storeProduct.formattedPrice;
		}
		Transform componentByName5 = GetComponentByName<Transform>(panel, "SaleBadge");
		if (componentByName5 != null)
		{
			componentByName5.gameObject.SetActiveRecursively(clayJamProduct._IsSaleItem);
		}
		UIButtonEvent componentByName6 = GetComponentByName<UIButtonEvent>(panel, "BuyButton");
		if ((bool)componentByName6)
		{
			componentByName6.SetCallbackParam(storeProduct.productIdentifier);
			componentByName6.collider.enabled = true;
		}
		return true;
	}

	private T GetComponentByName<T>(GameObject panel, string name) where T : Component
	{
		Transform transform = panel.transform.FindChild(name);
		T val = ((!(transform != null)) ? ((T)null) : transform.GetComponent<T>());
		if (val == null)
		{
			Debug.LogError(string.Format("GameObject called {0} withh correct component not found on panel", name), panel);
		}
		return val;
	}

	private void OnItemBought(string productIdentifier)
	{
		if (_readyToPurchase)
		{
			FireItemSelectedEvent(productIdentifier);
			ShowMakingPurchaseState();
		}
	}

	private void HookUpBuyButtonTriggers()
	{
		foreach (GameObject clayItemPanel in _ClayItemPanels)
		{
			UIButtonEvent componentByName = GetComponentByName<UIButtonEvent>(clayItemPanel, "BuyButton");
			if ((bool)componentByName)
			{
				componentByName.RegisterCallback(OnItemBought);
			}
		}
		foreach (GameObject upgradeItemPanel in _UpgradeItemPanels)
		{
			UIButtonEvent componentByName2 = GetComponentByName<UIButtonEvent>(upgradeItemPanel, "BuyButton");
			if ((bool)componentByName2)
			{
				componentByName2.RegisterCallback(OnItemBought);
			}
		}
	}

	private void ShowConnectingMessage()
	{
		if (_forcedUserToStore)
		{
			_ConnectingText.text = Localization.instance.Get("IAP_ForcedConnect");
		}
		else
		{
			_ConnectingText.text = Localization.instance.Get("IAP_connecting");
		}
		_forcedUserToStore = false;
		_ConnectingText.Activate(true);
		_ConnectionFailedText.Activate(false);
		_PurchasedFailedText.Activate(false);
		_PurchasedCompleteText.Activate(false);
		_BackButton.SetActiveRecursively(true);
	}

	private void ShowConnectionFailedMessage()
	{
		HidePanels();
		_ConnectingText.Activate(false);
		_ConnectionFailedText.Activate(true);
		_PurchasedFailedText.Activate(false);
		_PurchasedCompleteText.Activate(false);
		_BackButton.SetActiveRecursively(true);
	}

	private void ShowFailedToPurchaseMessage()
	{
		HidePanels();
		_ServerPanelAnim.gameObject.active = true;
		_ConnectingText.Activate(false);
		_PurchasedFailedText.Activate(true);
		_ServerPanelAnim.Play("ServerPanelTransIn");
		_BackButton.SetActiveRecursively(true);
	}

	private void ShowPurchaseCompleteMessage(int clayBought)
	{
		if (!_restoringTransactions)
		{
			if (clayBought > 0)
			{
				string arg = Localization.PunctuatedNumber(clayBought, int.MaxValue);
				_PurchasedCompleteText.text = string.Format(Localization.instance.Get("IAP_complete"), arg);
				_PurchasedCompleteText.Activate(true);
				HidePanels();
				_ServerPanelAnim.gameObject.active = true;
				_ConnectingText.Activate(false);
				_ServerPanelAnim.Play("ServerPanelTransIn");
				_BackButton.SetActiveRecursively(true);
			}
			else
			{
				_ServerPanelAnim.gameObject.active = false;
				_ConnectingText.Activate(false);
				ShowPanels();
			}
		}
	}

	private void HideConnectingMessage()
	{
		_ServerPanelAnim.gameObject.SetActiveRecursively(false);
	}

	private void ShowMakingPurchaseState()
	{
		HidePanels();
		_ServerPanelAnim.gameObject.active = true;
		_ConnectingText.Activate(true);
		_ServerPanelAnim.Play("ServerPanelTransIn");
		_BackButton.SetActiveRecursively(false);
	}

	private void ShowPanels()
	{
		HideConnectingMessage();
		InitItemPanels();
		_BackButton.SetActiveRecursively(true);
	}
}
