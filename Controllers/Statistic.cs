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
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class StatisticController : ControllerBase
{
    [HttpGet("get_yearly_revenue")]
    public IActionResult GetYearlyRevenue(int year)
    {
        if (!AuthHelper.IsAdmin(User))
        {
            return Forbid("You are not authorized to do the action");
        }

        using var context = new MyDbContext();

        var yearlyRevenue =
            (from oi in context.OrderItems
             join o in context.Orders on oi.OrderId equals o.Id
             join p in context.Products on oi.ProductId equals p.Id
             where o.OrderDate.Year == year
             select p.Price * oi.Quantity)
            .Sum();
        if (yearlyRevenue == 0)
        {
            return NotFound("No revenue yet");
        }

        var result = new YearlyRevenueDTO()
        {
            Year = year,
            Revenue = yearlyRevenue
        };

        return Ok(result);
    }
    [HttpGet("get_monthly_revenue")]
    public IActionResult GetMonthlyRevenue(int year, int month)
    {
        if (!AuthHelper.IsAdmin(User))
        {
            return Forbid("You are not authorized to do the action");
        }

        using var context = new MyDbContext();
        var monthlyRevenue =
            (from oi in context.OrderItems
             join o in context.Orders on oi.OrderId equals o.Id
             join p in context.Products on oi.ProductId equals p.Id
             where o.OrderDate.Year == year && o.OrderDate.Month == month
             select p.Price * oi.Quantity)
            .Sum();
        if (monthlyRevenue == 0)
        {
            return NotFound("No revenue yet");
        }

        var result = new MonthlyRevenueDTO()
        {
            Year = year,
            Month = month,
            Revenue = monthlyRevenue
        };
                                                                                  
        return Ok(result);
    }
    [HttpGet("get_busiest_day")]
    public IActionResult GetBusiestDay()
    {
        if (!AuthHelper.IsAdmin(User))
        {
            return Forbid("You are not authorized to do the action");
        }

        using var context = new MyDbContext();

        var busiestDates = 
            (from oi in context.OrderItems
             join o in context.Orders on oi.OrderId equals o.Id
             group oi by o.OrderDate.DayOfWeek into g
             select new BusiestDayDTO
                  {
                    Date = g.Key,
                    TotalProductSold = g.Sum(oi => oi.Quantity)
                  })
                   .OrderByDescending(g => g.TotalProductSold)
                   .ToList();


        return Ok(busiestDates);
    }
    [HttpGet("get_average_day_revenue")]
    public IActionResult GetAverageDayRevenue()
    {
        //if (!AuthHelper.IsAdmin(User))
        //{
        //    return Forbid("You are not authorized to do the action");
        //}
        using var context = new MyDbContext();
        var TotalRevenue = from oi in context.OrderItems
                     join p in context.Products on oi.ProductId equals p.Id
                     select new
                     {
                         TotalRevenue = p.Price * oi.Quantity
                     };
        int TotalDate = context.Orders.GroupBy(oi => oi.OrderDate.Date).Count();
        float AverageRevenue = (float)TotalRevenue.Sum(x => x.TotalRevenue) / TotalDate;
        return Ok(AverageRevenue);
    }

}





