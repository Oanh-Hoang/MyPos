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
            foreach (var Item in cartDto.OrderProducts)
            {
                var product = context.Products.FirstOrDefault(p => p.Id == Item.ProductId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }
                var existingCartitem = context.CartItems
                     .FirstOrDefault(ci => ci.CartId == cartDto.CartId && ci.ProductId == Item.ProductId);
                if (existingCartitem != null)
                {
                    existingCartitem.Quantity += Item.Quantity;
                }
                else
                {
                    var newCartItem = new CartItem
                    {
                        CartId = cartDto.CartId,
                        ProductId = Item.ProductId,
                        Quantity = Item.Quantity
                    };
                    context.CartItems.Add(newCartItem);
                    context.CartItems.Add(newCartItem);
                }

                context.SaveChanges();
            }
            return Ok("Item added to cart successfully.");

        }

        [HttpPost("create_cart")]
        public IActionResult CreateCart()
        {
            using var context = new MyDbContext();

            var newCart = new Cart
            {
                UserId = 0,
                Items = new List<CartItem>()
            };

            context.Carts.Add(newCart);
            context.SaveChanges();

            return Ok(new { cartId = newCart.Id, message = "New cart created" });
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

            // Validate order type
            if ((request.OrderType != 0 && request.OrderType != 1) || request.CartId <= 0)
            {
                return BadRequest("Invalid order information.");
            }

            // Create order
            var order = new MyPos.Models.Order
            {
                CartId = request.CartId,
                Type = request.OrderType,
                OrderDate = DateTime.Now
            };

            context.Orders.Add(order);
            context.SaveChanges();

            // If it's Take Away, save extra info
            if (request.OrderType == 1)
            {
                if (request.TakeAwayInfo == null ||
                    string.IsNullOrWhiteSpace(request.TakeAwayInfo.Name) ||
                    string.IsNullOrWhiteSpace(request.TakeAwayInfo.PhoneNumber) ||
                    string.IsNullOrWhiteSpace(request.TakeAwayInfo.Address))
                {
                    return BadRequest("Take Away information is required.");
                }

                var takeAway = new TakeAway
                {
                    OrderId = order.Id,
                    Name = request.TakeAwayInfo.Name,
                    PhoneNumber = request.TakeAwayInfo.PhoneNumber,
                    Address = request.TakeAwayInfo.Address
                };

                context.TakeAway.Add(takeAway);
                context.SaveChanges();
            }

            return Ok(new { message = "Order placed", orderId = order.Id });
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




