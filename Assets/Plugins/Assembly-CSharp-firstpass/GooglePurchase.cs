using System.Collections.Generic;

public class GooglePurchase
{
	public string packageName { get; private set; }

	public string orderId { get; private set; }

	public string productId { get; private set; }

	public string developerPayload { get; private set; }

	public long purchaseTime { get; private set; }

	public int purchaseState { get; private set; }

	public string purchaseToken { get; private set; }

	public GooglePurchase(Dictionary<string, object> dict)
	{
		if (dict.ContainsKey("packageName"))
		{
			packageName = dict["packageName"].ToString();
		}
		if (dict.ContainsKey("orderId"))
		{
			orderId = dict["orderId"].ToString();
		}
		if (dict.ContainsKey("productId"))
		{
			productId = dict["productId"].ToString();
		}
		if (dict.ContainsKey("developerPayload"))
		{
			developerPayload = dict["developerPayload"].ToString();
		}
		if (dict.ContainsKey("purchaseTime"))
		{
			purchaseTime = long.Parse(dict["purchaseTime"].ToString());
		}
		if (dict.ContainsKey("purchaseState"))
		{
			purchaseState = int.Parse(dict["purchaseState"].ToString());
		}
		if (dict.ContainsKey("purchaseToken"))
		{
			purchaseToken = dict["purchaseToken"].ToString();
		}
	}

	public static List<GooglePurchase> fromList(List<object> items)
	{
		List<GooglePurchase> list = new List<GooglePurchase>();
		foreach (Dictionary<string, object> item in items)
		{
			list.Add(new GooglePurchase(item));
		}
		return list;
	}

	public override string ToString()
	{
		return string.Format("<GooglePurchase> packageName: {0}, orderId: {1}, productId: {2}, developerPayload: {3}, purchaseToken: {4}", packageName, orderId, productId, developerPayload, purchaseToken);
	}
}
