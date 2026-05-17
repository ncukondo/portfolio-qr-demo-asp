using System.Runtime.CompilerServices;

namespace DoctorPortfolioSite.Domain;

internal static class Guard
{
    public static string AgainstBlank(string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} must not be empty.", paramName);
        return value;
    }

    public static int AgainstNonPositive(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be positive.");
        return value;
    }
}
