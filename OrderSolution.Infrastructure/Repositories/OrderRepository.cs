using Microsoft.EntityFrameworkCore;
using OrderSolution.Domain.Entities;
using OrderSolution.Domain.Repositories;
using OrderSolution.Infrastructure.Data;

namespace OrderSolution.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _context;

        public OrderRepository(OrderContext context)
        {
            _context = context;
        }

        public async Task SaveOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Update(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> GetOrderById(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items) // Inclui os itens do pedido na consulta, se necessário
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
