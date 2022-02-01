using Microsoft.EntityFrameworkCore;
using StemmeSystem.Data.Entities;

namespace StemmeSystem.Data.Repositories;

public interface IDelegatRepository
{
    Task<Delegat?> ValiderKode(string delegatkode, CancellationToken cancellationToken = default);
}

public class DelegatRepository : IDelegatRepository
{
    private readonly StemmesystemContext _context;

    public DelegatRepository(StemmesystemContext context)
    {
        _context = context;
    }

    public async Task<Delegat?> ValiderKode(string delegatkode, CancellationToken cancellationToken = default)
    {
        delegatkode = delegatkode.ToUpper();
        var delegat = await _context.Delegat
            .Include(d => d.Arrangement)
            .Where(d => d.Delegatkode == delegatkode)
            .FirstOrDefaultAsync(cancellationToken);
        return delegat;

    }
}