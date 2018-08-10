using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MSBLOC.Infrastructure.Interfaces
{
    public interface IRepository<T, in TField> where T : class, new()
    {
        Task DeleteAsync(T item);
        Task DeleteAsync(Expression<Func<T, bool>> expression);
        Task DeleteAsync(TField value);
        Task<T> GetAsync(Expression<Func<T, bool>> expression);
        Task<T> GetAsync(TField value);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression, int skip, int take);
        Task AddAsync(T item);
        Task AddAsync(IEnumerable<T> items);
    }
}
