using Microsoft.Extensions.Options;
using MongoDB.Driver;
using AccountsService.Utilities;
using MongoDB.Bson;

namespace AccountsService.Infrastructure.Context
{
    public class MongoDbContext<T>
    {
        private readonly IMongoDatabase _database = null!;
        private readonly string _collectionName = string.Empty;
        public string DatabaseName
        {
            get => _database.DatabaseNamespace.DatabaseName;
        }

        public MongoDbContext(string connectionString, string collectionName)
        {
            var mongoUrl = new MongoUrl(connectionString);
            var mongoClient = new MongoClient(mongoUrl);

            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            _collectionName = collectionName;
        }

        public IMongoCollection<T> GetCollection()
        {
            return _database.GetCollection<T>(_collectionName);
        }
    }
}
