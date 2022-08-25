using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogsViewerService.Models
{
    [BsonIgnoreExtraElements]
    public class LoggingRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        [BsonElement("Level")]
        public string LogLevel { get; set; } = string.Empty;

        public string MessageTemplate { get; set; } = string.Empty;

        [BsonElement("RenderedMessage")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("Properties")]
        public object? Properties { get; set; }

        public string UtcTimestamp { get; set; } = string.Empty;
    }
}
