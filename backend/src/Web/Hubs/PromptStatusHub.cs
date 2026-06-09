using PromptifyWebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace PromptifyWebApi.Web.Hubs;

[Authorize]
public class PromptStatusHub : Hub
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public PromptStatusHub(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task JoinSession(int sessionId)
    {
        var ownsSession = await _context.Sessions
            .AnyAsync(s => s.Id == sessionId && s.UserId == _user.Id);

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
