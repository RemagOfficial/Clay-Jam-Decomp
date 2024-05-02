using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class IAPPanelController : MonoBehaviour
{
	private enum State
	{
		Inactive = 0,
		Active = 1,
		Waiting = 2
	}

	private IStoreEventDispatcher _storeEventDispatcher;

	private IAPItemSelector _itemSelector;

	private List<TransactionVO> _transactionsToValidate;

	private static State CurrentState { get; set; }

	[method: MethodImpl(32)]
	public static event Action IAPProductsSeen;

	[method: MethodImpl(32)]
	public static event Action<string> IAPPurchaseRequest;

	[method: MethodImpl(32)]
	public static event Action<string> IAPPurchaseFailed;

	[method: MethodImpl(32)]
	public static event Action<string> IAPPurchaseCancelled;

	[method: MethodImpl(32)]
	public static event Action<string> IAPPurchaseComplete;

	private void Awake()
	{
		if (!BuildDetails.Instance._HasIAP)
		{
			Debug.LogError("IAP Panel should not be accessible for non IAP builds", base.gameObject);
			return;
		}
		if (IAPGlobal.Instance.Store is IStoreEventDispatcher)
		{
			_storeEventDispatcher = (IStoreEventDispatcher)IAPGlobal.Instance.Store;
		}
		else
		{
			_storeEventDispatcher = StoreFactory.GetStoreEventDispatcher();
		}
		_transactionsToValidate = new List<TransactionVO>();
	}

	private void OnEnable()
	{
		_storeEventDispatcher.purchaseAwaitingConfirmation += OnPurchaseAwaitingConfirmation;
		_storeEventDispatcher.purchaseCancelled += OnPurchaseCancelled;
		_storeEventDispatcher.purchaseFailed += OnPurchaseFailed;
		_storeEventDispatcher.restoreTransactionsFailed += OnRestoreTransactionsFailed;
		_storeEventDispatcher.restoreTransactionsFinished += OnRestoreTransactionsFinished;
		IAPProductRequester.IAPProductsReceived += OnProductsReceived;
		IAPProductRequester.IAPProductsFailed += OnProductsFailed;
		if (_itemSelector == null)
		{
			_itemSelector = GetComponent<IAPItemSelector>();
		}
		if (!(_itemSelector != null))
		{
			return;
		}
		_itemSelector.ItemSelectedEvent += OnItemSelected;
		if (IAPGlobal.Instance.Store.canMakePayments())
		{
			if (_itemSelector.IsAwake)
			{
				ShowProducts();
			}
			else
			{
				_itemSelector.AwakeEvent += OnItemScrollerAwake;
			}
		}
		else
		{
			CurrentState = State.Inactive;
			DisplayUnableToMakePurchases();
		}
	}

	private void OnDisable()
	{
		_storeEventDispatcher.purchaseAwaitingConfirmation -= OnPurchaseAwaitingConfirmation;
		_storeEventDispatcher.purchaseCancelled -= OnPurchaseCancelled;
		_storeEventDispatcher.purchaseFailed -= OnPurchaseFailed;
		_storeEventDispatcher.restoreTransactionsFailed -= OnRestoreTransactionsFailed;
		_storeEventDispatcher.restoreTransactionsFinished -= OnRestoreTransactionsFinished;
		IAPProductRequester.IAPProductsReceived -= OnProductsReceived;
		IAPProductRequester.IAPProductsFailed -= OnProductsFailed;
		if (_itemSelector != null)
		{
			_itemSelector.ItemSelectedEvent -= OnItemSelected;
			_itemSelector.AwakeEvent -= OnItemScrollerAwake;
		}
		CurrentState = State.Inactive;
	}

	private void OnItemScrollerAwake()
	{
		_itemSelector.AwakeEvent -= OnItemScrollerAwake;
		ShowProducts();
	}

	private void PurchaseProduct(string productIdentifier)
	{
		CurrentState = State.Waiting;
		IAPGlobal.Instance.Store.purchaseProduct(productIdentifier, 1);
		if (IAPPanelController.IAPPurchaseRequest != null)
		{
			IAPPanelController.IAPPurchaseRequest(productIdentifier);
		}
	}

	private void DisplayUnableToMakePurchases()
	{
		FailedToGetProducst();
	}

	private void OnItemSelected(string productIdentifier)
	{
		PurchaseProduct(productIdentifier);
	}

	private void OnProductsReceived(int count)
	{
		if (CurrentState == State.Waiting)
		{
			ShowProducts();
		}
	}

	private void ShowProducts()
	{
		if (_itemSelector == null)
		{
			return;
		}
		if (BuildDetails.Instance._UseLeapIfAvailable)
		{
			FailedToGetProducst();
		}
		else if (IAPProductRequester.Instance.HasProductList)
		{
			ActivateItemSelector();
		}
		else if (CurrentState != State.Waiting)
		{
			IAPProductRequester.Instance.RequestProducts();
			if (IAPProductRequester.Instance.HasProductList)
			{
				ActivateItemSelector();
				return;
			}
			if (IAPProductRequester.Instance.StoreProductsCannotBeRetrieved)
			{
				FailedToGetProducst();
				return;
			}
			_itemSelector.ShowConnecting();
			CurrentState = State.Waiting;
		}
		else
		{
			FailedToGetProducst();
		}
	}

	private void ActivateItemSelector()
	{
		_itemSelector.Init(IAPProductRequester.Instance.StoreProducts);
		CurrentState = State.Active;
		if (IAPPanelController.IAPProductsSeen != null)
		{
			IAPPanelController.IAPProductsSeen();
		}
	}

	private void OnProductsFailed(string response)
	{
		if (CurrentState == State.Waiting)
		{
			FailedToGetProducst();
		}
	}

	private void FailedToGetProducst()
	{
		if (!(_itemSelector == null))
		{
			_itemSelector.FailedToConnect();
			CurrentState = State.Inactive;
		}
	}

	private void OnPurchaseAwaitingConfirmation(TransactionVO transaction)
	{
		_transactionsToValidate.Add(transaction);
		OnReceiptValidated(transaction.transactionIdentifier);
	}

	private void OnReceiptNotValid(string response, string transactionID = null)
	{
		OnPurchaseFailed(response);
		RemoveTransactionFromValidationList(transactionID);
	}

	private void RemoveTransactionFromValidationList(string transactionID)
	{
		if (transactionID == null)
		{
			return;
		}
		foreach (TransactionVO item in _transactionsToValidate)
		{
			if (item.transactionIdentifier == transactionID)
			{
				_transactionsToValidate.Remove(item);
				break;
			}
		}
	}

	private TransactionVO GetTransactionFromValidationList(string transactionID)
	{
		if (transactionID != null)
		{
			foreach (TransactionVO item in _transactionsToValidate)
			{
				if (item.transactionIdentifier == transactionID)
				{
					return item;
				}
			}
		}
		return null;
	}

	private void OnReceiptValidated(string transactionID = null)
	{
		TransactionVO transactionVO = GetTransactionFromValidationList(transactionID);
		if (transactionVO == null)
		{
			return;
		}
		ProductVO productVO = IAPGlobal.Instance._Products.Find((ProductVO item) => item._StoreId == transactionVO.productIdentifier);
		if (productVO != null)
		{
			productVO.HonourPurchase();
			IAPGlobal.Instance.Store.confirmPurchase(productVO, transactionVO.transactionIdentifier);
			if (IAPPanelController.IAPPurchaseComplete != null)
			{
				IAPPanelController.IAPPurchaseComplete(transactionVO.productIdentifier);
			}
			if (_itemSelector != null)
			{
				_itemSelector.PurchaseComplete(productVO._Quantity);
				CurrentState = State.Active;
			}
		}
		else
		{
			OnPurchaseFailed("ProductNotMatched_very_bad");
		}
		RemoveTransactionFromValidationList(transactionID);
	}

	private void OnPurchaseFailed(string response)
	{
		if (IAPPanelController.IAPPurchaseFailed != null)
		{
			IAPPanelController.IAPPurchaseFailed(response);
		}
		IAPProductRequester.Instance.ForceReconnect();
		if (_itemSelector != null)
		{
			_itemSelector.FailedToPurchase();
			CurrentState = State.Inactive;
		}
	}

	private void OnRestoreTransactionsFailed(string response)
	{
		if (_itemSelector != null)
		{
			_itemSelector.RestoreTransactionsFailed();
		}
	}

	private void OnRestoreTransactionsFinished()
	{
		if (_itemSelector != null)
		{
			_itemSelector.RestoreTransactionsComplete();
		}
	}

	private void OnPurchaseCancelled(string response)
	{
		if (IAPPanelController.IAPPurchaseCancelled != null)
		{
			IAPPanelController.IAPPurchaseCancelled(response);
		}
		if (_itemSelector != null)
		{
			_itemSelector.PurchaseCancelled();
			CurrentState = State.Active;
		}
	}

	public void ValidateReceipt(string base64EncodedTransactionReceipt, bool forceThrough = false, bool forceValid = false)
	{
		if (forceThrough)
		{
			StartCoroutine(DummyReceiptCheck(3f, forceValid));
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("receipt-data", base64EncodedTransactionReceipt);
		string receiptJson = MiniJSON.jsonEncode(dictionary);
		PostReceiptCheck(receiptJson);
	}

	private void PostReceiptCheck(string receiptJson)
	{
		string empty = string.Empty;
		empty = IAPGlobal.Instance.ReceiptServer;
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		byte[] bytes = aSCIIEncoding.GetBytes(receiptJson);
		StartCoroutine(PostURL(empty, bytes));
	}

	private IEnumerator PostURL(string url, byte[] postData)
	{
		WWW www = new WWW(url, postData);
		yield return www;
		if (www.error != null)
		{
			OnReceiptNotValid(string.Format("ReceiptPostFailed{0}", www.error));
		}
		else
		{
			ReceiptValidationReceived(www.text);
		}
	}

	private IEnumerator DummyReceiptCheck(float seconds, bool valid)
	{
		yield return new WaitForSeconds(seconds);
		if (valid)
		{
			string goodValidation = string.Format("{{\"status\" : 0, \"receipt\" : {0} }}", string.Format("{{ \"transaction_id\" : \"{0}\" }}", _transactionsToValidate[0].transactionIdentifier));
			ReceiptValidationReceived(goodValidation);
		}
		else
		{
			OnReceiptNotValid("TESTReceiptNotValid");
		}
	}

	private void ReceiptValidationReceived(string response)
	{
		Debug.Log("IAP : Receipt response : " + response);
		Hashtable hashtable = (Hashtable)MiniJSON.jsonDecode(response);
		if (hashtable.ContainsKey("status"))
		{
			double num = (double)hashtable["status"];
			if (num == 0.0)
			{
				if (hashtable.ContainsKey("receipt"))
				{
					Hashtable hashtable2 = (Hashtable)hashtable["receipt"];
					if (hashtable2.ContainsKey("transaction_id"))
					{
						string text = hashtable2["transaction_id"].ToString();
						TransactionVO transactionFromValidationList = GetTransactionFromValidationList(text);
						if (transactionFromValidationList != null)
						{
							if (text.CompareTo(transactionFromValidationList.transactionIdentifier) == 0)
							{
								OnReceiptValidated(text);
							}
							else
							{
								OnReceiptNotValid(string.Format("ReceiptInvalid_TransactionMismatch"), text);
							}
						}
					}
					else
					{
						OnReceiptNotValid(string.Format("ReceiptInvalid_Malformed_Receipt_trans"));
					}
				}
				else
				{
					OnReceiptNotValid(string.Format("ReceiptInvalid_Malformed_Receipt"));
				}
			}
			else
			{
				OnReceiptNotValid(string.Format("ReceiptInvalid{0}", num));
			}
		}
		else
		{
			OnReceiptNotValid("ReceiptInvalid_Malfromed_Status");
		}
	}

	private string ByteArrayToString(byte[] input)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		return uTF8Encoding.GetString(input);
	}

	private string GetSha1(string myString)
	{
		HashAlgorithm hashAlgorithm = new SHA1Managed();
		byte[] bytes = Encoding.Default.GetBytes(myString);
		byte[] input = hashAlgorithm.ComputeHash(bytes);
		return ByteArrayToString(input);
	}

	private byte[] GetSha1Bytes(string myString)
	{
		HashAlgorithm hashAlgorithm = new SHA1Managed();
		byte[] bytes = Encoding.Default.GetBytes(myString);
		return hashAlgorithm.ComputeHash(bytes);
	}

	private bool CompareSignatures(byte[] receivedSignature, byte[] expectedSignature)
	{
		bool result = false;
		if (receivedSignature.Length == expectedSignature.Length)
		{
			int i;
			for (i = 0; i < receivedSignature.Length && receivedSignature[i] == expectedSignature[i]; i++)
			{
			}
			if (i == receivedSignature.Length)
			{
				result = true;
			}
		}
		return result;
	}
}
