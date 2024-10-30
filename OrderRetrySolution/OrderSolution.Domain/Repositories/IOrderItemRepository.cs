using OrderSolution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSolution.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task SaveOrderItem(OrderItem orderItem);
        Task<OrderItem> GetOrderItemById(int itemId);
    }
}
