using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    /// <summary>
    /// ViewModel utilizado para los formularios de creación y edición de
    /// horarios médicos (RF-03). Se mantiene separado de la entidad EF para
    /// poder aplicar validaciones de UI sin afectar el modelo de datos.
    /// </summary>
    public class HorarioMedicoFormModel
    {
        public int IdHorario { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un doctor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un doctor.")]
        [Display(Name = "Doctor")]
        public int IdDoctor { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el día de la semana.")]
        [Range(1, 7, ErrorMessage = "El día de la semana debe estar entre Lunes y Domingo.")]
        [Display(Name = "Día de la semana")]
        public byte DiaSemana { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de fin")]
        public TimeSpan HoraFin { get; set; }

        [Required(ErrorMessage = "La duración de la cita es obligatoria.")]
        [Range(15, 480, ErrorMessage = "La duración de la cita debe estar entre 15 y 480 minutos.")]
        [Display(Name = "Duración de la cita (minutos)")]
        public int DuracionCita { get; set; }
    }
}
