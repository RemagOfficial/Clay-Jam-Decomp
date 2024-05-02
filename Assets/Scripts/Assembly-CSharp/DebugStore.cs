using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using UnityEngine;

public class DebugStore : IStore, IStoreEventDispatcher
{
	private delegate void AsyncCallbackType(object payload);

	private const int ASYNC_SIMULATION_TIME = 3000;

	private Timer _timer;

	private object _callbackPayload;

	private AsyncCallbackType _callback;

	private DebugStoreData _data;

	private string _requestedProductIdentifier;

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

	public DebugStore()
	{
		_timer = new Timer(3000.0);
		_timer.Elapsed += onTimer;
		CreateProducts();
	}

	public bool canMakePayments()
	{
		return true;
	}

	public void requestProductData(string[] productIdentifiers)
	{
		if (_data._CanConnect)
		{
			_data.DoTimedCallback(OnProductListReceived, 1.5f);
		}
		else
		{
			_data.DoTimedCallback(OnConnetionFailed, 1.5f);
		}
	}

	private void OnProductListReceived()
	{
		if (this.productListReceived != null)
		{
			this.productListReceived(_data._ProductList);
		}
	}

	private void OnConnetionFailed()
	{
		if (this.productListRequestFailed != null)
		{
			this.productListRequestFailed("RANDOM LIST REQUEST FAIL");
		}
	}

	public void confirmPurchase(ProductVO product, string transactionID)
	{
	}

	public void purchaseProduct(string productIdentifier, int quantity)
	{
		_requestedProductIdentifier = productIdentifier;
		if (_data._NextPurchaseWillSucceed)
		{
			_data.DoTimedCallback(OnPurchaseSuccesfull, 0.5f);
		}
		else if (_data._NextPurchaseWillCancel)
		{
			_data.DoTimedCallback(OnPurchaseCancelled, 0.5f);
		}
		else
		{
			_data.DoTimedCallback(OnPurchaseFailed, 0.5f);
		}
	}

	private void OnPurchaseSuccesfull()
	{
		string s = "RECEIPTTEST";
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		if (this.purchaseAwaitingConfirmation != null)
		{
			this.purchaseAwaitingConfirmation(new TransactionVO
			{
				productIdentifier = _requestedProductIdentifier,
				transactionIdentifier = "debug-transaction-identifeir",
				quantity = 1,
				base64EncodedTransactionReceipt = Convert.ToBase64String(aSCIIEncoding.GetBytes(s))
			});
		}
	}

	private void OnPurchaseFailed()
	{
		if (this.purchaseFailed != null)
		{
			this.purchaseFailed("RANDOM FAIL");
		}
	}

	private void OnPurchaseCancelled()
	{
		if (this.purchaseCancelled != null)
		{
			this.purchaseCancelled("RANDOM CANCEL");
		}
	}

	public void restoreCompletedTransactions()
	{
		_data.DoTimedCallback(OnRestoreCompletedTransactionsSuccesfull, 5f);
	}

	private void OnRestoreCompletedTransactionsSuccesfull()
	{
		if (this.restoreTransactionsFinished != null)
		{
			this.restoreTransactionsFinished();
		}
	}

	public void validateAutoRenewableReceipt(string base64EncodedTransactionReceipt, string secret, bool isTest)
	{
	}

	public List<TransactionVO> getAllSavedTransactions()
	{
		return null;
	}

	private void CreateProducts()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "DebugIAPStore";
		_data = gameObject.AddComponent<DebugStoreData>();
		_data._ProductList = new List<StoreProductVO>();
		int num = 0;
		foreach (ProductVO product in IAPGlobal.Instance._Products)
		{
			if (product._IsActive)
			{
				StoreProductVO storeProductVO = new StoreProductVO();
				storeProductVO.productIdentifier = product._StoreId;
				float num2 = 1f;
				if (storeProductVO.productIdentifier.Contains("sale"))
				{
					num2 = 0.5f;
					continue;
				}
				num++;
				float num3 = (float)num * num2;
				storeProductVO.description = "DESC AMOUNT" + num;
				storeProductVO.title = "TITLE " + num;
				storeProductVO.formattedPrice = string.Format("${0:0.00}", num3);
				_data._ProductList.Add(storeProductVO);
			}
		}
	}

	public bool DebugForceReceiptValidation(out bool valid)
	{
		valid = _data._ReceiptsAreValid;
		return true;
	}

	private void onTimer(object source, ElapsedEventArgs e)
	{
		_timer.Stop();
		if (_callback != null)
		{
			_callback(_callbackPayload);
		}
		_callback = null;
		_callbackPayload = null;
	}
}
