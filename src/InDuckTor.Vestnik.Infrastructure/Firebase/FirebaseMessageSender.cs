using FirebaseAdmin.Messaging;
using InDuckTor.Vestnik.Domain;
using InDuckTor.Vestnik.Domain.Messaging;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Infrastructure.Firebase;

internal class FirebaseMessageSender : IMessageSender
{
    private readonly InDuckTorBankFirebaseApp _firebaseApp;
    private readonly ILogger<FirebaseMessageSender> _logger;
    private readonly FirebaseMessaging _firebaseMessaging;

    public FirebaseMessageSender(InDuckTorBankFirebaseApp firebaseApp, ILogger<FirebaseMessageSender> logger)
    {
        _firebaseApp = firebaseApp;
        _logger = logger;
        _firebaseMessaging = FirebaseMessaging.GetMessaging(firebaseApp.App);
    }

    public async Task<string> SendSimpleNotification(NotificationDataBase notificationData, ClientAppRegistration registration, CancellationToken ct)
    {
        var messageId = await _firebaseMessaging.SendAsync(new()
        {
            Token = registration.RegistrationToken,
            Notification = CreateNotification(notificationData)
        }, ct);

        return messageId;
    }

    public async Task<IMessageSender.BatchSendResult> SendSimpleNotification(NotificationDataBase notificationData, IEnumerable<ClientAppRegistration> registrations, CancellationToken ct)
    {
        var registrationsCollection = registrations as IReadOnlyCollection<ClientAppRegistration> ?? registrations.ToList(); 
        var multicastMessage = new MulticastMessage
        {
            Tokens = registrationsCollection.Select(x => x.RegistrationToken).ToList(),
            Notification = CreateNotification(notificationData)
        };
        var batchResponse = await _firebaseMessaging.SendEachForMulticastAsync(multicastMessage, ct);

        return UnwrapBatchResponse(batchResponse, registrationsCollection);
    }

    public async Task<IMessageSender.BatchSendResult> SendManySimpleNotifications(IEnumerable<(ClientAppRegistration registration, NotificationDataBase data)> notifications, CancellationToken ct)
    {
        var notificationsCollection = notifications as IReadOnlyCollection<(ClientAppRegistration registration, NotificationDataBase data)> ?? notifications.ToList();
        var batchResponse = await _firebaseMessaging.SendEachAsync(
            notificationsCollection.Select(x => new Message
            {
                Token = x.registration.RegistrationToken,
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

    private IMessageSender.BatchSendResult UnwrapBatchResponse(BatchResponse batchResponse, IEnumerable<ClientAppRegistration> requestRegistrations)
    {
        List<IMessageSender.SuccessfulDispatch>? successfulDispatches = null;
        List<ClientAppRegistration>? failedDispatchRegistrations = null;
        List<ClientAppRegistration>? unprocessableRegistrations = null;

        foreach (var (response, registration) in batchResponse.Responses.Zip(requestRegistrations))
        {
            if (response.IsSuccess)
            {
                AddItem(ref successfulDispatches, new(registration, response.MessageId));
                continue;
            }

            if (response.Exception is { MessagingErrorCode: MessagingErrorCode.Unregistered or MessagingErrorCode.SenderIdMismatch }) // todo : maybe other reasons 
            {
                AddItem(ref failedDispatchRegistrations, registration);
            }
            else
            {
                AddItem(ref unprocessableRegistrations, registration);
            }

            _logger.LogDebug(response.Exception, "Не удалось отправить сообщение в Firebase по токену {RegistrationTopic}", registration);
        }

        _logger.LogInformation("Результат отправки сообщений в Firebase : удачно - {SuccessCount}; с ошибкой - {FailureCount}", batchResponse.SuccessCount, batchResponse.FailureCount);

        return new IMessageSender.BatchSendResult(
            successfulDispatches ?? (IReadOnlyCollection<IMessageSender.SuccessfulDispatch>)Array.Empty<IMessageSender.SuccessfulDispatch>(),
            failedDispatchRegistrations ?? (IReadOnlyCollection<ClientAppRegistration>)Array.Empty<ClientAppRegistration>(),
            unprocessableRegistrations ?? (IReadOnlyCollection<ClientAppRegistration>)Array.Empty<ClientAppRegistration>());


        void AddItem<T>(ref List<T>? collection, T item)
        {
            collection ??= new();
            collection.Add(item);
        }
    }
}