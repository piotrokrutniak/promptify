using System.Text.Json;
using PromptifyWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PromptifyWebApi.Infrastructure.Prompts;

public static class PromptStatusNotifier
{
    public const string Channel = "prompt_status_changed";

    public static async Task NotifyAsync(DbContext context, Prompt prompt, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var payload = JsonSerializer.Serialize(new
        {
            prompt.Id,
            prompt.SessionId,
            Status = prompt.Status.ToString()
        });

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_notify({Channel}, {payload})",
            cancellationToken);
    }
}
