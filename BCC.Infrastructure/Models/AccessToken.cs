using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace BCC.Infrastructure.Models
{
    [BsonIgnoreExtraElements]
    public class AccessToken
    {
        [BsonId(IdGenerator = typeof(GuidGenerator))]
        public Guid Id { get; set; }

        public long GitHubRepositoryId { get; set; }

        public string IssuedTo { get; set; }

        public DateTimeOffset IssuedAt { get; set; }
    }
}
