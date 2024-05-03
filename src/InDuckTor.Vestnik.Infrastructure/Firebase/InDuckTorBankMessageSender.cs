using FirebaseAdmin.Messaging;
using InDuckTor.Vestnik.Domain.Messaging;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Infrastructure.Firebase;

public interface IInDuckTorBankMessageSender
{
    public Task<string> SendSimpleNotification(NotificationDataBase notificationData, string registration, CancellationToken ct);

    public Task<(
            IReadOnlyCollection<string> successMessageIds,
            IReadOnlyCollection<string> failedMessageIds,
            IReadOnlyCollection<string> unprocessableMessageIds)>
        SendSimpleNotification(NotificationDataBase notificationData, IEnumerable<string> registrations, CancellationToken ct);

    public Task<(
            IReadOnlyCollection<string> successMessageIds,
            IReadOnlyCollection<string> failedMessageIds,
            IReadOnlyCollection<string> unprocessableMessageIds)>
        SendManySimpleNotifications(IEnumerable<(string registration, NotificationDataBase data)> notifications, CancellationToken ct);
}

internal class InDuckTorBankFirebaseMessageSender : IInDuckTorBankMessageSender
{
    private readonly InDuckTorBankFirebaseApp _firebaseApp;
    private readonly ILogger<InDuckTorBankFirebaseMessageSender> _logger;
    private readonly FirebaseMessaging _firebaseMessaging;

    public InDuckTorBankFirebaseMessageSender(InDuckTorBankFirebaseApp firebaseApp, ILogger<InDuckTorBankFirebaseMessageSender> logger)
    {
        _firebaseApp = firebaseApp;
        _logger = logger;
        _firebaseMessaging = FirebaseMessaging.GetMessaging(firebaseApp.App);
    }

    public async Task<string> SendSimpleNotification(NotificationDataBase notificationData, string registration, CancellationToken ct)
    {
        var messageId = await _firebaseMessaging.SendAsync(new()
        {
            Token = registration,
            Notification = CreateNotification(notificationData)
        }, ct);

        return messageId;
    }

    public async Task<(
            IReadOnlyCollection<string> successMessageIds,
            IReadOnlyCollection<string> failedMessageIds,
            IReadOnlyCollection<string> unprocessableMessageIds)>
        SendSimpleNotification(NotificationDataBase notificationData, IEnumerable<string> registrations, CancellationToken ct)
    {
        var multicastMessage = new MulticastMessage
        {
            Tokens = registrations.ToList(),
            Notification = CreateNotification(notificationData)
        };
        var batchResponse = await _firebaseMessaging.SendEachForMulticastAsync(multicastMessage, ct);

        return UnwrapBatchResponse(batchResponse, multicastMessage.Tokens);
    }

    public async Task<(
            IReadOnlyCollection<string> successMessageIds,
            IReadOnlyCollection<string> failedMessageIds,
            IReadOnlyCollection<string> unprocessableMessageIds)>
        SendManySimpleNotifications(IEnumerable<(string registration, NotificationDataBase data)> notifications, CancellationToken ct)
    {
        var notificationsCollection = notifications as IReadOnlyCollection<(string registration, NotificationDataBase data)> ?? notifications.ToList();
        var batchResponse = await _firebaseMessaging.SendEachAsync(
            notificationsCollection.Select(x => new Message
            {
                Token = x.registration,
                Notification = CreateNotification(x.data)
            }),
            ct);

        return UnwrapBatchResponse(batchResponse, notificationsCollection.Select(x => x.registration));
    }

    private static Notification CreateNotification(NotificationDataBase data) => new()
    {
        Title = data.Title,
        Body = data.Body,
        ImageUrl = data.ImageUrl
    };

    private (
        IReadOnlyCollection<string> successMessageIds,
        IReadOnlyCollection<string> failedMessageIds,
        IReadOnlyCollection<string> unprocessableMessageIds)
        UnwrapBatchResponse(BatchResponse batchResponse, IEnumerable<string> requestRegistrations)
    {
        List<string>? successMessageIds = null;
        List<string>? failedMessageIds = null;
        List<string>? unprocessableMessageIds = null;

        foreach (var (response, registration) in batchResponse.Responses.Zip(requestRegistrations))
        {
            if (response.IsSuccess)
            {
                AddRegistration(ref successMessageIds, registration);
                continue;
            }

            if (response.Exception is { MessagingErrorCode: MessagingErrorCode.Unregistered or MessagingErrorCode.SenderIdMismatch }) // todo : maybe other reasons 
            {
                AddRegistration(ref unprocessableMessageIds, registration);
            }
            else
            {
                AddRegistration(ref failedMessageIds, registration);
            }

            _logger.LogDebug(response.Exception, "Не удалось отправить сообщение в Firebase по токену {RegistrationTopic}", registration);
        }

        _logger.LogInformation("Результат отправки сообщений в Firebase : удачно - {SuccessCount}; с ошибкой - {FailureCount}", batchResponse.SuccessCount, batchResponse.FailureCount);

        return (successMessageIds ?? (IReadOnlyCollection<string>)Array.Empty<string>(), failedMessageIds ?? (IReadOnlyCollection<string>)Array.Empty<string>(), unprocessableMessageIds ?? (IReadOnlyCollection<string>)Array.Empty<string>());

        void AddRegistration(ref List<string>? collection, string registration)
        {
            collection ??= new();
            collection.Add(registration);
        }
    }
}