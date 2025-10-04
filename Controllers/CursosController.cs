using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Catalogo(CatalogoViewModel filtros)
        {
            var cursosQuery = _context.Cursos.Where(c => c.Activo);

            // Aplicar filtros que EF puede traducir
            if (!string.IsNullOrEmpty(filtros.NombreFiltro))
            {
                cursosQuery = cursosQuery.Where(c => c.Nombre.Contains(filtros.NombreFiltro));
            }

            if (filtros.CreditosMin.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.Creditos >= filtros.CreditosMin.Value);
            }

            if (filtros.CreditosMax.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.Creditos <= filtros.CreditosMax.Value);
            }

            // Traer los datos y aplicar filtros de horario en memoria
            var cursos = await cursosQuery.ToListAsync();

            // Aplicar filtros de horario en memoria
            if (filtros.HorarioDesde.HasValue && filtros.HorarioDesde.Value != TimeSpan.Zero)
            {
                cursos = cursos.Where(c => c.HorarioInicio >= filtros.HorarioDesde.Value).ToList();
            }

            if (filtros.HorarioHasta.HasValue && filtros.HorarioHasta.Value != TimeSpan.Zero)
            {
                cursos = cursos.Where(c => c.HorarioFin <= filtros.HorarioHasta.Value).ToList();
            }

            filtros.Cursos = cursos;
            return View(filtros);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
                
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }
    }
}