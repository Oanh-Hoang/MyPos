using System;
using System.ComponentModel.DataAnnotations;

namespace MyPos.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string ?ProductName { get; set; }
        public int Type { get; set; }
        public int Price { get; set; }
        public int CategoryId { get; set; } 
    }
    public class ProductItem
    {
        public required List<ProductDTO>ProductDTO { get; set; }

    }

    public class ProductDTO
    {
        public string ProductName { get; set; }
        public int Type { get; set; }
        public int Price { get; set; }
        public int CategoryId { get; set; }
    }
    
    public class ProductView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsBestSeller { get; set; }
    }
    public class BestSellerResponse
    {
        public string Message { get; set; }
        public ProductView Product { get; set; }
    }

}
