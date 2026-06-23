using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class PaymentDTO
    {
        public string PaymentMethodId { get; set; }
        public long AmountInBani { get; set; } // 10000 RON -> 100.00 RON
        public string Description { get; set; }
    }
}
