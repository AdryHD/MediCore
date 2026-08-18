using System;

namespace MediCore.Models
{
    public class CitaDetalleModel
    {
        public int IdCita { get; set; }
        public string NombrePaciente { get; set; }
        public string NombreDoctor { get; set; }
        public string NombreEspecialidad { get; set; }
        public DateTime FechaCita { get; set; }
        public int DuracionMin { get; set; }
        public string Motivo { get; set; }
        public string MotivoCancelacion { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}