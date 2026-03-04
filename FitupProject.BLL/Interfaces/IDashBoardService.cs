using FitupProject.BLL.DTOs.DashBoard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.Interfaces
{
    public interface IDashBoardService
    {
        Task<GetSummaryResponse> GetSummaryAsync(GetSummaryRequest request);
    }
}
