using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MSBLOC.Infrastructure.Contexts;
using MSBLOC.Infrastructure.Interfaces;
using MSBLOC.Infrastructure.Repositories;

namespace MSBLOC.Infrastructure.Extensions
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
