using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderSolution.API.DTOs;
using OrderSolution.API.Mappers;
using OrderSolution.Application.Interfaces;
using OrderSolution.Domain.Notifications;

namespace OrderSolution.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        private readonly DomainNotificationHandler _notificationHandler;

        public OrdersController(IOrdersService ordersService, DomainNotificationHandler notificationHandler)
        {
            _ordersService = ordersService;
            _notificationHandler = notificationHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO orderDto)
        {
            var correlationId = Request.Headers["X-Correlation-ID"].ToString();
            var order = OrderMapper.MapToEntity(orderDto);
            await _ordersService.ProcessOrder(order, correlationId);

            if (_notificationHandler.HasNotifications())
            {
                return BadRequest(_notificationHandler.GetNotifications());
            }

            return Ok(order);
        }
    }
}
