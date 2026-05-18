using DoctorPortfolioSite.Data;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Application.Credits;

public class CreditService : ICreditService
{
    private readonly ApplicationDbContext _db;

    public CreditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CreditOption>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Credits
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CreditOption(c.Code, c.Label, c.Category))
            .ToListAsync(cancellationToken);
    }
}
