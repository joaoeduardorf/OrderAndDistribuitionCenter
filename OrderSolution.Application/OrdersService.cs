using Microsoft.Extensions.Logging;
using OrderSolution.Application.Interfaces;
using OrderSolution.Domain.Entities;
using OrderSolution.Domain.Repositories;
using OrderSolution.Domain.Validation.Interfaces;
using OrderSolution.Infrastructure.Integrations;

namespace OrderSolution.Application
{

    public class OrdersService : IOrdersService
    {
        private readonly IDistributionCenterAPI _dcApi;
        private readonly IOrderRepository _orderRepository;
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly IOrderValidationService _validationService;
        private readonly ILogger<OrdersService> _logger;

        public OrdersService(
            IDistributionCenterAPI dcApi,
            IOrderRepository orderRepository,
            IKafkaProducerService kafkaProducerService,
            IOrderValidationService validationService,
            ILogger<OrdersService> logger)
        {
            _dcApi = dcApi;
            _orderRepository = orderRepository;
            _kafkaProducerService = kafkaProducerService;
            _validationService = validationService;
            _logger = logger;
        }

        public async Task ProcessOrder(Order order, string correlationId)
        {
            _logger.LogInformation("Starting order processing. CorrelationId: {CorrelationId}", correlationId);

            if (!_validationService.ValidateOrder(order))
            {
                _logger.LogWarning("Order validation failed. CorrelationId: {CorrelationId}", correlationId);
                return;
            }

            await _orderRepository.SaveOrder(order);

            await Parallel.ForEachAsync(order.Items, async (item, cancellationToken) =>
            {
                try
                {
                    _logger.LogInformation("Fetching distribution center for SKU: {IdSku} with CorrelationId: {CorrelationId}", item.IdSku, correlationId);
                    var centers = await _dcApi.GetDistributionCentersByItemAsync(item.IdSku, correlationId);
                    item.DistributionCenter = centers;
                    _logger.LogInformation("Distribution center assigned for SKU: {IdSku} is {DistributionCenter}", item.IdSku, item.DistributionCenter);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while fetching distribution center for SKU: {IdSku} with CorrelationId: {CorrelationId}", item.IdSku, correlationId);
                    await _kafkaProducerService.PublishOrderForRetry(item, correlationId);
                    return;
                }
            });

            await _orderRepository.UpdateOrder(order);

            _logger.LogInformation("Saving order to database. CorrelationId: {CorrelationId}", correlationId);

            _logger.LogInformation("Order processing completed successfully. CorrelationId: {CorrelationId}", correlationId);
        }
    }

}
