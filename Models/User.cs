using MyPos.Models;
using ZstdSharp.Unsafe;
using System.Security.Claims;

namespace MyPos.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PassWord { get; set; }
        public required int Role { get; set; }
    }
    
}

namespace MyPos.Dtos
{
    public class UserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public int Role { get; set; }
    }
    public static class AuthHelper
    {
        public static bool IsAdmin(ClaimsPrincipal user)
        {
            var roleClaim = user?.FindFirst(ClaimTypes.Role)?.Value;
            return roleClaim == "Admin"; 
        }
    }

}





