using System.Security.Claims;
using InDuckTor.Shared.Security;
using Microsoft.AspNetCore.SignalR;

namespace InDuckTor.Vestnik.Api.Services;

public class InDuckTorUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) => connection.User.FindFirstValue(InDuckTorClaims.Id);
}