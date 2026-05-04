using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class AdresaDTO
    {

            public int IdAdresa { get; set; }


            public string Strada { get; set; }
            public string Tara { get; set; }

            public string  Judet { get; set; }

            public string Localitate { get; set; }

            public string? Bloc { get; set; }

            public string? Scara { get; set; }

            public string? Etaj { get; set; }

            public string? Apartament { get; set; }
            
        
    }
}
