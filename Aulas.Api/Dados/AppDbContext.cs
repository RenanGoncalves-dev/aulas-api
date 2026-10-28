using Aulas.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Aulas.Api.Dados;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Professor> Professores => Set<Professor>();
    public DbSet<AulaMarcada> AulasMarcadas => Set<AulaMarcada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(160);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.SenhaHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.Papel)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(u => u.CriadoEmUtc)
                .IsRequired();
        });

        modelBuilder.Entity<Professor>(entity =>
        {
            entity.ToTable("Professores");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(p => p.Materia)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(p => p.Descricao)
                .IsRequired()
                .HasMaxLength(600);

            entity.Property(p => p.ValorHora)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(p => p.DuracaoMinutos)
                .IsRequired();

            entity.Property(p => p.Ativo)
                .IsRequired();

            entity.Property(p => p.CriadoEmUtc)
                .IsRequired();
        });

        modelBuilder.Entity<AulaMarcada>(entity =>
        {
            entity.ToTable("AulasMarcadas");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.DataHoraInicioUtc)
                .IsRequired();

            entity.Property(a => a.DataHoraFimUtc)
                .IsRequired();

            entity.Property(a => a.Observacoes)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(a => a.CriadoEmUtc)
                .IsRequired();

            entity.HasOne(a => a.Professor)
                .WithMany(p => p.AulasMarcadas)
                .HasForeignKey(a => a.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
