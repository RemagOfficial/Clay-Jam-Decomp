using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class IAPItemSelector : MonoBehaviour
{
	public abstract bool IsAwake { get; }

	[method: MethodImpl(32)]
	public event Action AwakeEvent;

	[method: MethodImpl(32)]
	public event Action<string> ItemSelectedEvent;

	public virtual void ShowConnecting()
	{
	}

	public abstract void Init(List<StoreProductVO> storeProducts);

	public virtual void FailedToConnect()
	{
	}

	public virtual void FailedToPurchase()
	{
	}

	public virtual void PurchaseCancelled()
	{
	}

	public virtual void PurchaseComplete(int clayBought)
	{
	}

	public virtual void RestoreTransactionsFailed()
	{
	}

	public virtual void RestoreTransactionsComplete()
	{
	}

	protected void FireAwakeEvent()
	{
		if (this.AwakeEvent != null)
		{
			this.AwakeEvent();
		}
	}

	protected void FireItemSelectedEvent(string productIdentifier)
	{
		if (this.ItemSelectedEvent != null)
		{
			this.ItemSelectedEvent(productIdentifier);
		}
	}
}
