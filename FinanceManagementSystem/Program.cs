using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Security.Cryptography.X509Certificates;

// =========================
// QUESTION 1 - Finance
// =========================

//a. Transaction Record
public record Transaction (int Id, DateTime Date, decimal Amount, string Category);

//b. Implement Payment Behaviour using Interfaces
public interface ITransactionProcessor
{
    void Process(Transaction transaction);
}

//c. creating 3 classes that implement the interface

public class  BankTransferProcessor: ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[Processing Bank Transfer]:  Amount: {transaction.Amount}, Category: {transaction.Category}");
    }
}

public class MobileMoneyProcessor: ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[Processing Mobile Money Transfer]:  Amount: {transaction.Amount}, Category: {transaction.Category}");
    }
}

public class CryptoWalletProcessor: ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[Processing Crypto Wallet]:  Amount: {transaction.Amount}, Category: {transaction.Category} ");
    }
}

//d. Creadting a general account and sealing a specialized account
public class Account
{
    protected string AccountNumber { get; }
   protected  decimal Balance { get; set; }

    public Account(string AccountNumber, decimal initialBalance)
    {
        this.AccountNumber = AccountNumber;
        this.Balance = initialBalance;
    }

    public virtual void ApplyTransaction(Transaction transaction)
    {
        Balance -= transaction.Amount;
        Console.WriteLine($"Account Number: {AccountNumber}, Transaction applied to {transaction.Amount}. New balance: {Balance}");
    }
}

//e. sealed savings account
public sealed class SavingsAccount : Account
{
    public SavingsAccount(string accountNumber, decimal initialBalance)
        : base(accountNumber, initialBalance)
    { }

        public override void ApplyTransaction(Transaction transaction)
    {
        if (transaction.Amount > Balance)
        {
            Console.WriteLine("Insufficient funds");
            return;
        }
            Balance -= transaction.Amount;
            Console.WriteLine($"Savings Account Number: {AccountNumber}, Deducted {transaction.Amount}. New balance: {Balance}");

    }
}

public class FinancialApp
{
    private readonly List<Transaction>_transactions = new();

    public void Run()
    {
        Console.WriteLine("=== Welcome to the Financial App! ===");

        //i. Instantiate SavingsAccount
        var Account = new SavingsAccount("123456789", 1000.00m);

        //ii. Create Transactions
        var _transaction1 = new Transaction(1, DateTime.Now, 100.00m, "Groceries");
        var _transaction2 = new Transaction(2, DateTime.Now, 50.00m, "Utilities");
        var _transaction3 = new Transaction(3, DateTime.Now, 200.00m, "Entertainment");

        //iii. use processors
        ITransactionProcessor bankTransfer = new BankTransferProcessor();
        ITransactionProcessor mobileMoney = new MobileMoneyProcessor();
        ITransactionProcessor cryptoWallet = new CryptoWalletProcessor();

        bankTransfer.Process(_transaction1);
        Account.ApplyTransaction(_transaction1);
        _transactions.Add(_transaction1);

        mobileMoney.Process(_transaction2);
        Account.ApplyTransaction(_transaction2);
        _transactions.Add(_transaction2);

        cryptoWallet.Process(_transaction3);
        Account.ApplyTransaction(_transaction3);
        _transactions.Add(_transaction3);

        Console.WriteLine("\n=== Transaction History ===");
        Console.WriteLine("Total Transactions: " + _transactions.Count);
        Console.WriteLine();
    }

}

public class Program
{
    public static void Main(string[] args)
    {
        var app = new FinancialApp();
        app.Run();
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}