using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PurchasesFlags
{
	private const string ProductID_Cupid = "com.zynga.fatpebble.clayjam.cupidupgrade";

	private Hashtable _hashTable;

	public void SetInitialState()
	{
		Dictionary<string, string> d = new Dictionary<string, string>();
		_hashTable = new Hashtable(d);
		for (int i = 0; i < IAPGlobal.Instance._Products.Count; i++)
		{
			if (IAPGlobal.Instance._Products[i]._IsActive && IAPGlobal.Instance._Products[i]._Type == ProductTypeEnum.ProductType.NonConsumable)
			{
				string storeId = IAPGlobal.Instance._Products[i]._StoreId;
				_hashTable.Add(storeId, "0");
			}
		}
	}

	public Hashtable GetHash()
	{
		return _hashTable;
	}

	public void Decode(Hashtable hashtable, Version version, bool adding)
	{
		for (int i = 0; i < IAPGlobal.Instance._Products.Count; i++)
		{
			if (IAPGlobal.Instance._Products[i]._IsActive && IAPGlobal.Instance._Products[i]._Type == ProductTypeEnum.ProductType.NonConsumable)
			{
				string storeId = IAPGlobal.Instance._Products[i]._StoreId;
				if (_hashTable.Contains(storeId))
				{
					_hashTable[storeId] = hashtable[storeId];
				}
				else
				{
					_hashTable.Add(storeId, hashtable[storeId]);
				}
			}
		}
	}

	public bool IsPurchased(string productID)
	{
		if (_hashTable.Contains(productID))
		{
			return (string)_hashTable[productID] == "1";
		}
		return false;
	}

	public void SetPurchased(string productID)
	{
		if (_hashTable.Contains(productID))
		{
			_hashTable[productID] = "1";
		}
		else
		{
			_hashTable.Add(productID, "1");
		}
	}

	public float GetCupidBonus()
	{
		if (IsPurchased("com.zynga.fatpebble.clayjam.cupidupgrade"))
		{
			return 2f;
		}
		return 1f;
	}
}
