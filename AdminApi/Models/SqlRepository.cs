using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AdminApi.Models
{
    public class SqlRepository<T> : ISqlRepository<T> where T : class
    {
        private DbContext _db;

        private readonly DbSet<T> _entity;

        public SqlRepository(AppDbContext db)
        {
            _db = db;
            _entity = db.Set<T>();
        }
        public async Task<List<T>> SelectAll()
        {
            return await _entity.ToListAsync();
        }
        public virtual List<T> SelectAllByClause(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "")
        {
            IQueryable<T> query = _entity;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            query = includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            return orderBy != null ? orderBy(query).ToList() : query.ToList();
        }
        public async Task<T> SelectById(object id)
        {
            #pragma warning disable CS8603 // Possible null reference return.
            return await _entity.FindAsync(id);
            #pragma warning restore CS8603 // Possible null reference return.
        }
        public async Task<T> Insert(T obj)
        {
            _entity.Add(obj);
            await _db.SaveChangesAsync();
            return obj;
        }
        public async Task<T> Update(T obj)
        {
            _entity.Attach(obj);
            _db.Entry(obj).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return obj;
        }
        public async Task<T> Delete(object id)
        {
            T? existing =await _entity.FindAsync(id);
            if (existing != null)
            {
                _entity.Remove(existing);
                await _db.SaveChangesAsync();              
            }
            #pragma warning disable CS8603 // Possible null reference return.
            return existing;          
            #pragma warning restore CS8603 // Possible null reference return.
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                    //_db = null;
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
