using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AdminApi.Models
{
    public interface ISqlRepository<T> where T : class
    {
        Task<List<T>> SelectAll();
        List<T> SelectAllByClause(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "");
        Task<T> SelectById(object id);
        Task<T> Insert(T obj);
        Task<T> Update(T obj);
        Task<T> Delete(object id);
    }
}
