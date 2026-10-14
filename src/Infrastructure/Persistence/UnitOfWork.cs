using Application.Common.Interfaces;

namespace Infrastructure.Persistence;

// DbContext lifetime is owned by the DI container, so the unit of work must not dispose it
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
