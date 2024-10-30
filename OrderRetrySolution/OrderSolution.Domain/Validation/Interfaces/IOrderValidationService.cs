using OrderSolution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSolution.Domain.Validation.Interfaces
{
    public interface IOrderValidationService
    {
        bool ValidateOrder(Order order);
    }
}
