using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Handlers;

public class EventSourcingHandler(IEventStore eventStore, IEventProducer eventProducer) : IEventSourcingHandler<PostAggregate>
{
    public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
    {
        var aggregate = new PostAggregate();
        var events = await eventStore.GetEventsAsync(aggregateId);

        if (events is null || events.Count == 0)
        {
            return aggregate;
        }

        aggregate.ReplayEvents(events);
        aggregate.Version = events.Select(e => e.Version).Max();

        return aggregate;
    }

    public async Task RepublishEventsAsync()
    {
        var aggregateIds = await eventStore.GetAggregateIdsAsync();

        if (aggregateIds is null || aggregateIds.Count == 0)
        {
            return;
        }

        foreach (var aggregateId in aggregateIds)
        {
            var aggregate = await GetByIdAsync(aggregateId);
            if (aggregate is null || !aggregate.Active)
            {
                continue;
            }

            var events = await eventStore.GetEventsAsync(aggregateId);

            foreach (var @event in events)
            {
                var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
                await eventProducer.ProduceAsync(topic!, @event);
            }
        }
    }

    public async Task SaveAsync(AggregateRoot aggregate)
    {
        await eventStore.SaveEventsAsync(aggregate.Id, aggregate.GetUncommitedChanges(), aggregate.Version);
        aggregate.MarkChangesAsCommited();
    }
}
