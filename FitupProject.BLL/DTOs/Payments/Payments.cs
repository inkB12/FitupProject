using FitupProject.Core.Commons.Enums;

namespace FitupProject.BLL.DTOs.Payments
{
    public record CreateTopUpDto(decimal AmountVnd, string ConversionRateId);

    public record CreateTopUpResultDto(
        string PaymentId,
        decimal AmountVnd,
        long OrderCode,
        string CheckoutUrl,
        DateTimeOffset? ExpiredAt
    );

    public record PaymentStatusDto(
        string PaymentId,
        decimal Amount,
        PaymentStatus Status,
        long? OrderCode,
        string? CheckoutUrl,
        DateTimeOffset? PaidAt,
        DateTimeOffset? ExpiredAt,
        DateTimeOffset? ConfirmedAt
    );

    public record PaymentListItemDto(
        string PaymentId,
        string AccountId,
        decimal Amount,
        PaymentStatus Status,
        PaymentMethod Method,
        long? OrderCode,
        string? CheckoutUrl,
        DateTimeOffset? PaidAt,
        DateTimeOffset? ExpiredAt,
        DateTimeOffset? ConfirmedAt,
        DateTimeOffset? CreatedAt
    );

    public record ConversionRateCreateDto(
        ConversionRateType Type,
        decimal Rate,
        ConversionRateStatus Status
    );

    public record ConversionRateUpdateDto(
        ConversionRateType Type,
        decimal Rate,
        ConversionRateStatus Status
    );
}
