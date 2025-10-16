using Confluent.Kafka;
using KafkaObserverService.Configuration;
using KafkaObserverService.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace KafkaObserverService.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly EventObservable _eventObservable;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly KafkaSettings _kafkaSettings;

        public KafkaConsumerService(
            EventObservable eventObservable,
            IOptions<KafkaSettings> kafkaSettings,
            ILogger<KafkaConsumerService> logger)
        {
            _eventObservable = eventObservable;
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;

            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = _kafkaSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_kafkaSettings.Topic);
            _logger.LogInformation("Subscribed to Kafka topic: {Topic}", _kafkaSettings.Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value != null)
                    {
                        await ProcessMessageAsync(consumeResult.Message.Value);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumption was cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while consuming from Kafka");
                }
            }

            _consumer.Close();
        }

        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                var userEvent = JsonSerializer.Deserialize<UserEvent>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (userEvent != null)
                {
                    await Task.Run(() => _eventObservable.NotifyObservers(userEvent));
                    _logger.LogDebug("Processed event for user {UserId}, type: {EventType}",
                        userEvent.UserId, userEvent.EventType);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message: {Message}", message);
            }
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
