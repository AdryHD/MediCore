using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    public class ProgramarSeguimientoModel
    {
        public int IdCita { get; set; }
        public int IdCitaAnterior { get; set; }
        public int IdPaciente { get; set; }
        public int IdDoctor { get; set; }

        // Solo para mostrar
        public string NombrePaciente { get; set; }
        public string NombreDoctor { get; set; }
        public string DiagnosticoAnterior { get; set; }
        public DateTime FechaCitaAnterior { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una fecha.")]
        [Display(Name = "Fecha de la cita")]
        public DateTime? FechaCita { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un horario.")]
        [Display(Name = "Horario")]
        public TimeSpan? HoraCita { get; set; }

        [Display(Name = "Motivo / notas de seguimiento")]
        public string Motivo { get; set; }
    }
}
