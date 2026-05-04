namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class UpdateUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; } // Can be null if no update needed
        public UpdatePersoanaDto Persoana { get; set; }
    }
}
