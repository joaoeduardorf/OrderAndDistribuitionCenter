﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrderSolution.Domain.Entities
{
    public class OrderItem
    {
        public int ItemId { get; set; }  // Auto-generated by the database
        public int IdSku { get; set; }
        public string DistributionCenter { get; set; }

        // Foreign Key to associate with Order
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
    }
}
