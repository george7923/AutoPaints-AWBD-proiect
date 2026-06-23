namespace LicentaInAngular.Server.DataLayer.DTO
{

    public class UpdatePersoanaDto
    {
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Email { get; set; }
        public string TipPersoana { get; set; }
        public string Telefon { get; set; }
        public string Rol { get; set; } // Must be Owner, Administrator, or Participant
    }
}
