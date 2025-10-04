using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PortalAcademico.Models
{
    public class Matricula
    {
        public int Id { get; set; }
        
        public int CursoId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
        
        // Navigation properties
        public Curso? Curso { get; set; }
        public IdentityUser? Usuario { get; set; }
    }

    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }
}