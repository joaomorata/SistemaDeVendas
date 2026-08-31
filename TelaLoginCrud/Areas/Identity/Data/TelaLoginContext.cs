using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelaLoginCrud.Areas.Identity.Data;
using TelaLoginCrud.Models;

namespace TelaLoginCrud.Areas.Identity.Data;

public class TelaLoginContext : IdentityDbContext<Usuario>
{
    public TelaLoginContext(DbContextOptions<TelaLoginContext> options)
        : base(options)
    {
    }

    public DbSet<Venda> Venda { get; set; }

    public DbSet<Cliente> Cliente { get; set; }

    public DbSet<Pacote> Pacote { get; set; }

    public DbSet<Rota> Rota { get; set; }

    public DbSet<RotaParada> RotaParada { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new ConfigUser());

        builder.Entity<RotaParada>()
            .HasOne(p => p.Cliente)
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public class ConfigUser : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.Property(x => x.Nome).HasMaxLength(150);
            builder.Property(x => x.Sobrenome).HasMaxLength(150);
        }
    }
}
