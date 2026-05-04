using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.DataLayer.Models;

namespace LicentaInAngular.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets for the entities
        public DbSet<User> Users { get; set; }
        public DbSet<Produs> Products { get; set; }
        public DbSet<Persoana> Persoane { get; set; }
        public DbSet<Adresa> Adrese { get; set; }
        public DbSet<Carduri> Carduri { get; set; }
        public DbSet<Comanda> Comenzi { get; set; }
        public DbSet<Subcomanda> Subcomenzi { get; set; }
        public DbSet<Cos> Cosuri { get; set; }
        public DbSet<Subprodus> SubProduse { get; set; }
        public DbSet<Vopsea> Vopsele { get; set; }
        public DbSet<Adrese_Useri> Adrese_Useri { get; set; }
        public DbSet<Categorii> Categorii { get; set; }
        public DbSet<Imagini> Imagini { get; set; }
        public DbSet<Preturi_Produs> Preturi_Produs { get; set; }
        public DbSet<Judete> Judete { get; set; }
        public DbSet<Localitati> Localitati { get; set; }
        public DbSet<Strazi> Strazi { get; set; }
        public DbSet<Tari> Tari { get; set; }
        public DbSet<Useri_Carduri> Useri_Carduri { get; set; }
        public DbSet<Marca> Marci { get; set; }
        public DbSet<Model> Modele { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Adrese_Useri>(entity =>
            {
                entity.HasKey(au => au.idAU);

                entity.HasOne(au => au.Adrese)
                      .WithMany() 
                      .HasForeignKey(au => au.IdAdresa)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(au => au.User)
                      .WithMany() 
                      .HasForeignKey(au => au.IdUser)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Useri_Carduri>(entity =>
            {
                entity.HasKey(uc => uc.idUC);

                entity.HasOne(uc => uc.User)
                      .WithMany()
                      .HasForeignKey(uc => uc.IdUser)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uc => uc.Card)
                      .WithMany()
                      .HasForeignKey(uc => uc.IdCard)
                      .OnDelete(DeleteBehavior.Cascade);
            });


        }


    }
}
