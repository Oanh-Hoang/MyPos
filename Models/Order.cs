
namespace MyPos.Models
{
    public class Order
    {
        public int Id { get; set; }
        public List<OrderItems> orderItems { get; set; }
        public DateTime OrderDate { get; set; }
        public int Type { get; set; } // 0 for eat-in, 1 for take-away
        public static Order GetOrderTime()
        {
            DateTime now = DateTime.Now;
            int hour = now.Hour;
            int minute = now.Minute;

            Console.WriteLine($"Order placed at {hour:D2}:{minute:D2}");

            return new Order
            {
                OrderDate = now
            };
        }
    }


    public class OrderItems
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderRequest
    {
        public int OrderType { get; set; }
        public int CartId { get; set; } 
        public List<int> SelectedCartItemIds { get; set; }
    }
}
