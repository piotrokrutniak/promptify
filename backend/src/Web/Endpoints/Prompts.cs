using PromptifyWebApi.Application.Prompts.Commands.CancelPrompt;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PromptifyWebApi.Web.Endpoints;

public class Prompts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapPost(CancelPrompt, "{promptId}/cancel");
    }

    [EndpointSummary("Cancel a pending prompt")]
    public static async Task<NoContent> CancelPrompt(ISender sender, int promptId)
    {
        await sender.Send(new CancelPromptCommand(promptId));
        return TypedResults.NoContent();
    }
}
