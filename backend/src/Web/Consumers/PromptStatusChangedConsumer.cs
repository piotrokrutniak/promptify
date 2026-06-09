using MassTransit;
using PromptifyWebApi.Shared.Messaging;
using PromptifyWebApi.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace PromptifyWebApi.Web.Consumers;

public class PromptStatusChangedConsumer : IConsumer<PromptStatusChanged>
{
    private readonly IHubContext<PromptStatusHub> _hubContext;

    public PromptStatusChangedConsumer(IHubContext<PromptStatusHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task Consume(ConsumeContext<PromptStatusChanged> context)
    {
        var message = context.Message;

        return _hubContext.Clients
            .Group(PromptStatusHub.SessionGroup(message.SessionId))
            .SendAsync("PromptStatusChanged", message, context.CancellationToken);
    }
}
