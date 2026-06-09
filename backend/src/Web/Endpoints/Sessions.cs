using PromptifyWebApi.Application.Common.Models;
using PromptifyWebApi.Application.Sessions.Commands.CreatePrompt;
using PromptifyWebApi.Application.Sessions.Commands.CreateSession;
using PromptifyWebApi.Application.Sessions.Queries.GetSessionById;
using PromptifyWebApi.Application.Sessions.Queries.GetSessions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PromptifyWebApi.Web.Endpoints;

public class Sessions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapPost(CreateSession);
        groupBuilder.MapGet(GetSessions);
        groupBuilder.MapGet(GetSessionById, "{sessionId}");
        groupBuilder.MapPost(CreatePrompt, "{sessionId}/prompts");
    }

    [EndpointSummary("Create session with first prompt")]
    public static async Task<Created<CreateSessionResponse>> CreateSession(
        ISender sender,
        CreateSessionRequest request)
    {
        var result = await sender.Send(new CreateSessionCommand(request.Input, request.Title, request.Data));
        return TypedResults.Created($"/api/Sessions/{result.SessionId}", result);
    }

    [EndpointSummary("List sessions for current user")]
    public static async Task<Ok<IList<SessionListItemDto>>> GetSessions(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetSessionsQuery()));
    }

    [EndpointSummary("Get session with prompts")]
    public static async Task<Ok<SessionDto>> GetSessionById(ISender sender, int sessionId)
    {
        return TypedResults.Ok(await sender.Send(new GetSessionByIdQuery(sessionId)));
    }

    [EndpointSummary("Add prompt to session")]
    public static async Task<Created<PromptDto>> CreatePrompt(
        ISender sender,
        int sessionId,
        CreatePromptRequest request)
    {
        var result = await sender.Send(new CreatePromptCommand(sessionId, request.Input, request.Data));
        return TypedResults.Created($"/api/Sessions/{sessionId}/prompts/{result.Id}", result);
    }

    public record CreateSessionRequest(string Input, string? Title, string? Data);

    public record CreatePromptRequest(string Input, string? Data);
}
