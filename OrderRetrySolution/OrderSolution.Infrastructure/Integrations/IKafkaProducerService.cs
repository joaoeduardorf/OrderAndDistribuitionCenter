using OrderSolution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSolution.Infrastructure.Integrations
{
    public interface IKafkaProducerService
    {
        Task PublishOrderForRetry(OrderItem orderItem, string correlationId);
    }
}
