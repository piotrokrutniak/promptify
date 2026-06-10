using MassTransit;
using PromptifyWebApi.Application.Common.Exceptions;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Security;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Shared.Messaging;

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
        var prompt = await _context.Prompts
            .Include(p => p.Session)
            .FirstOrDefaultAsync(p => p.Id == request.PromptId, cancellationToken);

        if (prompt is null)
        {
            throw new EntityNotFoundException(nameof(Prompt), request.PromptId);
        }

        if (prompt.Session.UserId != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (prompt.Status is not (PromptStatus.Pending or PromptStatus.Processing))
        {
            throw new ConflictException("Only in-flight prompts can be cancelled.");
        }

        prompt.Status = PromptStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new PromptStatusChanged(
                prompt.Id,
                prompt.SessionId,
                prompt.OrderIndex,
                prompt.Status.ToString(),
                prompt.Input,
                prompt.Output,
                prompt.ErrorMessage),
            cancellationToken);
    }
}
