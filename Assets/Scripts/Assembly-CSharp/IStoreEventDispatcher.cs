using System;
using System.Collections.Generic;

public interface IStoreEventDispatcher
{
	event Action<TransactionVO> purchaseAwaitingConfirmation;

	event Action<List<StoreProductVO>> productListReceived;

	event Action<string> productListRequestFailed;

	event Action<string> purchaseFailed;

	event Action<string> purchaseCancelled;

	event Action<string> restoreTransactionsFailed;

	event Action restoreTransactionsFinished;
}
