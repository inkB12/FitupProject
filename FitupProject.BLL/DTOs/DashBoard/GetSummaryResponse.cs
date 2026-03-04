using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.DTOs.DashBoard
{
    public class GetSummaryResponse
    {
        public int TotalUsers { get; set; }
        public int PendingPTs { get; set; }
        public int TodayBookings { get; set; }
        public decimal PointRevenue { get; set; }
    }
}
