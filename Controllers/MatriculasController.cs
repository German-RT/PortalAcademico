using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Security.Claims;

namespace PortalAcademico.Controllers
{
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(int cursoId)
        {
            // Verificar que el usuario esté autenticado
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                TempData["Error"] = "Debe iniciar sesión para inscribirse en un curso.";
                return RedirectToAction("Login", "Identity", new { area = "Identity" });
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Catalogo", "Cursos");
            }

            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);

            if (curso == null)
            {
                TempData["Error"] = "Curso no encontrado o no disponible.";
                return RedirectToAction("Catalogo", "Cursos");
            }

            // Validación 1: No superar el CupoMaximo
            var matriculasConfirmadas = curso.Matriculas?.Count(m => m.Estado == EstadoMatricula.Confirmada) ?? 0;
            if (matriculasConfirmadas >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ha alcanzado su cupo máximo. No hay cupos disponibles.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Validación 2: No estar matriculado más de una vez en el mismo curso
            var matriculaExistente = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.CursoId == cursoId && m.UsuarioId == usuario.Id);

            if (matriculaExistente != null)
            {
                TempData["Error"] = "Ya está inscrito en este curso.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Validación 3: No solaparse con otro curso ya matriculado en el mismo horario
            var cursosMatriculados = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == usuario.Id && 
                           m.Estado == EstadoMatricula.Confirmada && 
                           m.Curso != null && m.Curso.Activo)
                .Select(m => m.Curso!)
                .ToListAsync();

            var haySolapamiento = cursosMatriculados.Any(c => 
                c != null && (
                (curso.HorarioInicio >= c.HorarioInicio && curso.HorarioInicio < c.HorarioFin) ||
                (curso.HorarioFin > c.HorarioInicio && curso.HorarioFin <= c.HorarioFin) ||
                (curso.HorarioInicio <= c.HorarioInicio && curso.HorarioFin >= c.HorarioFin)));

            if (haySolapamiento)
            {
                TempData["Error"] = "El horario de este curso se solapa con otro curso en el que ya está inscrito.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Crear la matrícula
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = usuario.Id,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            try
            {
                _context.Matriculas.Add(matricula);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"¡Inscripción exitosa! Se ha registrado en {curso.Nombre}. Estado: Pendiente de confirmación.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al procesar la inscripción. Por favor, intente nuevamente.";
            }

            return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
        }

        // Acción para que los usuarios vean sus matrículas
        public async Task<IActionResult> MisMatriculas()
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Identity", new { area = "Identity" });
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return RedirectToAction("Login", "Identity", new { area = "Identity" });
            }
            
            var matriculas = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == usuario.Id && m.Curso != null)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            return View(matriculas);
        }
    }
}