using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class Payment : BaseEntity
    {
        public string AccountId { get; set; } = string.Empty;
        public string ConversionRateId { get; set; } = string.Empty;

        public decimal Amount { get; set; } // VND
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // VNPAY optional logs
        public string? VnpTxnRef { get; set; }          // vnp_TxnRef
        public string? VnpTransactionNo { get; set; }   // vnp_TransactionNo
        public string? VnpResponseCode { get; set; }    // vnp_ResponseCode
        public string? VnpTransactionStatus { get; set; } // vnp_TransactionStatus
        public string? VnpBankCode { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset? ExpiredAt { get; set; }


        public Account? Account { get; set; }
        public ConversionRate? ConversionRate { get; set; }
    }
}
