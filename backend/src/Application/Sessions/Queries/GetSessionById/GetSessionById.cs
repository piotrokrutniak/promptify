using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Mappings;
using PromptifyWebApi.Application.Sessions;
using PromptifyWebApi.Application.Common.Models;
using PromptifyWebApi.Application.Common.Security;
using PromptifyWebApi.Domain.Enums;

namespace PromptifyWebApi.Application.Sessions.Queries.GetSessionById;

[Authorize]
public record GetSessionByIdQuery(int SessionId) : IRequest<SessionDto>;

public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, SessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetSessionByIdQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<SessionDto> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await SessionAccess.GetOwnedSessionAsync(
            _context, _user, request.SessionId, cancellationToken);

        var prompts = await _context.Prompts
            .AsNoTracking()
            .Where(p => p.SessionId == session.Id && p.Status != PromptStatus.Cancelled)
            .OrderBy(p => p.OrderIndex)
            .Select(PromptMappings.ToDtoExpression)
            .ToListAsync(cancellationToken);

        return new SessionDto
        {
            Id = session.Id,
            Title = session.Title,
            Created = session.Created,
            Prompts = prompts
        };
    }
}
