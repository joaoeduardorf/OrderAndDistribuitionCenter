﻿using OrderSolution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSolution.Application.Interfaces
{
    public interface IOrdersService
    {
        Task ProcessOrder(Order order, string correlationId);
    }
}