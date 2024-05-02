public class TransactionVO
{
	public string productIdentifier;

	public string transactionIdentifier;

	public string base64EncodedTransactionReceipt;

	public int quantity;

	public TransactionVO()
	{
	}

	public TransactionVO(GooglePurchase purchase)
	{
		productIdentifier = purchase.productId;
		transactionIdentifier = purchase.orderId;
		base64EncodedTransactionReceipt = "USING PRIME31 AUTO VERIFY";
		quantity = 1;
	}
}
