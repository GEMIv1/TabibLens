using Domain.Entities.Abstractions;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _ctx;
        protected readonly DbSet<T> _set;

        protected RepositoryBase(AppDbContext ctx)
        {
            _ctx = ctx;
            _set = ctx.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _set.ToListAsync(cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _set.AddAsync(entity, cancellationToken);
        }

        public void Update(T entity)
        {
            _set.Update(entity);
        }

        public void SoftDelete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTimeOffset.UtcNow;
            _set.Update(entity);
        }
    }
}
