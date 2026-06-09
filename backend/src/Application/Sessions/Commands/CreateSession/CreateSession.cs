using MassTransit;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Models;
using PromptifyWebApi.Application.Common.Security;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Shared.Messaging;

namespace PromptifyWebApi.Application.Sessions.Commands.CreateSession;

[Authorize]
public record CreateSessionCommand(string Input, string? Title, string? Data) : IRequest<CreateSessionResponse>;

public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(v => v.Input).NotEmpty().MaximumLength(8000);
        RuleFor(v => v.Title).MaximumLength(200);
    }
}

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, CreateSessionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateSessionCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _user = user;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateSessionResponse> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(_user.Id);

        var session = new Session
        {
            UserId = _user.Id,
            Title = request.Title
        };

        var prompt = new Prompt
        {
            OrderIndex = 0,
            Input = request.Input,
            Data = request.Data,
            Status = PromptStatus.Pending
        };

        session.Prompts.Add(prompt);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new ProcessPromptCommand(prompt.Id, session.Id),
            cancellationToken);

        return new CreateSessionResponse
        {
            SessionId = session.Id,
            Title = session.Title,
            Prompt = MapPrompt(prompt)
        };
    }

    private static PromptDto MapPrompt(Prompt prompt) => new()
    {
        Id = prompt.Id,
        OrderIndex = prompt.OrderIndex,
        Status = prompt.Status.ToString(),
        Input = prompt.Input,
        Output = prompt.Output,
        ErrorMessage = prompt.ErrorMessage,
        Created = prompt.Created
    };
}
