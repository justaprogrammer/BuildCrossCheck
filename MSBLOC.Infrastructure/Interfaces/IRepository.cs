using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSBLOC.Infrastructure.Interfaces
{
    public interface IRepository<T, in TField> where T : class, new()
    {
        Task DeleteAsync(T item);
        Task DeleteAsync(TField value);
        Task<T> GetAsync(TField value);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(int skip, int take);
        Task AddAsync(T item);
        Task AddAsync(IEnumerable<T> items);
    }
}
