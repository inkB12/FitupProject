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

        public decimal Amount { get; set; } // tiền thật hoặc point gốc (tuỳ định nghĩa)
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // nav
        public Account? Account { get; set; }
        public ConversionRate? ConversionRate { get; set; }
    }
}
