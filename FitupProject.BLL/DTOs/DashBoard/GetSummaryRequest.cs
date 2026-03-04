using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.DTOs.DashBoard
{
    public class GetSummaryRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }
}
