﻿using InDuckTor.Account.HttpClient;
using InDuckTor.Shared.Strategies;
using LazyCache;
using TimeSpan = System.TimeSpan;

namespace InDuckTor.Vestnik.Features.Account;

public readonly record struct GetUserAccountsArgs(int UserId);

public interface IUserAccountsQuery : IQuery<GetUserAccountsArgs, IEnumerable<AccountDto>>;

public class UserAccountsQuery(IAccountClient accountClient, IAppCache cache) : IUserAccountsQuery
{
    private const double CacheExpirationMinutes = 1;

    public async Task<IEnumerable<AccountDto>> Execute(GetUserAccountsArgs args, CancellationToken ct)
    {
        return await cache.GetOrAddAsync(CreateUsersAccountsCacheKey(args.UserId), 
            async () =>
            {
                var accounts = await accountClient.SearchAccountsAsync(
                    new() { OwnerId = args.UserId, Take = 1000 }, ct);
                return accounts.Items ?? Array.Empty<AccountDto>();
            },
            TimeSpan.FromMinutes(CacheExpirationMinutes));
    }

    // todo add cache invalidation on account events
    private static string CreateUsersAccountsCacheKey(int userId) => $"user/{userId}/accounts";
}