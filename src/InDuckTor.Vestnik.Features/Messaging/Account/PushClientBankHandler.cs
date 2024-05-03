//todo
//using FluentResults;
// using InDuckTor.Shared.Strategies;
// using InDuckTor.Vestnik.Domain;
// using InDuckTor.Vestnik.Infrastructure.Database;
// using InDuckTor.Vestnik.Infrastructure.Firebase;
// using Microsoft.EntityFrameworkCore;
//
// namespace InDuckTor.Vestnik.Features.Messaging.Account;
//
// public class ClientBankPushNotificationHandler :
//     IMulticastCommandHandler<AccountCreatedEvent>,
//     IMulticastCommandHandler<AccountUpdatedEvent>
// {
//     private readonly IInDuckTorBankMessageSender _messageSender;
//     private readonly VestnikDbContext _dbContext;
//
//     public ClientBankPushNotificationHandler(IInDuckTorBankMessageSender messageSender, VestnikDbContext dbContext)
//     {
//         _messageSender = messageSender;
//         _dbContext = dbContext;
//     }
//
//     public async Task<Result> Execute(AccountCreatedEvent @event, CancellationToken ct)
//     {
//         var relatedUsers = @event.GrantedUsers.Select(x => x.id).Append(@event.OwnerId).Append(@event.CreatedById).Distinct();
//
//         var registrations = await _dbContext.ClientAppRegistrations
//             .Where(x => x.UserId != null && relatedUsers.Contains(x.UserId.Value))
//             .AsNoTracking()
//             .ToListAsync(ct);
//
//         var (successMessageIds, failedMessageIds, unprocessableMessageIds)
//             = await _messageSender.SendSimpleNotification(
//                 new NotificationDataBase(""),
//                 registrations.Select(x => x.RegistrationToken), ct);
//     }
//
//     private NotificationDataBase CreateNotification(AccountCreatedEvent @event)
//     {
//         
//     }
//
//     public Task<Result> Execute(AccountUpdatedEvent @event, CancellationToken ct)
//     {
//         throw new NotImplementedException();
//     }
// }