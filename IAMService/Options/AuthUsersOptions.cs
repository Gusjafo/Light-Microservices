using System.Collections.Generic;
using IAMService.Models;

namespace IAMService.Options;

public sealed class AuthUsersOptions
{
    public const string SectionName = "AuthUsers";

    public List<AuthUser> Users { get; set; } = new();
}
