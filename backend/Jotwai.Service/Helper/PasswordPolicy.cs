namespace Jotwai.Service.Helper;

/// <summary>
/// Mirrors ASP.NET Core Identity's default password complexity rules
/// (RequireDigit, RequireLowercase, RequireUppercase, RequireNonAlphanumeric,
/// RequiredLength = 6), applied directly since the app uses its own plain
/// User entity rather than the full Identity UserManager pipeline.
/// </summary>
public static class PasswordPolicy
{
    private const int MinLength = 6;

    public static IReadOnlyList<string> Validate(string password)
    {
        var violations = new List<string>();

        if (password.Length < MinLength)
            violations.Add($"Password must be at least {MinLength} characters long.");
        if (!password.Any(char.IsDigit))
            violations.Add("Password must contain at least one digit.");
        if (!password.Any(char.IsLower))
            violations.Add("Password must contain at least one lowercase letter.");
        if (!password.Any(char.IsUpper))
            violations.Add("Password must contain at least one uppercase letter.");
        if (password.All(char.IsLetterOrDigit))
            violations.Add("Password must contain at least one non-alphanumeric character.");

        return violations;
    }
}
