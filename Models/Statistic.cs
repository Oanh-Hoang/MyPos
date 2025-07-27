using System;

namespace MyPos.Models
{
    public class YearlyRevenueDTO
    {
        public int Year { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MonthlyRevenueDTO
    {
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int Month { get; set; }
    }
    public class  BusiestDayDTO
    {
        public DayOfWeek Date { get; set; }
        public int TotalProductSold { get; set; }
    }

}
