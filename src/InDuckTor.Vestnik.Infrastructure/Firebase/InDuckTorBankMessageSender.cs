using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Infrastructure.Firebase;

public record NotificationDataBase(string Title, string Body, string? ImageUrl = null);

public interface IInDuckTorBankMessageSender
{
    public Task<string> SendSimpleNotification(NotificationDataBase notificationData, string registration, CancellationToken ct);

    public Task<(
            ICollection<string> successMessageIds,
            ICollection<string> failedMessageIds,
            ICollection<string> unprocessableMessageIds)>
        SendSimpleNotification(NotificationDataBase notificationData, IEnumerable<string> registrations, CancellationToken ct);
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
            Notification = new()
            {
                Title = notificationData.Title,
                Body = notificationData.Body,
                ImageUrl = notificationData.ImageUrl
            }
        }, ct);

        return messageId;
    }

    public async Task<(
            ICollection<string> successMessageIds,
            ICollection<string> failedMessageIds,
            ICollection<string> unprocessableMessageIds)>
        SendSimpleNotification(NotificationDataBase notificationData, IEnumerable<string> registrations, CancellationToken ct)
    {
        var multicastMessage = new MulticastMessage
        {
            Tokens = registrations.ToList(),
            Notification = new()
            {
                Title = notificationData.Title,
                Body = notificationData.Body,
                ImageUrl = notificationData.ImageUrl
            }
        };
        var batchResponse = await _firebaseMessaging.SendEachForMulticastAsync(multicastMessage, ct);

        var successMessageIds = new List<string>();
        var failedMessageIds = new List<string>();
        var unprocessableMessageIds = new List<string>();

        for (var i = 0; i < batchResponse.Responses.Count; ++i)
        {
            if (batchResponse.Responses[i].IsSuccess)
            {
                successMessageIds.Add(multicastMessage.Tokens[i]);
                continue;
            }

            if (batchResponse.Responses[i].Exception is { MessagingErrorCode: MessagingErrorCode.Unregistered or MessagingErrorCode.SenderIdMismatch }) // todo : maybe other reasons 
            {
                unprocessableMessageIds.Add(multicastMessage.Tokens[i]);
            }
            else
            {
                failedMessageIds.Add(multicastMessage.Tokens[i]);
            }

            _logger.LogDebug(batchResponse.Responses[i].Exception, "Не удалось отправить сообщение в Firebase по токену {RegistrationTopic}", multicastMessage.Tokens[i]);
        }

        _logger.LogInformation("Результат отправки сообщений в Firebase : удачно - {SuccessCount}; с ошибкой - {FailureCount}", batchResponse.SuccessCount, batchResponse.FailureCount);

        return (successMessageIds, failedMessageIds, unprocessableMessageIds);
    }
}