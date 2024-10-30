using Microsoft.Extensions.Logging;
using OrderSolution.Domain.Entities;
using OrderSolution.Domain.Notifications;
using OrderSolution.Domain.Validation.Interfaces;

namespace OrderSolution.Domain.Validation
{
    public class OrderValidationService : IOrderValidationService
    {
        private readonly DomainNotificationHandler _notificationHandler;
        private readonly ILogger<OrderValidationService> _logger;

        public OrderValidationService(DomainNotificationHandler notificationHandler, ILogger<OrderValidationService> logger)
        {
            _notificationHandler = notificationHandler;
            _logger = logger;
        }

        public bool ValidateOrder(Order order)
        {
            _logger.LogInformation("Validating order with Id: {OrderId}", order.Id);

            // Regra de negócio: verificar se o pedido contém mais de 100 itens
            if (order.Items.Count > 100)
            {
                _notificationHandler.AddNotification("Items", "The order cannot contain more than 100 items.");
                _logger.LogWarning("Order validation failed: more than 100 items in order with Id: {OrderId}", order.Id);
                return false;
            }

            _logger.LogInformation("Order validation successful for Id: {OrderId}", order.Id);
            return true;
        }
    }
}
