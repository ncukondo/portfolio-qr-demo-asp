namespace DoctorPortfolioSite.Domain.Models;

public class ClassCredit
{
    public int Id { get; private set; }
    public int CourseId { get; private set; }
    public int CreditId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private ClassCredit() { }

    public static ClassCredit Create(int courseId, int creditId, decimal amount = 1.0m, DateTimeOffset? now = null)
    {
        Guard.AgainstNonPositive(courseId);
        Guard.AgainstNonPositive(creditId);
        if (amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "amount must be positive.");

        var stamp = now ?? DateTimeOffset.UtcNow;
        return new ClassCredit
        {
            CourseId = courseId,
            CreditId = creditId,
            Amount = amount,
            CreatedAt = stamp,
            UpdatedAt = stamp,
        };
    }
}
