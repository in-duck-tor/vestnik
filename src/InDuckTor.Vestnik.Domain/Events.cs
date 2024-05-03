using InDuckTor.Account.Contracts.Public;

namespace InDuckTor.Vestnik.Domain;

public record AccountCreatedEvent(
    string AccountNumber,
    AccountType AccountType,
    AccountState AccountState,
    int OwnerId,
    int CreatedById,
    (int id, AccountAction[] actions)[] GrantedUsers);

public record AccountUpdatedEvent(string AccountNumber, AccountType AccountType, AccountState AccountState, int ChangedById);

public record TransactionTarget(string AccountNumber, string CurrencyCode, decimal Amount, long BankCode);

public record TransactionCreatedEvent(
    long TransactionId,
    TransactionType Type,
    TransactionStatus Status,
    TransactionTarget DepositOn,
    TransactionTarget WithdrawFrom);

public record TransactionUpdatedEvent(long TransactionId, TransactionType Type, TransactionStatus Status);