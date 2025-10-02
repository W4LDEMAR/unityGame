[System.Serializable]
public class TransactionModel
{
    public enum TransactionType
    {
        Deposit,
        Withdrawal
    }
    
    public TransactionType Type;
    public int Amount;
    public int NewBalance;
    public System.DateTime Timestamp;
    
    public TransactionModel(TransactionType type, int amount, int newBalance)
    {
        Type = type;
        Amount = amount;
        NewBalance = newBalance;
        Timestamp = System.DateTime.Now;
    }
}