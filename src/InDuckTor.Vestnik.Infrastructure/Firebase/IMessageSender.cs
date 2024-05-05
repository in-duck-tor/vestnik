using InDuckTor.Vestnik.Domain;
using InDuckTor.Vestnik.Domain.Messaging;

namespace InDuckTor.Vestnik.Infrastructure.Firebase;

public interface IMessageSender
{
    public readonly record struct SuccessfulDispatch(ClientAppRegistration Registration, string MessageId);

    public readonly record struct BatchSendResult(
        IReadOnlyCollection<SuccessfulDispatch> SuccessfulDispatches,
        IReadOnlyCollection<ClientAppRegistration> FailedDispatchRegistrations,
        IReadOnlyCollection<ClientAppRegistration> UnprocessableRegistrations)
    {
        public BatchSendResult()
            : this(Array.Empty<SuccessfulDispatch>(), Array.Empty<ClientAppRegistration>(), Array.Empty<ClientAppRegistration>()) { }
    }

    public Task<string> SendSimpleNotification(NotificationDataBase notificationData, ClientAppRegistration registration, CancellationToken ct);

    public Task<BatchSendResult> SendSimpleNotification(NotificationDataBase notificationData, IEnumerable<ClientAppRegistration> registrations, CancellationToken ct);

    public Task<BatchSendResult> SendManySimpleNotifications(IEnumerable<(ClientAppRegistration registration, NotificationDataBase data)> notifications, CancellationToken ct);
}