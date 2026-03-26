using Microsoft.EntityFrameworkCore;
using DelegateEntity = Stemmesystem.Data.Entities.Delegate;

namespace Stemmesystem.Data.Repositories;

public interface IDelegateRepository
{
    Task<DelegateEntity?> ValidateCode(string delegateCode, CancellationToken cancellationToken = default);
}

public class DelegateRepository : IDelegateRepository
{
    private readonly StemmesystemContext _context;

    public DelegateRepository(StemmesystemContext context)
    {
        _context = context;
    }

    public async Task<DelegateEntity?> ValidateCode(string delegateCode, CancellationToken cancellationToken = default)
    {
        delegateCode = delegateCode.ToUpper();
        var delegateEntity = await _context.Delegates
            .Include(d => d.Arrangement)
            .Where(d => d.DelegateCode == delegateCode)
            .FirstOrDefaultAsync(cancellationToken);
        return delegateEntity;

    }
}