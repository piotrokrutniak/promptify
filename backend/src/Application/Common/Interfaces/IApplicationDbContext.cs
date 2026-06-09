using PromptifyWebApi.Domain.Entities;

namespace PromptifyWebApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Session> Sessions { get; }

    DbSet<Prompt> Prompts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
