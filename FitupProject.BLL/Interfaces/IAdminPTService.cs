using FitupProject.BLL.DTOs.PTRegister;
using FitupProject.BLL.Commons;

namespace FitupProject.BLL.Interfaces
{
    public interface IAdminPTService
    {
        Task<PagedResult<AdminPTListItemDto>> GetPtsAsync(string? status, int pageIndex, int pageSize);
        Task<AdminPTDetailDto> GetPtDetailAsync(string ptId);

        Task ApproveAsync(string ptId, string reviewerAccountId);
        Task RejectAsync(string ptId, string reviewerAccountId, string reason);
    }
}
