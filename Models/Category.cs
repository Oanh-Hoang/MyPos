namespace MyPos.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int Type { get; set; } 
    }
   public class CategoryDTO
    {
        public string CategoryName { get; set; }
    }
}
