using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPos.Dtos;
using MyPos.Models;
using Mysqlx.Crud;
using MySqlX.XDevAPI.Common;

namespace MyPos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        [HttpPost("add_to_cart")]
        public IActionResult AddToCart([FromBody] CartDto cartDto)
        {
            using var context = new MyDbContext();

            // 1. Find existing cart or create new one
            var cart = context.Carts
                .FirstOrDefault(c => c.Id == cartDto.CartId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = 0, // <- should come from dto or JWT
                    Items = new List<CartItem>()
                };
                context.Carts.Add(cart);
                context.SaveChanges(); // Save to get cart.Id
            }

            // 2. Add / update items
            foreach (var item in cartDto.OrderProducts)
            {
                var product = context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    return NotFound($"Product {item.ProductId} not found.");
                }

                var existingCartItem = context.CartItems
                    .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == item.ProductId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += item.Quantity;
                }
                else
                {
                    var newCartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    };
                    context.CartItems.Add(newCartItem);
                }
            }

            // 3. Save once
            context.SaveChanges();

            return Ok(new { Message = "Item(s) added successfully", CartId = cart.Id });
        }


        [HttpGet("view_cart")]
        public IActionResult ViewCart(int cartId)
        {
            using var context = new MyDbContext();

            var cartItems = (
                from ci in context.CartItems
                join p in context.Products on ci.ProductId equals p.Id
                where ci.CartId == cartId
                select new CartItemDto
                {
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Quantity = ci.Quantity
                }
            ).ToList();

            if (cartItems == null || !cartItems.Any())
            {
                return BadRequest("Invalid cartId or empty cart.");
            }

            return Ok(cartItems);
        }
        [HttpPut("update_cart")]
        public IActionResult UpdateCart(CartInput cartInput)
        { using var context = new MyDbContext();
            var updateItem = context.CartItems.Where(ci => ci.CartId == cartInput.CartId && ci.ProductId == cartInput.ProductId).First();
            if (updateItem == null)
                return NotFound("Cart item not found.");

            updateItem.Quantity = cartInput.Quantity;
            context.CartItems.Update(updateItem);
            context.SaveChanges();
            return Ok("Cart item updated successfully.");
        }
        //        [HttpDelete("delete_cart_item")]
        //        public IActionResult DeleteCartItem(int cartId, [FromQuery] List<int> productIds)
        //        {
        //            using var context = new MyDbContext();
        //            var cartItem = context.CartItems.Where(ci => ci.CartId == cartId && productIds.Contains(ci.ProductId))
        //                 .ToList();
        //            if (cartItem.Count == 0)
        //                return NotFound("Cart item not found.");
        //            context.CartItems.RemoveRange(cartItem);
        //            context.SaveChanges();
        //            return Ok("Cart item deleted successfully.");
        //        }


        [HttpPost("order")]
        public IActionResult Order([FromBody] OrderRequest request)
        {
            using var context = new MyDbContext();

            // Step 1: Load raw cart items (needed for order & deletion)
            var cartItemsToDelete = context.CartItems
                .Where(c => c.CartId == request.CartId && request.SelectedCartItemIds.Contains(c.Id))
                .ToList();

            // Step 2: Join with products to get display data
            var displayItems = (from cartItem in cartItemsToDelete
                                join product in context.Products
                                    on cartItem.ProductId equals product.Id
                                select new
                                {
                                    ProductId = product.Id,
                                    ProductName = product.ProductName,
                                    Quantity = cartItem.Quantity,
                                    Price = product.Price
                                }).ToList();

            if (!displayItems.Any())
                return BadRequest("Cart is empty");

            // Step 3: Create Order
            var order = new MyPos.Models.Order
            {
                Type = request.OrderType,
                OrderDate = DateTime.Now
            };
            context.Orders.Add(order);
            context.SaveChanges();

            // Step 4: Create order items
            foreach (var item in displayItems)
            {
                var orderItem = new OrderItems
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };
                context.OrderItems.Add(orderItem);
            }

            // Step 5: Remove from cart
            context.CartItems.RemoveRange(cartItemsToDelete);
            context.SaveChanges();

            // Step 6: Return summary
            var response = displayItems.Select(item => new
            {
                item.ProductName,
                item.Quantity,
                item.Price
            }).ToList();
            return Ok(new
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Items = response
            });
        }


        [HttpDelete("delete_order")]
  public IActionResult DeleteOrder([FromBody] List<int> orderId)
        {
            using var context = new MyDbContext();

            var deleteOrder = context.Orders
                .Where(o => orderId.Contains(o.Id))
                .ToList();

            if (deleteOrder.Count == 0)
            {
                return NotFound("Order(s) not found.");
            }

            context.Orders.RemoveRange(deleteOrder);
            context.SaveChanges();

            return Ok($"{deleteOrder.Count} order(s) deleted successfully.");
        }

    }
}




