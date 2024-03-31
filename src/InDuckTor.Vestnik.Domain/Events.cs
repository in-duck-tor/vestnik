namespace InDuckTor.Vestnik.Domain;

public record AccountCreatedEvent(string AccountNumber, string AccountType, string AccountStatus, int OwnerId, int CreatedById, (int id, string[] actions)[] GrantedUsers);

public record AccountUpdatedEvent(string AccountNumber, string AccountType, string AccountStatus, int ChangedById);

public record TransactionTarget(string AccountNumber, string CurrencyCode, decimal Amount, long BankCode);

public record TransactionCreatedEvent(long TransactionId, string Type, string Status, TransactionTarget DepositOn, TransactionTarget WithdrawFrom);

public record TransactionUpdatedEvent(long TransactionId, string Type, string Status);