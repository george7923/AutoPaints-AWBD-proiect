using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class ValidateCardDTO
    {
        [Required]
        [StringLength(16, MinimumLength = 12)]
        public string CardNumber { get; set; }  // e.g. "4242424242424242"

        [Required]
        public string ExpMonth { get; set; }    // "12"

        [Required]
        public string ExpYear { get; set; }     // "2026"

        [Required]
        [StringLength(4, MinimumLength = 3)]
        public string Cvc { get; set; }         // "123"
    }
}
