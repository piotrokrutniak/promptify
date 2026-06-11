using MassTransit;
using PromptifyWebApi.Application.Common.Exceptions;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Mappings;
using PromptifyWebApi.Application.Common.Security;
using PromptifyWebApi.Application.Sessions;
using PromptifyWebApi.Domain.Enums;

namespace PromptifyWebApi.Application.Prompts.Commands.CancelPrompt;

[Authorize]
public record CancelPromptCommand(int PromptId) : IRequest;

public class CancelPromptCommandHandler : IRequestHandler<CancelPromptCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelPromptCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _user = user;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CancelPromptCommand request, CancellationToken cancellationToken)
    {
        var prompt = await SessionAccess.GetOwnedPromptAsync(
            _context, _user, request.PromptId, cancellationToken);

        if (prompt.Status is not (PromptStatus.Pending or PromptStatus.Processing))
        {
            throw new ConflictException("Only in-flight prompts can be cancelled.");
        }

        prompt.Status = PromptStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(prompt.ToStatusChanged(), cancellationToken);
    }
}
