namespace PromptifyWebApi.Application.Common.Interfaces;

public interface ILlmClient
{
    Task<string> CompleteAsync(string input, CancellationToken cancellationToken);
}
