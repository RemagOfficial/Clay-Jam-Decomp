using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class GooglePlayStore : IStore, IStoreEventDispatcher
{
	private bool _canMakePayments;

	private List<StoreProductVO> _productsOnTheStore;

	[method: MethodImpl(32)]
	public event Action<TransactionVO> purchaseAwaitingConfirmation;

	[method: MethodImpl(32)]
	public event Action<List<StoreProductVO>> productListReceived;

	[method: MethodImpl(32)]
	public event Action<string> productListRequestFailed;

	[method: MethodImpl(32)]
	public event Action<string> purchaseFailed;

	[method: MethodImpl(32)]
	public event Action<string> purchaseCancelled;

	[method: MethodImpl(32)]
	public event Action<string> restoreTransactionsFailed;

	[method: MethodImpl(32)]
	public event Action restoreTransactionsFinished;

	public GooglePlayStore()
	{
		string publicKey = string.Empty;
		if (BuildDetails.Instance.IAPProvider == IAPGlobal.IAPProvider.GOOGLE_FINAL)
		{
			publicKey = IAPGlobal.Instance.AndroidFinalPublicKey;
		}
		else if (BuildDetails.Instance.IAPProvider == IAPGlobal.IAPProvider.GOOGLE_TEST)
		{
			publicKey = IAPGlobal.Instance._AndroidTestPublicKey;
		}
		GoogleIAB.init(publicKey);
		GoogleIAB.enableLogging(false);
		GoogleIAB.setAutoVerifySignatures(true);
		_productsOnTheStore = new List<StoreProductVO>();
		GoogleIABManager.billingSupportedEvent += BillingIsSupportedEventMapping;
		GoogleIABManager.billingNotSupportedEvent += BillingNotSupportedEventMapping;
		GoogleIABManager.queryInventorySucceededEvent += QueryInventorySucceededEventMapping;
		GoogleIABManager.queryInventoryFailedEvent += QueryInventoryFailedEventMapping;
		GoogleIABManager.purchaseSucceededEvent += PurchaseSucceededEventMapping;
		GoogleIABManager.purchaseFailedEvent += PurchaseFailedEventMapping;
		GoogleIABManager.consumePurchaseSucceededEvent += ConsumePurchaseSucceededEventMapping;
		GoogleIABManager.consumePurchaseFailedEvent += ConsumePurchaseFailedEventMapping;
	}

	~GooglePlayStore()
	{
		GoogleIABManager.billingSupportedEvent -= BillingIsSupportedEventMapping;
		GoogleIABManager.billingNotSupportedEvent -= BillingNotSupportedEventMapping;
		GoogleIABManager.queryInventorySucceededEvent -= QueryInventorySucceededEventMapping;
		GoogleIABManager.queryInventoryFailedEvent -= QueryInventoryFailedEventMapping;
		GoogleIABManager.purchaseSucceededEvent -= PurchaseSucceededEventMapping;
		GoogleIABManager.purchaseFailedEvent -= PurchaseFailedEventMapping;
		GoogleIABManager.consumePurchaseSucceededEvent -= ConsumePurchaseSucceededEventMapping;
		GoogleIABManager.consumePurchaseFailedEvent -= ConsumePurchaseFailedEventMapping;
	}

	private void BillingIsSupportedEventMapping()
	{
		_canMakePayments = true;
	}

	private void BillingNotSupportedEventMapping(string reason)
	{
		_canMakePayments = false;
	}

	private void QueryInventorySucceededEventMapping(List<GooglePurchase> purchases, List<GoogleSkuInfo> skus)
	{
		_productsOnTheStore.Clear();
		foreach (GoogleSkuInfo sku in skus)
		{
			_productsOnTheStore.Add(new StoreProductVO(sku));
		}
		RestorePurchases(purchases);
		if (this.productListReceived != null)
		{
			this.productListReceived(_productsOnTheStore);
		}
	}

	private void RestorePurchases(List<GooglePurchase> purchases)
	{
		GooglePurchase purchase;
		foreach (GooglePurchase purchase2 in purchases)
		{
			purchase = purchase2;
			ProductVO productVO = IAPGlobal.Instance._Products.Find((ProductVO p) => p._StoreId == purchase.productId);
			if (productVO != null && productVO._Type == ProductTypeEnum.ProductType.NonConsumable)
			{
				productVO.HonourPurchase();
			}
		}
	}

	private void QueryInventoryFailedEventMapping(string error)
	{
		_productsOnTheStore.Clear();
		_canMakePayments = false;
		if (this.productListRequestFailed != null)
		{
			this.productListRequestFailed(error);
		}
	}

	private void PurchaseSucceededEventMapping(GooglePurchase purchase)
	{
		if (this.purchaseAwaitingConfirmation != null)
		{
			this.purchaseAwaitingConfirmation(new TransactionVO(purchase));
		}
	}

	private void PurchaseFailedEventMapping(string error)
	{
		if (this.purchaseFailed != null)
		{
			this.purchaseFailed(error);
		}
	}

	private void ConsumePurchaseSucceededEventMapping(GooglePurchase purchase)
	{
	}

	private void ConsumePurchaseFailedEventMapping(string error)
	{
	}

	public bool canMakePayments()
	{
		return _canMakePayments;
	}

	public void requestProductData(string[] productIdentifiers)
	{
		_productsOnTheStore.Clear();
		if (_canMakePayments)
		{
			GoogleIAB.queryInventory(productIdentifiers);
		}
		else if (this.productListRequestFailed != null)
		{
			this.productListRequestFailed("IAB not available");
		}
	}

	public void purchaseProduct(string productIdentifier, int quantity)
	{
		GoogleIAB.purchaseProduct(productIdentifier);
	}

	public void confirmPurchase(ProductVO product, string transactionID)
	{
		if (product._Type == ProductTypeEnum.ProductType.Clay)
		{
			GoogleIAB.consumeProduct(product._StoreId);
		}
	}

	public void restoreCompletedTransactions()
	{
	}

	public List<TransactionVO> getAllSavedTransactions()
	{
		return null;
	}

	public bool DebugForceReceiptValidation(out bool valid)
	{
		valid = false;
		return false;
	}
}
