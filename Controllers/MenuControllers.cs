using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MyPos.Controllers;
using MyPos.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Microsoft.OpenApi.Validations;
using System.Security.Cryptography.X509Certificates;
using MyPos.Function;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Authorization;
using MyPos.Dtos;
using MySqlX.XDevAPI.Common;

[ApiController]
[Route("[controller]")]
public class MenuControllers : ControllerBase
{

    [HttpGet("menu")]
    public IActionResult GetMenu(int categoryId, int? PageNumber, int? PageSize)
    {
        int currentPage = PageNumber ?? 1;
        int currentPageSize = PageSize ?? 10;

        using (var context = new MyDbContext())
        {
            var menu = context.Products
    .Join(context.Category,
          Product => Product.CategoryId,
          Category => Category.Id,
          (Product, Category) => new
          {
              Name = Product.ProductName,
              Price = Product.Price,
              Type = Product.Type,
              Category = Category.CategoryName
          })
    .Skip((currentPage - 1) * currentPageSize)
    .Take(currentPageSize)
    .ToList();
            if (menu == null || menu.Count == 0)
            {
                return NotFound("No menu yet");
            }
            else
            {
                return Ok(menu);
            }
        }
    }
    [HttpGet("get_product_by_category")]
    public IActionResult GetProductByCategory (int categoryId, int? PageNumber, int? PageSize)
    {
        int currentPage = PageNumber ?? 1;
        int currentPageSize = PageSize ?? 10;
        using (var context = new MyDbContext())
        {
            var products = context.Products
                .Where(p => p.CategoryId == categoryId)
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();
            if (products == null || products.Count == 0)
            {
                return Ok(new { products = new List<Product>() }); // ✅ Always return JSON
            }

            return Ok(new { products = products });
        }
    }

    [HttpGet("get_best_seller")]
    public IActionResult GetBestSeller(int? PageNumber, int? PageSize)
    {
        using var context = new MyDbContext();
        var BestSeller = context.OrderItems
            .Join(context.Products,
                oi => oi.ProductId, p => p.Id,
                (oi, p) => new { Product = p, oi.Quantity })
                .GroupBy(oi => oi.Product)
                .Select(g => new
                {
                    Product = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                }).ToList();
        if (BestSeller == null || BestSeller.Count == 0)
        {
            return NotFound("No best seller yet");
        }
        var bestFood = BestSeller.Where(x => x.Product.Type == 0)
            .OrderByDescending(x => x.TotalQuantity)
            .FirstOrDefault();
        var bestDrink = BestSeller.Where(x => x.Product.Type == 1)
            .OrderByDescending(x => x.TotalQuantity)
            .FirstOrDefault();
        return Ok(new
        {
            BestFood = bestFood == null
         ? new BestSellerResponse { Message = "No bestselling food" }
         : new BestSellerResponse { Message = "Best selling food is below" , Product = Function.MapOfProduct(bestFood.Product, true) }
        ,

            BestDrink = bestDrink == null
         ? new BestSellerResponse { Message = "No bestselling drink" }
         : new BestSellerResponse { Message = "Best selling drink is below", Product = Function.MapOfProduct(bestDrink.Product, true) }
        });
    }

    [HttpGet("get_category")]
    public IActionResult GetCategory()
    {
        using var context = new MyDbContext();
        var categories = context.Category.ToList();
        if (categories == null || categories.Count == 0)
        {
            return NotFound("No category yet");
        }
        return Ok(categories);
    }

    [HttpPost("add_category")]
    public IActionResult AddCategory([FromBody] CategoryDTO categoryDTO)
    {
        if (!AuthHelper.IsAdmin(User))
        {
            return Forbid("You are not authorized to do the action");
        }
        using var context = new MyDbContext();
        if (string.IsNullOrWhiteSpace(categoryDTO.CategoryName))
        {
            return BadRequest("Category name is required");
        }
        var existingCategory = context.Category
            .FirstOrDefault(c => c.CategoryName.ToLower() == categoryDTO.CategoryName.ToLower());
        if (existingCategory != null)
        {
            return BadRequest("Category already exists");
        }
        var newCategory = new Category()
        {
            CategoryName = categoryDTO.CategoryName
        };

        context.Category.Add(newCategory);
        context.SaveChanges();
        return Ok("Category added successfully");
    }
    [HttpDelete("delete_menu")]
    public IActionResult DeleteMenu(int ProductId)
    {
        if (!AuthHelper.IsAdmin(User))
        {
            return Forbid("You are not authorized to do the action"); 
        }
        using var context = new MyDbContext();

        var deleteMenu = context.Products
            .FirstOrDefault(p => p.Id == ProductId);

        if (deleteMenu == null)
        {
            return NotFound("Menu not found");
        }

        context.Products.Remove(deleteMenu);
        context.SaveChanges();

        return Ok("Menu deleted successfully");
    }

    [HttpPost("add_product")]
    public IActionResult AddMenu([FromBody] ProductItem productItem)
    {
        if (!AuthHelper.IsAdmin(User))
        {
            return Forbid("You are not authorized to do the action"); 
        }
        using var context = new MyDbContext();
        foreach (var item in productItem.ProductDTO)
        {
            // Check for missing or invalid input
            if (string.IsNullOrWhiteSpace(item.ProductName))
            {
                return BadRequest("No product added");
            }

            if (item.Price == 0)
            {
                return BadRequest("Missing price");
            }
            var existingProduct = context.Products
                .FirstOrDefault(p => p.ProductName.ToLower() == item.ProductName.ToLower());
                
            if (existingProduct != null)
            {
                return BadRequest("Product already exists");
            }
            foreach (var product in productItem.ProductDTO)
            {
                var existingCategory = context.Category
                    .FirstOrDefault(c => c.Id == product.CategoryId);

                if (existingCategory == null)
                {
                    throw new BadHttpRequestException($"Invalid Category ID: {product.CategoryId}");
                }
            }


            var newProduct = new Product
            {
                ProductName = item.ProductName,
                Type = item.Type,
                Price = item.Price,
                CategoryId = item.CategoryId
            };

            context.Products.Add(newProduct);
            context.SaveChanges();
        }
        return Ok("Menu added successfully");
    }


    [HttpPut("update_product")]
    public IActionResult UpdateMenu([FromBody] Product product)
    {
        if (!AuthHelper.IsAdmin(User))
        {
            return Forbid("You are not authorized to do the action"); // Only admin can add product
        }
        using var context = new MyDbContext();

        var updateMenu = context.Products
            .FirstOrDefault(p => p.Id == product.Id);

        if (updateMenu == null)
        {
            return NotFound("Menu not found");
        }

        updateMenu.ProductName = product.ProductName;
        updateMenu.Type = product.Type;
        updateMenu.Price = product.Price;
        updateMenu.CategoryId = product.CategoryId;
        context.SaveChanges();

        return Ok("Menu updated successfully");
    }

}



