using MyPos.Models;
using System;
using System.Security.Claims;

namespace MyPos.Function
{
    public static class Function
    {
        public static ProductView MapOfProduct(Product product, bool IsBestSeller)
        {
            return new ProductView
            {
                Id = product.Id,
                Name = product.ProductName,
                Price = product.Price,
                IsBestSeller = IsBestSeller
            };
        }
    }
    

    }
