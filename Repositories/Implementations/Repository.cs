using CreaState.Data;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        public virtual async Task<List<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public virtual async Task<T> AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _db.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync(int id)
            => await _dbSet.FindAsync(id) != null;
    }
}
