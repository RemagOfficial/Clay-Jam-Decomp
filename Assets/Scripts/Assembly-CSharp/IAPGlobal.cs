using System.Collections.Generic;
using UnityEngine;

public class IAPGlobal : MonoBehaviour
{
	public enum IAPProvider
	{
		NONE = 0,
		DEBUG = 1,
		APPLE_TEST = 2,
		APPLE_FINAL = 3,
		GOOGLE_TEST = 4,
		GOOGLE_FINAL = 5
	}

	public string _AppleReceiptServer = "https://buy.itunes.apple.com/verifyReceipt";

	public string _AppleReceiptServerTest = "https://sandbox.itunes.apple.com/verifyReceipt";

	public List<ProductVO> _Products;

	private string[] _productIDList;

	public static IAPGlobal Instance { get; private set; }

	public string _AndroidTestPublicKey { get; set; }

	public IStore Store { get; private set; }

	public string ReceiptServer
	{
		get
		{
			string result = _AppleReceiptServerTest;
			switch (BuildDetails.Instance.IAPProvider)
			{
			case IAPProvider.APPLE_TEST:
				result = _AppleReceiptServerTest;
				break;
			case IAPProvider.APPLE_FINAL:
				result = _AppleReceiptServer;
				break;
			case IAPProvider.NONE:
			case IAPProvider.DEBUG:
			case IAPProvider.GOOGLE_TEST:
			case IAPProvider.GOOGLE_FINAL:
				result = string.Empty;
				break;
			}
			return result;
		}
	}

	public string AndroidFinalPublicKey
	{
		get
		{
			return string.Format("{3}{1}{4}{0}{2}", MetaGameController.Instance._Mike, Localization.instance._Chris, FrontendController.Instance._Ciaran, InGameController.Instance._George, CurrentHill.Instance._Iain);
		}
	}

	public string[] ProductIDList
	{
		get
		{
			if (_productIDList == null)
			{
				CreateProductIDList();
			}
			return _productIDList;
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of IAPGlobal");
		}
		Instance = this;
		Store = StoreFactory.GetStoreImplementation();
	}

	private void CreateProductIDList()
	{
		_productIDList = new string[_Products.Count];
		for (int i = 0; i < _productIDList.Length; i++)
		{
			_productIDList[i] = _Products[i]._StoreId;
		}
	}
}
