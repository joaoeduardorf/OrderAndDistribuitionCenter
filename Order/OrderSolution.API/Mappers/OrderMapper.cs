using OrderSolution.API.DTOs;
using OrderSolution.Domain.Entities;

namespace OrderSolution.API.Mappers
{
    public static class OrderMapper
    {
        public static Order MapToEntity(OrderDTO orderDTO)
        {
            var order = new Order();

            foreach (var itemDto in orderDTO.Items)
            {
                var item = new OrderItem
                {
                    IdSku = itemDto.IdSku
                };
                order.AddItem(item);
            }

            return order;
        }
    }
}
