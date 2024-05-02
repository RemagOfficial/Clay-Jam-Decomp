using System;

[Serializable]
public class ProductVO
{
	public string _StoreId;

	public ProductTypeEnum.ProductType _Type;

	public int _Quantity;

	public string _IconName;

	public bool _IsSaleItem;

	public float _Price;

	public string _TitleKey;

	public int _ValuePercentage;

	public bool _IsActive;

	public bool _UseCupidPanel;

	public string QuantityText
	{
		get
		{
			return Localization.PunctuatedNumber(_Quantity, int.MaxValue);
		}
	}

	public void HonourPurchase()
	{
		switch (_Type)
		{
		case ProductTypeEnum.ProductType.Clay:
			SaveData.Instance.AddClayToCollection(new ClayData(0, _Quantity));
			break;
		case ProductTypeEnum.ProductType.NonConsumable:
			SaveData.Instance.Purchases.SetPurchased(_StoreId);
			break;
		}
		SaveData.Instance.Save();
	}
}
