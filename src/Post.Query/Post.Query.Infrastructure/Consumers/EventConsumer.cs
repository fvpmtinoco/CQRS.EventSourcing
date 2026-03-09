using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;
using System.Text.Json;

namespace Post.Query.Infrastructure.Consumers
{
    public class EventConsumer(IOptions<ConsumerConfig> config, IEventHandler eventHandler, ILogger<EventConsumer> logger) : IEventConsumer
    {
        private readonly ConsumerConfig _config = config.Value;
        private readonly IEventHandler _eventHandler = eventHandler;
        private readonly ILogger<EventConsumer> _logger = logger;

        public void Consume(string topic)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config)
                                        .SetKeyDeserializer(Deserializers.Utf8)
                                        .SetValueDeserializer(Deserializers.Utf8)
                                        .Build();

            consumer.Subscribe(topic);

            var options = new JsonSerializerOptions
            {
                Converters = { new EventJsonConverter() }
            };

            while (true)
            {
                try
                {
                    var consumerResult = consumer.Consume();

                    if (consumerResult?.Message == null)
                        continue;

                    var @event = JsonSerializer.Deserialize<BaseEvent>(consumerResult.Message.Value, options);
                    var handlerMethod = _eventHandler.GetType().GetMethod("On", [@event!.GetType()]);

                    if (handlerMethod == null)
                    {
                        throw new ArgumentNullException(nameof(handlerMethod), "Could not find event handler method");
                    }
                    handlerMethod.Invoke(_eventHandler, [@event]);
                    consumer.Commit(consumerResult);
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    _logger.LogWarning("Topic '{Topic}' not yet available, retrying in 5s...", topic);
                    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                }
            }
        }
    }
}
