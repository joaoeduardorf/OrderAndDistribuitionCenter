using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderSolution.Domain.Entities;
using System.Text.Json;

namespace OrderSolution.Infrastructure.Integrations
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _topic = configuration["Kafka:Topic"];
            _logger = logger;
        }

        public async Task PublishOrderForRetry(OrderItem orderItem, string correlationId)
        {
            _logger.LogInformation("Publishing order to Kafka retry topic for CorrelationId: {CorrelationId}", correlationId);

            //var message = new
            //{
            //    OrderItem = orderItem,
            //    CorrelationId = correlationId
            //};

            var messageJson = JsonSerializer.Serialize(orderItem);
            var kafkaMessage = new Message<string, string> { Key = correlationId, Value = messageJson };

            try
            {
                await _producer.ProduceAsync(_topic, kafkaMessage);
                _logger.LogInformation("Order published to Kafka retry topic successfully for CorrelationId: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish order to Kafka retry topic for CorrelationId: {CorrelationId}", correlationId);
            }
        }
    }
}
