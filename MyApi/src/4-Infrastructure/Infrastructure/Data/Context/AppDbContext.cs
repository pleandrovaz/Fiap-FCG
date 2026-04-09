using Domain.Entities;
using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Jogo> Jogos { get; set; }
        public DbSet<Promocao> Promocoes { get; set; }
        public DbSet<BibliotecaJogo> BibliotecaJogos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(500);

                entity.Property(e => e.Role)
                    .HasMaxLength(50);

                entity.Property(e => e.Perfil)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasMany(e => e.Biblioteca)
                    .WithOne(b => b.Usuario)
                    .HasForeignKey(b => b.UsuarioId);
            });

            modelBuilder.Entity<Jogo>(entity =>
            {
                entity.ToTable("jogos");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedNever();

                entity.Property(e => e.Nome)
                    .HasMaxLength(200);

                entity.Property(e => e.Descricao)
                    .HasMaxLength(1000);

                entity.Property(e => e.Preco)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasMany(e => e.Biblioteca)
                    .WithOne(b => b.Jogo)
                    .HasForeignKey(b => b.JogoId);

                entity.HasMany(e => e.Promocoes)
                    .WithOne(p => p.Jogo)
                    .HasForeignKey(p => p.IdJogo);
            });

            modelBuilder.Entity<Promocao>(entity =>
            {
                entity.ToTable("promocoes");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedNever();

                entity.Property(e => e.Nome)
                    .HasMaxLength(200);

                entity.Property(e => e.TipoDesconto)
                    .IsRequired();

                entity.Property(e => e.Valor)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasMany(e => e.BibliotecaJogos)
                    .WithOne(b => b.Promocao)
                    .HasForeignKey(b => b.PromocaoId);
            });

            modelBuilder.Entity<BibliotecaJogo>(entity =>
            {
                entity.ToTable("biblioteca_jogos");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedNever();

                entity.Property(e => e.PrecoPago)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");
            });
        }
    }
}
