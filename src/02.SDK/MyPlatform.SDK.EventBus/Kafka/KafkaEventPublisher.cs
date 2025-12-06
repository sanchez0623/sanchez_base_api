
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.Shared.Contracts.Events;
using System.Collections.Generic;

namespace MyPlatform.SDK.EventBus.Kafka
{
    public class KafkaEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _defaultTopic;

        public KafkaEventPublisher(string bootstrapServers, string defaultTopic = "default-topic")
        {
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _defaultTopic = defaultTopic;
        }

        // Standard interface implementation
        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IntegrationEvent
        {
            // By default use the constructor topic, or derive from event attributes if we had that logic.
            // For now, simpler to rely on the overload or default.
            await PublishToTopicAsync(@event, _defaultTopic, cancellationToken);
        }

        public async Task PublishAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default) where T : IntegrationEvent
        {
            foreach (var evt in events)
            {
                await PublishToTopicAsync(evt, _defaultTopic, cancellationToken);
            }
        }

        // Custom method to allow topic specification (as used in the Demo Service)
        // This is not part of IEventPublisher but useful for the demo.
        public async Task PublishAsync<T>(T @event, string? topic = null, CancellationToken cancellationToken = default) where T : class
        {
            var targetTopic = topic ?? _defaultTopic;
            var message = JsonSerializer.Serialize(@event);
            await _producer.ProduceAsync(targetTopic, new Message<Null, string> { Value = message }, cancellationToken);
        }

        private async Task PublishToTopicAsync<T>(T @event, string topic, CancellationToken cancellationToken)
        {
            var message = JsonSerializer.Serialize(@event);
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
