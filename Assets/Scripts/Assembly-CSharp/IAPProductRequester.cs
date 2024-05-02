using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IAPProductRequester : MonoBehaviour
{
	private IStoreEventDispatcher _storeEventDispatcher;

	public static IAPProductRequester Instance { get; private set; }

	public List<StoreProductVO> StoreProducts { get; set; }

	public bool HasProductList
	{
		get
		{
			return StoreProducts != null && StoreProducts.Count > 0;
		}
	}

	public bool StoreProductsCannotBeRetrieved { get; private set; }

	[method: MethodImpl(32)]
	public static event Action<int> IAPProductsReceived;

	[method: MethodImpl(32)]
	public static event Action<string> IAPProductsFailed;

	private void Start()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of IAPProductREquester");
		}
		Instance = this;
		if (!BuildDetails.Instance._HasIAP)
		{
			_storeEventDispatcher = null;
			StoreProducts = null;
			StoreProductsCannotBeRetrieved = true;
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
		_storeEventDispatcher.productListReceived += OnProductListReceived;
		_storeEventDispatcher.productListRequestFailed += OnProductListRequestFailed;
	}

	private void OnDestroy()
	{
		if (BuildDetails.Instance._HasIAP)
		{
			_storeEventDispatcher.productListReceived -= OnProductListReceived;
			_storeEventDispatcher.productListRequestFailed -= OnProductListRequestFailed;
		}
	}

	public void ForceReconnect()
	{
		if (BuildDetails.Instance._HasIAP)
		{
			StoreProducts = null;
		}
	}

	public void RequestProducts()
	{
		if (BuildDetails.Instance._HasIAP)
		{
			StoreProducts = null;
			StoreProductsCannotBeRetrieved = false;
			if (IAPGlobal.Instance._Products.Count > 0)
			{
				IAPGlobal.Instance.Store.requestProductData(IAPGlobal.Instance.ProductIDList);
			}
		}
	}

	private void OnProductListReceived(List<StoreProductVO> productList)
	{
		StoreProducts = productList;
		if (IAPProductRequester.IAPProductsReceived != null)
		{
			IAPProductRequester.IAPProductsReceived(productList.Count);
		}
	}

	private void OnProductListRequestFailed(string response)
	{
		StoreProductsCannotBeRetrieved = true;
		if (IAPProductRequester.IAPProductsFailed != null)
		{
			IAPProductRequester.IAPProductsFailed(response);
		}
	}
}
