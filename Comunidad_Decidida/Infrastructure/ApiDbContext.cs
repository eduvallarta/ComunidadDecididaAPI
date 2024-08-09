using Microsoft.EntityFrameworkCore;
using Comunidad_Decidida.Models;

namespace Comunidad_Decidida.Infrastructure
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public DbSet<Asociado> Asociado { get; set; }
        public DbSet<Direccion> Direccion { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<AsociadoTags> AsociadosTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asociado>()
                .HasKey(a => a.IDAsociado);

            modelBuilder.Entity<Asociado>()
                .HasMany(a => a.Tags)
                .WithOne(t => t.Asociado)
                .HasForeignKey(t => t.IDSAE);

            modelBuilder.Entity<Asociado>()
                .HasMany(a => a.Direcciones)
                .WithOne(d => d.Asociado)
                .HasForeignKey(d => d.IDSAE);

            modelBuilder.Entity<Direccion>()
                .HasKey(d => d.IDDireccion);

            modelBuilder.Entity<Tag>()
                .HasKey(t => t.IDTags);

            modelBuilder.Entity<Tag>()
                .HasOne(t => t.Asociado)
                .WithMany(a => a.Tags)
                .HasForeignKey(t => t.IDSAE);

            modelBuilder.Entity<AsociadoTags>().HasNoKey();
        }
    }
}
