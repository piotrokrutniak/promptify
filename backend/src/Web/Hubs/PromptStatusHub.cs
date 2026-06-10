using System.Security.Claims;
using PromptifyWebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace PromptifyWebApi.Web.Hubs;

[Authorize]
public class PromptStatusHub : Hub
{
    private readonly IApplicationDbContext _context;

    public PromptStatusHub(IApplicationDbContext context)
    {
        _context = context;
    }

    private string? GetUserId() =>
        Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task JoinSession(int sessionId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("Unauthorized.");
        }

        var ownsSession = await _context.Sessions
            .AnyAsync(s => s.Id == sessionId && s.UserId == userId);

        if (!ownsSession)
        {
            throw new HubException("Session not found or access denied.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, SessionGroup(sessionId));
    }

    public Task LeaveSession(int sessionId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, SessionGroup(sessionId));

    internal static string SessionGroup(int sessionId) => $"session-{sessionId}";
}
