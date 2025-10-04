using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models; // Añade este namespace según tu proyecto

namespace PortalAcademico.Data // Asegúrate que coincida con tu namespace
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Curso> Cursos => Set<Curso>();
        public DbSet<Matricula> Matriculas => Set<Matricula>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuraciones
            builder.Entity<Curso>(entity =>
            {
                entity.HasIndex(c => c.Codigo).IsUnique();
                
                // Configuración alternativa para la restricción de horario
                entity.ToTable(tb => tb.HasCheckConstraint(
                    "CK_Curso_Horario", 
                    "HorarioInicio < HorarioFin"));
            });

            builder.Entity<Matricula>(entity =>
            {
                entity.HasIndex(m => new { m.CursoId, m.UsuarioId }).IsUnique();
            });
        }
    }
}