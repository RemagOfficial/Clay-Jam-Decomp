using System;

[Serializable]
public class StoreProductVO
{
	public string productIdentifier;

	public string title;

	public string description;

	public string price;

	public string currencySymbol;

	public string currencyCode;

	public string formattedPrice;

	public StoreProductVO()
	{
	}

	public StoreProductVO(GoogleSkuInfo sku)
	{
		productIdentifier = sku.productId;
		title = sku.title;
		description = sku.description;
		price = sku.price;
		currencySymbol = string.Empty;
		currencyCode = string.Empty;
		formattedPrice = price;
		int num = title.IndexOf("(");
		if (num != -1)
		{
			title = title.Remove(num);
		}
	}
}
