using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MSBLOC.Infrastructure.Models
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
