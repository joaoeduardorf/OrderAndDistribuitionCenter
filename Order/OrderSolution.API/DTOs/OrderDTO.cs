namespace OrderSolution.API.DTOs
{
    public class OrderDTO
    {
        public List<OrderItemDTO> Items { get; set; }

    }

    public class OrderItemDTO
    {
        public int IdSku { get; set; }  // SKU identifier for inventory
    }
}
