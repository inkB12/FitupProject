using FitupProject.BLL.DTOs.PTs;

namespace FitupProject.BLL.Interfaces
{
    public interface IPTService
    {
        Task<PTProfileResponse?> GetProfileAsync(string accountId);
    }
}