using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using PortalAcademico.Services;

namespace PortalAcademico.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;

        public CoordinadorController(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new CoordinadorViewModel
            {
                Cursos = await _context.Cursos
                    .Include(c => c.Matriculas)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync(),
                MatriculasPendientes = await _context.Matriculas
                    .Include(m => m.Curso)
                    .Include(m => m.Usuario)
                    .Where(m => m.Estado == EstadoMatricula.Pendiente)
                    .OrderBy(m => m.FechaRegistro)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // CRUD de Cursos

        public IActionResult CrearCurso()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCurso(CursoFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validación personalizada: HorarioInicio < HorarioFin
                if (model.HorarioInicio >= model.HorarioFin)
                {
                    ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al horario de inicio.");
                    return View(model);
                }

                // Validación: Código único
                if (await _context.Cursos.AnyAsync(c => c.Codigo == model.Codigo))
                {
                    ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                    return View(model);
                }

                var curso = new Curso
                {
                    Codigo = model.Codigo,
                    Nombre = model.Nombre,
                    Creditos = model.Creditos,
                    CupoMaximo = model.CupoMaximo,
                    HorarioInicio = model.HorarioInicio,
                    HorarioFin = model.HorarioFin,
                    Activo = model.Activo
                };

                _context.Cursos.Add(curso);
                await _context.SaveChangesAsync();

                // Invalidar cache después de crear curso
                await _cacheService.RemoveCursosActivosAsync();

                TempData["Success"] = $"Curso {curso.Nombre} creado exitosamente.";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public async Task<IActionResult> EditarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            var viewModel = new CursoFormViewModel
            {
                Id = curso.Id,
                Codigo = curso.Codigo,
                Nombre = curso.Nombre,
                Creditos = curso.Creditos,
                CupoMaximo = curso.CupoMaximo,
                HorarioInicio = curso.HorarioInicio,
                HorarioFin = curso.HorarioFin,
                Activo = curso.Activo
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(int id, CursoFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validación personalizada: HorarioInicio < HorarioFin
                if (model.HorarioInicio >= model.HorarioFin)
                {
                    ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al horario de inicio.");
                    return View(model);
                }

                // Validación: Código único (excluyendo el curso actual)
                if (await _context.Cursos.AnyAsync(c => c.Codigo == model.Codigo && c.Id != id))
                {
                    ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                    return View(model);
                }

                var curso = await _context.Cursos.FindAsync(id);
                if (curso == null)
                {
                    return NotFound();
                }

                curso.Codigo = model.Codigo;
                curso.Nombre = model.Nombre;
                curso.Creditos = model.Creditos;
                curso.CupoMaximo = model.CupoMaximo;
                curso.HorarioInicio = model.HorarioInicio;
                curso.HorarioFin = model.HorarioFin;
                curso.Activo = model.Activo;

                try
                {
                    _context.Cursos.Update(curso);
                    await _context.SaveChangesAsync();

                    // Invalidar cache después de editar curso
                    await _cacheService.RemoveCursosActivosAsync();

                    TempData["Success"] = $"Curso {curso.Nombre} actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CursoExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = false;
            _context.Cursos.Update(curso);
            await _context.SaveChangesAsync();

            // Invalidar cache después de desactivar curso
            await _cacheService.RemoveCursosActivosAsync();

            TempData["Success"] = $"Curso {curso.Nombre} desactivado exitosamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = true;
            _context.Cursos.Update(curso);
            await _context.SaveChangesAsync();

            // Invalidar cache después de activar curso
            await _cacheService.RemoveCursosActivosAsync();

            TempData["Success"] = $"Curso {curso.Nombre} activado exitosamente.";
            return RedirectToAction("Index");
        }

        // Gestión de Matrículas

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMatricula(int id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null || matricula.Curso == null)
            {
                return NotFound();
            }

            // Validar que no exceda el cupo máximo
            var cuposOcupados = await _context.Matriculas
                .CountAsync(m => m.CursoId == matricula.CursoId && m.Estado == EstadoMatricula.Confirmada);

            if (cuposOcupados >= matricula.Curso.CupoMaximo)
            {
                TempData["Error"] = "No se puede confirmar la matrícula. El curso ha alcanzado su cupo máximo.";
                return RedirectToAction("Index");
            }

            matricula.Estado = EstadoMatricula.Confirmada;
            _context.Matriculas.Update(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula confirmada exitosamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
            {
                return NotFound();
            }

            matricula.Estado = EstadoMatricula.Cancelada;
            _context.Matriculas.Update(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula cancelada exitosamente.";
            return RedirectToAction("Index");
        }

        private async Task<bool> CursoExists(int id)
        {
            return await _context.Cursos.AnyAsync(e => e.Id == id);
        }
    }
}