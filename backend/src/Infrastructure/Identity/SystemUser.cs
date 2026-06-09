using PromptifyWebApi.Application.Common.Interfaces;

namespace PromptifyWebApi.Infrastructure.Identity;

public class SystemUser : IUser
{
    public string? Id => null;

    public List<string>? Roles => null;
}
