using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());
            
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Crear rol Coordinador
            if (!await roleManager.RoleExistsAsync("Coordinador"))
            {
                await roleManager.CreateAsync(new IdentityRole("Coordinador"));
            }

            // Crear usuario coordinador
            var coordinadorEmail = "coordinador@universidad.edu";
            var coordinador = await userManager.FindByEmailAsync(coordinadorEmail);
            
            if (coordinador == null)
            {
                coordinador = new IdentityUser 
                { 
                    UserName = coordinadorEmail, 
                    Email = coordinadorEmail,
                    EmailConfirmed = true
                };
                
                await userManager.CreateAsync(coordinador, "Password123!");
                await userManager.AddToRoleAsync(coordinador, "Coordinador");
            }

            // Crear cursos de ejemplo
            if (!context.Cursos.Any())
            {
                context.Cursos.AddRange(
                    new Curso
                    {
                        Codigo = "MAT101",
                        Nombre = "Matemáticas Básicas",
                        Creditos = 4,
                        CupoMaximo = 30,
                        HorarioInicio = new TimeSpan(8, 0, 0),
                        HorarioFin = new TimeSpan(10, 0, 0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "PROG101",
                        Nombre = "Programación I",
                        Creditos = 5,
                        CupoMaximo = 25,
                        HorarioInicio = new TimeSpan(10, 0, 0),
                        HorarioFin = new TimeSpan(12, 0, 0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "FIS101",
                        Nombre = "Física General",
                        Creditos = 4,
                        CupoMaximo = 20,
                        HorarioInicio = new TimeSpan(14, 0, 0),
                        HorarioFin = new TimeSpan(16, 0, 0),
                        Activo = true
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}