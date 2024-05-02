using System.Collections.Generic;

public interface IStore
{
	bool canMakePayments();

	void requestProductData(string[] productIdentifiers);

	void purchaseProduct(string productIdentifier, int quantity);

	void confirmPurchase(ProductVO product, string transactionID);

	void restoreCompletedTransactions();

	List<TransactionVO> getAllSavedTransactions();

	bool DebugForceReceiptValidation(out bool valid);
}
