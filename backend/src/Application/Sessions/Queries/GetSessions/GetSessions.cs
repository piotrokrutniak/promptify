using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Models;
using PromptifyWebApi.Application.Common.Security;

namespace PromptifyWebApi.Application.Sessions.Queries.GetSessions;

[Authorize]
public record GetSessionsQuery : IRequest<IList<SessionListItemDto>>;

public class SessionListItemDto
{
    public int Id { get; init; }

    public string? Title { get; init; }

    public DateTimeOffset Created { get; init; }

    public DateTimeOffset LastModified { get; init; }
}

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, IList<SessionListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetSessionsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IList<SessionListItemDto>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(_user.Id);

        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == _user.Id)
            .OrderByDescending(s => s.LastModified)
            .Select(s => new SessionListItemDto
            {
                Id = s.Id,
                Title = s.Title,
                Created = s.Created,
                LastModified = s.LastModified
            })
            .ToListAsync(cancellationToken);
    }
}
