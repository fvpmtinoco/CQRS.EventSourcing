using MongoDB.Bson.Serialization.Attributes;

namespace CQRS.Core.Events;

public class EventModel
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public Guid AggregateIdentifier { get; set; }
    public string AggregateType { get; set; } = default!;
    public int Version { get; set; }
    public string EventType { get; set; } = default!;
    public BaseEvent EventData { get; set; } = default!;
}
