using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using PortalAcademico.Services;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;

        public CursosController(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<IActionResult> Catalogo(CatalogoViewModel filtros)
        {
            // Guardar último curso visitado en sesión (para el layout)
            if (filtros.Cursos?.Any() == true)
            {
                var primerCurso = filtros.Cursos.First();
                HttpContext.Session.SetString("UltimoCursoVisitado", primerCurso.Nombre);
                HttpContext.Session.SetInt32("UltimoCursoId", primerCurso.Id);
            }

            List<Curso> cursos;

            // Intentar obtener del cache primero
            var cachedCursos = await _cacheService.GetCursosActivosAsync();
            if (cachedCursos != null)
            {
                cursos = cachedCursos;
            }
            else
            {
                // Si no hay cache, obtener de la base de datos
                cursos = await _context.Cursos
                    .Where(c => c.Activo)
                    .Include(c => c.Matriculas)
                    .ToListAsync();
                
                // Almacenar en cache
                await _cacheService.SetCursosActivosAsync(cursos);
            }

            // Aplicar filtros en memoria
            var query = cursos.AsQueryable();

            if (!string.IsNullOrEmpty(filtros.NombreFiltro))
            {
                query = query.Where(c => c.Nombre.Contains(filtros.NombreFiltro, StringComparison.OrdinalIgnoreCase));
            }

            if (filtros.CreditosMin.HasValue)
            {
                query = query.Where(c => c.Creditos >= filtros.CreditosMin.Value);
            }

            if (filtros.CreditosMax.HasValue)
            {
                query = query.Where(c => c.Creditos <= filtros.CreditosMax.Value);
            }

            if (filtros.HorarioDesde.HasValue && filtros.HorarioDesde.Value != TimeSpan.Zero)
            {
                query = query.Where(c => c.HorarioInicio >= filtros.HorarioDesde.Value);
            }

            if (filtros.HorarioHasta.HasValue && filtros.HorarioHasta.Value != TimeSpan.Zero)
            {
                query = query.Where(c => c.HorarioFin <= filtros.HorarioHasta.Value);
            }

            filtros.Cursos = query.ToList();
            return View(filtros);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            // Guardar último curso visitado en sesión
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
                
            if (curso == null)
            {
                return NotFound();
            }

            HttpContext.Session.SetString("UltimoCursoVisitado", curso.Nombre);
            HttpContext.Session.SetInt32("UltimoCursoId", curso.Id);

            return View(curso);
        }
    }
}