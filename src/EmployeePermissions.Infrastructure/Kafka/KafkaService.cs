using System.Text.Json;
using Confluent.Kafka;
using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmployeePermissions.Infrastructure.Kafka;

public class KafkaService : IKafkaService, IDisposable
{
    private readonly ILogger<KafkaService> _logger;
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaService(IOptions<KafkaOptions> settings, ILogger<KafkaService> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers
        };

        _logger = logger;
        _producer = new ProducerBuilder<string, string>(config).Build();
        _topic = settings.Value.Topic;
    }

    public async Task PublishOperationAsync(Guid id, string name)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = id.ToString(),
                Value = JsonSerializer.Serialize(new
                {
                    Id = id,
                    Name = name,
                    Timestamp = DateTime.UtcNow
                })
            };

            _logger.LogInformation("[Kafka] Publishing Kafka message: OperationId={OperationId}, OperationName={OperationName}", id, name);
            await _producer.ProduceAsync(_topic, message);
            _logger.LogInformation("[Kafka] Kafka message published: MessageKey={MessageKey}", message.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing operation {Operation} with ID {OperationId}",
                name,
                id);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}