using BCC.Infrastructure.Contexts;
using BCC.Infrastructure.Interfaces;
using BCC.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace BCC.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"];
            var database = configuration["MongoDB:Database"];

            services.AddScoped<IMongoClient>(s => new MongoClient(connectionString));
            services.AddScoped<IMongoDatabase>(s => s.GetService<IMongoClient>().GetDatabase(database));
            services.AddScoped<IPersistantDataContext, PersistantDataContext>();
            services.AddScoped<IAccessTokenRepository>(s => new AccessTokenRepository(s.GetService<IPersistantDataContext>().AccessTokens));

            return services;
        }
    }
}
