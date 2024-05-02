using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugStoreData : MonoBehaviour
{
	public List<StoreProductVO> _ProductList;

	public bool _CanConnect = true;

	public bool _ReceiptsAreValid = true;

	public bool _NextPurchaseWillSucceed = true;

	public bool _NextPurchaseWillCancel;

	private Action _callBack;

	private float _timeForCallback;

	private void Update()
	{
		if (_callBack != null && Time.time > _timeForCallback)
		{
			_callBack();
			_callBack = null;
		}
	}

	public void DoTimedCallback(Action callBack, float delay)
	{
		_callBack = callBack;
		_timeForCallback = Time.time + delay;
	}
}
