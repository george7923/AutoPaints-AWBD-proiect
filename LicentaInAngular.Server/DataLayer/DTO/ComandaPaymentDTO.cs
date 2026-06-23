using LicentaInAngular.Server.Models;

namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class ComandaPaymentDTO
    {
        public ComandaSubmitDTO Comanda { get; set; }
        public PaymentDTO Payment { get; set; }
    }
}
