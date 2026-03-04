using FitupProject.BLL.DTOs.PTRegister;
using FitupProject.BLL.DTOs.PTs;

namespace FitupProject.BLL.Interfaces
{
    public interface IPTService
    {
        Task<PTProfileResponse?> GetProfileAsync(string accountId);
        Task<IEnumerable<PTListItemResponse>> GetAllPTsAsync(PTFilterRequest? filter = null);
        Task<PTProfileResponse?> GetPTByIdAsync(string ptId);
        Task<PTMeResponse> RegisterAsync(string accountId, PTRegisterRequest req);
        Task<PTMeResponse> GetMeAsync(string accountId);
    }
}
