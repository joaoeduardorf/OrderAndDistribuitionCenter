using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OrderSolution.Domain.Entities;
using System.Text.Json;
using OrderSolution.Domain.Repositories;
using OrderSolution.Infrastructure.Integrations;
using OrderSolution.Infrastructure.Data;

namespace OrdersSolution.RetryConsumer
{
    public class OrderRetryConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderRetryConsumerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly IDistributionCenterAPI _dcApi;
        private readonly IOrderRepository _orderRepository;

        public OrderRetryConsumerService(
            IDistributionCenterAPI dcApi,
            //IOrderRepository orderRepository,
            IConfiguration configuration,
            ILogger<OrderRetryConsumerService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _dcApi = dcApi;
            //_orderRepository = orderRepository;
            
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:29092",//_configuration["Kafka:BootstrapServers"],
                GroupId = "order-retry-consumer-group-1",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:29092"
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private void StartConsumerLoop(CancellationToken stoppingToken)
        {
            //_consumer.Subscribe(_configuration["Kafka:Topic"]);
            _consumer.Subscribe("order-retry-topic");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        var correlationId = consumeResult.Message.Key;
                        var orderData = consumeResult.Message.Value;

                        _logger.LogInformation("Consuming message for reprocessing. CorrelationId: {CorrelationId}", correlationId);

                        var orderItem = JsonSerializer.Deserialize<OrderItem>(orderData);
                        if (orderItem != null)
                        {
                            ReprocessOrder(orderItem, correlationId);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error occurred while consuming Kafka message.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Order reprocessing consumer has been stopped.");
            }
            finally
            {
                _consumer.Close();
            }
        }

        private void ReprocessOrder(OrderItem orderItem, string correlationId)
        {
            using var scope = _scopeFactory.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var orderItemRepository = scope.ServiceProvider.GetRequiredService<IOrderItemRepository>();

            try
            {
                _logger.LogInformation("Reprocessing order with CorrelationId: {CorrelationId}", correlationId);

                var centers = _dcApi.GetDistributionCentersByItemAsync(orderItem.IdSku, correlationId).GetAwaiter().GetResult();
                if(centers != null)
                {
                    orderItem.DistributionCenter = string.Join(", ", centers);
                    _orderRepository.UpdateOrderItem(orderItem).Wait();
                }
                else
                {
                    var messageJson = JsonSerializer.Serialize(orderItem);
                    var kafkaMessage = new Message<string, string> { Key = correlationId, Value = messageJson };
                    _producer.ProduceAsync("order-DLQ-topic", kafkaMessage);
                }
                _logger.LogInformation("Order reprocessed successfully. CorrelationId: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reprocess order. CorrelationId: {CorrelationId}", correlationId);
                var messageJson = JsonSerializer.Serialize(orderItem);
                var kafkaMessage = new Message<string, string> { Key = correlationId, Value = messageJson };
                _producer.ProduceAsync("order-DLQ-topic", kafkaMessage);
            }
        }

        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
