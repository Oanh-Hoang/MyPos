
namespace MyPos.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public Cart Cart { get; set; }
    }

    public class CartDto
    {
        public int CartId { get; set; }
        public required List<OrderProduct> OrderProducts { get; set; }
    }

    public class CartItemDto
    {
        public int Quantity { get; set; }
        public string? ProductName { get; set; }
        public int Price { get; set; }
    }
    public class OrderProduct
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class CartInput
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}



