namespace MyPos.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }
    public class TableDTO
    {
        public int Number { get; set; }
    }


}
