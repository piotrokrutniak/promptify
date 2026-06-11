using MassTransit;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Mappings;
using PromptifyWebApi.Application.Sessions;
using PromptifyWebApi.Application.Common.Models;
using PromptifyWebApi.Application.Common.Security;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Shared.Messaging;

namespace PromptifyWebApi.Application.Sessions.Commands.CreatePrompt;

[Authorize]
public record CreatePromptCommand(int SessionId, string Input, string? Data) : IRequest<PromptDto>;

public class CreatePromptCommandValidator : AbstractValidator<CreatePromptCommand>
{
    public CreatePromptCommandValidator()
    {
        RuleFor(v => v.SessionId).GreaterThan(0);
        RuleFor(v => v.Input).NotEmpty().MaximumLength(8000);
    }
}

public class CreatePromptCommandHandler : IRequestHandler<CreatePromptCommand, PromptDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePromptCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _user = user;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<PromptDto> Handle(CreatePromptCommand request, CancellationToken cancellationToken)
    {
        await SessionAccess.GetOwnedSessionAsync(_context, _user, request.SessionId, cancellationToken);
        await SessionAccess.EnsureSessionIdleAsync(_context, request.SessionId, cancellationToken);

        var maxOrder = await _context.Prompts
            .Where(p => p.SessionId == request.SessionId)
            .Select(p => (int?)p.OrderIndex)
            .MaxAsync(cancellationToken) ?? -1;

        var prompt = new Prompt
        {
            SessionId = request.SessionId,
            OrderIndex = maxOrder + 1,
            Input = request.Input,
            Data = request.Data,
            Status = PromptStatus.Pending
        };

        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new ProcessPromptCommand(prompt.Id, request.SessionId),
            cancellationToken);

        return prompt.ToDto();
    }
}
