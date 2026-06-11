using System.Linq.Expressions;
using PromptifyWebApi.Application.Common.Models;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Shared.Messaging;

namespace PromptifyWebApi.Application.Common.Mappings;

public static class PromptMappings
{
    public static PromptDto ToDto(this Prompt prompt) => new()
    {
        Id = prompt.Id,
        OrderIndex = prompt.OrderIndex,
        Status = prompt.Status,
        Input = prompt.Input,
        Output = prompt.Output,
        ErrorMessage = prompt.ErrorMessage,
        Created = prompt.Created
    };

    public static Expression<Func<Prompt, PromptDto>> ToDtoExpression => p => new PromptDto
    {
        Id = p.Id,
        OrderIndex = p.OrderIndex,
        Status = p.Status,
        Input = p.Input,
        Output = p.Output,
        ErrorMessage = p.ErrorMessage,
        Created = p.Created
    };

    public static PromptStatusChanged ToStatusChanged(this Prompt prompt) =>
        new(
            prompt.Id,
            prompt.SessionId,
            prompt.OrderIndex,
            prompt.Status.ToString(),
            prompt.Input,
            prompt.Output,
            prompt.ErrorMessage);
}
