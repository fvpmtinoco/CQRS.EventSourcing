using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore(IEventStoreRepository eventStoreRepository, IEventProducer eventProducer) : IEventStore
    {
        public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
        {
            var eventStream = await eventStoreRepository.FindByAggregateIdAsync(aggregateId);

            if (eventStream is null || eventStream.Count == 0)
                throw new AggregateNotFoundException("Incorrect post ID provided");

            return [.. eventStream.OrderBy(x => x.Version).Select(x => x.EventData)];
        }

        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
        {
            var eventStream = await eventStoreRepository.FindByAggregateIdAsync(aggregateId);

            if (expectedVersion != -1 && eventStream.Count > 0 && eventStream[^1].Version != expectedVersion)
                throw new ConcurrencyException();

            var version = expectedVersion;
            foreach (var @event in events)
            {
                version++;
                @event.Version = version;
                var eventType = @event.GetType().Name;
                var eventModel = new EventModel
                {
                    Timestamp = DateTime.UtcNow,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await eventStoreRepository.SaveAsync(eventModel);

                var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
                await eventProducer.ProduceAsync(topic!, @event);
            }
        }
    }
}
