namespace DoctorPortfolioSite.Domain.Models;

public class Credit
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Credit() { }

    public static Credit Create(
        string code,
        string label,
        string category,
        string description,
        DateTimeOffset? now = null)
    {
        Guard.AgainstBlank(code);
        Guard.AgainstBlank(label);

        var stamp = now ?? DateTimeOffset.UtcNow;
        return new Credit
        {
            Code = code,
            Label = label,
            Category = category ?? string.Empty,
            Description = description ?? string.Empty,
            CreatedAt = stamp,
            UpdatedAt = stamp,
        };
    }
}
