using FIAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Infraestructure.Context
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Investment> Investments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder model)
    {
      base.OnModelCreating(model);

      model.Entity<Usuario>(b =>
      {
        b.HasKey(u => u.Id);
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.Nome).HasMaxLength(200).IsRequired();
        b.Property(u => u.Email).HasMaxLength(200).IsRequired();
        b.Property(u => u.SenhaHash).HasMaxLength(500).IsRequired();
        b.HasMany(u => u.Investments)
         .WithOne(i => i.Usuario)
         .HasForeignKey(i => i.IdUsuario)
         .IsRequired()
         .OnDelete(DeleteBehavior.Cascade);
      });

      model.Entity<Investment>(b =>
      {
        b.HasKey(i => i.Id);
        b.Property(i => i.TipoInvestment).HasMaxLength(100).IsRequired();
        b.Property(i => i.ValorInvestment).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(i => i.DataInvestment).IsRequired();
      });
    }

  }
}
