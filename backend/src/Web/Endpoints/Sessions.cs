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
    [EndpointDescription(
        "Creates a new session for the authenticated user and enqueues the first prompt for worker processing. " +
        "Request body: input (required, max 8000 chars), title (optional, max 200 chars), data (optional context). " +
        "Returns 201 Created with sessionId, title, and the initial prompt in Pending status. " +
        "Location header points to /api/Sessions/{sessionId}.")]
    public static async Task<Created<CreateSessionResponse>> CreateSession(
        ISender sender,
        CreateSessionRequest request)
    {
        var result = await sender.Send(new CreateSessionCommand(request.Input, request.Title, request.Data));
        return TypedResults.Created($"/api/Sessions/{result.SessionId}", result);
    }

    [EndpointSummary("List sessions for current user")]
    [EndpointDescription("Returns all sessions owned by the authenticated user, ordered by most recently created.")]
    public static async Task<Ok<IList<SessionListItemDto>>> GetSessions(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetSessionsQuery()));
    }

    [EndpointSummary("Get session with prompts")]
    [EndpointDescription("Returns a single session with all prompts for the authenticated owner. Returns 404 if not found or not owned.")]
    public static async Task<Ok<SessionDto>> GetSessionById(ISender sender, int sessionId)
    {
        return TypedResults.Ok(await sender.Send(new GetSessionByIdQuery(sessionId)));
    }

    [EndpointSummary("Add prompt to session")]
    [EndpointDescription(
        "Adds a follow-up prompt to an existing session. The session must be idle (no Pending or Processing prompts). " +
        "Returns 409 Conflict if a prompt is already in flight. Returns 201 Created with the new prompt.")]
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
