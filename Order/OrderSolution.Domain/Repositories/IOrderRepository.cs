using OrderSolution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSolution.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task SaveOrder(Order order);
        Task UpdateOrder(Order order);

        Task UpdateOrderItem(OrderItem orderItem);
        Task<Order> GetOrderById(int orderId);
    }
}
