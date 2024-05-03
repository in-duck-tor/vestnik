namespace InDuckTor.Vestnik.Domain.Messaging;

public record NotificationDataBase(string Title, string Body, string? ImageUrl = null);