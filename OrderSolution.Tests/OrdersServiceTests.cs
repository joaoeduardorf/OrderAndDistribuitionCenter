using Microsoft.Extensions.Logging;
using Moq;
using OrderSolution.Application;
using OrderSolution.Domain.Entities;
using OrderSolution.Domain.Repositories;
using OrderSolution.Domain.Validation.Interfaces;
using OrderSolution.Infrastructure.Integrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;


namespace OrderSolution.Tests
{
    public class OrdersServiceTests
    {
        private readonly OrdersService _ordersService;
        private readonly Mock<IDistributionCenterAPI> _dcApiMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IKafkaProducerService> _kafkaProducerMock;
        private readonly Mock<IOrderValidationService> _validationServiceMock;
        private readonly Mock<ILogger<OrdersService>> _loggerMock;

        public OrdersServiceTests()
        {
            _dcApiMock = new Mock<IDistributionCenterAPI>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _kafkaProducerMock = new Mock<IKafkaProducerService>();
            _validationServiceMock = new Mock<IOrderValidationService>();
            _loggerMock = new Mock<ILogger<OrdersService>>();
            _ordersService = new OrdersService(
                _dcApiMock.Object,
                _orderRepositoryMock.Object,
                _kafkaProducerMock.Object,
                _validationServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessOrder_ShouldLogWarning_WhenOrderIsInvalid()
        {
            // Arrange
            var order = new Order();
            _validationServiceMock.Setup(v => v.ValidateOrder(order)).Returns(false);

            // Act
            await _ordersService.ProcessOrder(order, "test-correlation-id");

            // Assert
            _validationServiceMock.Verify(v => v.ValidateOrder(order), Times.Once);
            _orderRepositoryMock.Verify(r => r.SaveOrder(It.IsAny<Order>()), Times.Never);
            _loggerMock.VerifyLog(LogLevel.Warning, "Order validation failed", Times.Once());
        }

        [Fact]
        public async Task ProcessOrder_ShouldProcessOrderSuccessfully_WhenOrderIsValid()
        {
            // Arrange
            var order = new Order();
            order.AddItem(new OrderItem { IdSku = 123 });
            _validationServiceMock.Setup(v => v.ValidateOrder(order)).Returns(true);
            _dcApiMock.Setup(dc => dc.GetDistributionCentersByItemAsync(123, "test-correlation-id"))
                .ReturnsAsync("Center1");
            _orderRepositoryMock.Setup(r => r.SaveOrder(order)).Returns(Task.CompletedTask);
            _orderRepositoryMock.Setup(r => r.UpdateOrder(order)).Returns(Task.CompletedTask);

            // Act
            await _ordersService.ProcessOrder(order, "test-correlation-id");

            // Assert
            _orderRepositoryMock.Verify(r => r.SaveOrder(order), Times.Once);
            _dcApiMock.Verify(dc => dc.GetDistributionCentersByItemAsync(123, "test-correlation-id"), Times.Once);
            Assert.Equal("Center1", order.Items[0].DistributionCenter);
            _orderRepositoryMock.Verify(r => r.UpdateOrder(order), Times.Once);
        }

        [Fact]
        public async Task ProcessOrder_ShouldProcessOrderWithMixedResults()
        {
            // Arrange
            var order = new Order();
            order.AddItem(new OrderItem { IdSku = 123 });
            order.AddItem(new OrderItem { IdSku = 456 });

            _validationServiceMock.Setup(v => v.ValidateOrder(order)).Returns(true);
            _dcApiMock.Setup(dc => dc.GetDistributionCentersByItemAsync(123, "test-correlation-id"))
                .ReturnsAsync("Center1");
            _dcApiMock.Setup(dc => dc.GetDistributionCentersByItemAsync(456, "test-correlation-id"))
                .ThrowsAsync(new System.Exception("API error"));
            _kafkaProducerMock.Setup(k => k.PublishOrderForRetry(It.IsAny<OrderItem>(), "test-correlation-id"))
                .Returns(Task.CompletedTask);
            _orderRepositoryMock.Setup(r => r.SaveOrder(order)).Returns(Task.CompletedTask);
            _orderRepositoryMock.Setup(r => r.UpdateOrder(order)).Returns(Task.CompletedTask);

            // Act
            await _ordersService.ProcessOrder(order, "test-correlation-id");

            // Assert
            Assert.Equal("Center1", order.Items[0].DistributionCenter);
            _kafkaProducerMock.Verify(k => k.PublishOrderForRetry(order.Items[1], "test-correlation-id"), Times.Once);
            _orderRepositoryMock.Verify(r => r.SaveOrder(order), Times.Once);
            _orderRepositoryMock.Verify(r => r.UpdateOrder(order), Times.Once);
        }       
    }

    public static class LoggerExtensions
    {
        public static void VerifyLog(this Mock<ILogger<OrdersService>> loggerMock, LogLevel level, string containsText, Times times)
        {
            loggerMock.Verify(
                l => l.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(containsText)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), times);
        }
    }
}
