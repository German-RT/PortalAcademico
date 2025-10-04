using System.Collections.Generic;

namespace PortalAcademico.Models
{
    public class CatalogoViewModel
    {
        public string? NombreFiltro { get; set; }
        public int? CreditosMin { get; set; }
        public int? CreditosMax { get; set; }
        public TimeSpan? HorarioDesde { get; set; }
        public TimeSpan? HorarioHasta { get; set; }
        public List<Curso> Cursos { get; set; } = new();
    }
}