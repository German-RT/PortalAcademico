namespace PortalAcademico.Models
{
    public class CursoCacheDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public int CupoMaximo { get; set; }
        public TimeSpan HorarioInicio { get; set; }
        public TimeSpan HorarioFin { get; set; }
        public bool Activo { get; set; }
        public int CuposOcupados { get; set; }

        public static CursoCacheDto FromCurso(Curso curso)
        {
            return new CursoCacheDto
            {
                Id = curso.Id,
                Codigo = curso.Codigo,
                Nombre = curso.Nombre,
                Creditos = curso.Creditos,
                CupoMaximo = curso.CupoMaximo,
                HorarioInicio = curso.HorarioInicio,
                HorarioFin = curso.HorarioFin,
                Activo = curso.Activo,
                CuposOcupados = curso.Matriculas?.Count(m => m.Estado == EstadoMatricula.Confirmada) ?? 0
            };
        }
    }
}