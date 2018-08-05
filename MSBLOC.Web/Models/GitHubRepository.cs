using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MSBLOC.Web.Models
{
    public class GitHubRepository
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Secret { get; set; }
    }
}
