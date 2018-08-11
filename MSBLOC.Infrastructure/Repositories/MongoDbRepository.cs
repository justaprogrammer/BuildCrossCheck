using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MSBLOC.Infrastructure.Interfaces;

namespace MSBLOC.Infrastructure.Repositories
{
    public class MongoDbRepository<T, TField> : IRepository<T, TField> where T : class, new()
    {
        protected readonly IMongoCollection<T> Entities;

        private readonly Expression<Func<T, TField>> _idExpression;

        public MongoDbRepository(IMongoCollection<T> entities, Expression<Func<T, TField>> idExpression)
        {
            Entities = entities;
            _idExpression = idExpression;
        }

        public async Task DeleteAsync(TField value)
        {
            var filter = Builders<T>.Filter.Eq(_idExpression, value);

            await Entities.DeleteOneAsync(filter);
        }

        public async Task DeleteAsync(T item)
        {
            var getter = _idExpression.Compile();

            var value = getter(item);

            await DeleteAsync(value);
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> expression)
        {
            var filter = Builders<T>.Filter.Where(expression);

            await Entities.DeleteManyAsync(filter);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> expression)
        {
            var filter = Builders<T>.Filter.Where(expression);

            return await Entities.Find(filter).FirstAsync();
        }

        public async Task<T> GetAsync(TField value)
        {
            var filter = Builders<T>.Filter.Eq(_idExpression, value);

            return await Entities.Find(filter).FirstAsync();
        }

        public Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression)
        {
            var filter = Builders<T>.Filter.Where(expression);

            var items = Entities.Find(filter).ToEnumerable();

            return Task.FromResult(items);
        }

        public Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression, int skip, int take)
        {
            var filter = Builders<T>.Filter.Where(expression);

            var items = Entities.Find(filter).Skip(skip).Limit(take).ToEnumerable();

            return Task.FromResult(items);
        }

        public async Task AddAsync(T item)
        {
            await Entities.InsertOneAsync(item);
        }

        public async Task AddAsync(IEnumerable<T> items)
        {
            await Entities.InsertManyAsync(items);
        }
    }
}
