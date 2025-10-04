using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models
{
    public class CoordinadorViewModel
    {
        public List<Curso> Cursos { get; set; } = new();
        public List<Matricula> MatriculasPendientes { get; set; } = new();
    }

    public class CursoFormViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(10, ErrorMessage = "El código no puede tener más de 10 caracteres")]
        public string Codigo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        
        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
        public int Creditos { get; set; }
        
        [Range(1, 100, ErrorMessage = "El cupo máximo debe estar entre 1 y 100")]
        public int CupoMaximo { get; set; }
        
        [DataType(DataType.Time)]
        public TimeSpan HorarioInicio { get; set; }
        
        [DataType(DataType.Time)]
        public TimeSpan HorarioFin { get; set; }
        
        public bool Activo { get; set; } = true;
    }
}