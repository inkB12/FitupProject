using FitupProject.BLL.DTOs.VNPay;
using Microsoft.AspNetCore.Http;

namespace FitupProject.BLL.Interfaces
{
    public interface ITopUpService
    {
        Task<CreateTopUpResultDto> CreateTopUpAsync(string accountId, CreateTopUpDto dto, string ipAddress);
        Task<(string rspCode, string message)> HandleIpnAsync(IQueryCollection query);
        Task<object> HandleReturnAsync(IQueryCollection query);
    }
}
