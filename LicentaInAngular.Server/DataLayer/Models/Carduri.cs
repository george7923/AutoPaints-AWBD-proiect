using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicentaInAngular.Server.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(NumarCard), IsUnique = true)]  // Ensures NumarCard is unique
public class Carduri
{
    [Key]
    public int IdCard { get; set; }  // Primary Key

    [Required]
    //[MaxLength(16)]  // Criptat
    public string NumarCard { get; set; }  // Unique Card Number

    [Required]
    public string CVV { get; set; }  // Card Security Code

    [Required]
    public DateTime DataExpirare { get; set; }  // Expiration Date

}
