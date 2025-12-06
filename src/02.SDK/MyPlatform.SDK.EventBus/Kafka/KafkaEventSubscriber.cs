
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using MyPlatform.SDK.EventBus.Abstractions;

namespace MyPlatform.SDK.EventBus.Kafka
{
    // A simple base class for Kafka Consumers
    public abstract class KafkaEventSubscriber : BackgroundService
    {
        private readonly string _bootstrapServers;
        private readonly string _groupId;
        private readonly string _topic;

        protected KafkaEventSubscriber(string bootstrapServers, string groupId, string topic)
        {
            _bootstrapServers = bootstrapServers;
            _groupId = groupId;
            _topic = topic;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = _groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            return Task.Run(() =>
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                consumer.Subscribe(_topic);

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(stoppingToken);
                            // Handle message
                            ProcessMessage(consumeResult.Message.Value);
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occurred: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }, stoppingToken);
        }

        protected abstract void ProcessMessage(string message);
    }
}
