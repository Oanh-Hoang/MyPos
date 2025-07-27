namespace MyPos.Controllers;
using MyPos.Dtos;
using Microsoft.AspNetCore.Mvc;
using MyPos.Models;
using Microsoft.EntityFrameworkCore;
using MyPos.Function;
using System.Security.Claims;
using Microsoft.Extensions.Configuration.UserSecrets;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{


    [HttpPost("sign_up_user")]
    public IActionResult SignUpUser([FromBody] UserDto userDto)
    {
        if (userDto == null || string.IsNullOrEmpty(userDto.Username) || string.IsNullOrEmpty(userDto.Password))
        {
            return BadRequest("Invalid user data.");
        }

        var newUser = new User
        {
            Name = userDto.Username,
            PassWord = userDto.Password,
            Role = 1
        };

        try
        {
            var context = new MyDbContext();
            var addedUser = context.User.Add(newUser);
            context.SaveChanges();
            return Ok("User signed up successfully!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }



    [HttpPost("sign_up_admin")]
    public IActionResult SignUpAdmin([FromBody] UserDto userDto)
    {
        if (userDto == null || string.IsNullOrEmpty(userDto.Username) || string.IsNullOrEmpty(userDto.Password))
        {
            return BadRequest("Invalid user data.");
        }

        // Create a DB context
        var context = new MyDbContext();

        // Check if user already exists
        var existingUser = context.User.FirstOrDefault(u => u.Name == userDto.Username);

        if (existingUser != null)
        {
            return BadRequest("Username already exists.");
        }

        // Create new user
        var newUser = new User
        {
            Name = userDto.Username,
            PassWord = userDto.Password,
            Role = 2 // Admin
        };

        try
        {
            context.User.Add(newUser);
            context.SaveChanges();
            return Ok("User signed up successfully!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [HttpPost("log_in")]
    public IActionResult LogIn([FromBody] UserDto userDto)
    {
        if (userDto == null || string.IsNullOrEmpty(userDto.Username) || string.IsNullOrEmpty(userDto.Password))
        {
            return BadRequest("Invalid user data.");
        }

        var context = new MyDbContext();
        var user = context.User
            .FirstOrDefault(u => u.Name == userDto.Username && u.PassWord == userDto.Password);

        if (user != null)
        {
            var authService = new AuthService();
            var token = authService.GenerateJwtToken(user);

            var currentUser = new AuthResponse
            {
                Token = token,
                Username = userDto.Username,
                Role = user.Role
            };
            return Ok(currentUser);
        }
        else
        {
            return Unauthorized("Invalid username or password.");
        }
    }
    [HttpGet("get_user")]
    public IActionResult GetUser()
    {
        if (!AuthHelper.IsAdmin(User))
        {
            return Forbid();
        }
        return Ok("Here are the users");
    }
}

    
