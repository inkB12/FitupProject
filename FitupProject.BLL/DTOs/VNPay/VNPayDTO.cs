using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.VNPay
{
    public record CreateTopUpDto(decimal AmountVnd, string ConversionRateId);

    public record CreateTopUpResultDto(string PaymentId, string PayUrl);

    public record ConversionRateCreateDto(decimal Rate, ConversionRateStatus Status);
    public record ConversionRateUpdateDto(decimal Rate, ConversionRateStatus Status);

}
