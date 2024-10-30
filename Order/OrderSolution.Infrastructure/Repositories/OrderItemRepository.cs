using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderSolution.Domain.Entities;
using OrderSolution.Domain.Repositories;
using OrderSolution.Infrastructure.Data;

namespace OrderSolution.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly OrderContext _context;

        public OrderItemRepository(OrderContext context)
        {
            _context = context;
        }

        public async Task SaveOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task<OrderItem> GetOrderItemById(int itemId)
        {
            return await _context.OrderItems.FindAsync(itemId);
        }
    }
}