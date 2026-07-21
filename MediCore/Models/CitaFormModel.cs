using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MediCore.Models
{
    /// <summary>
    /// ViewModel utilizado para registrar y editar citas médicas.
    /// Se mantiene separado de la entidad EF para aplicar validaciones
    /// y facilitar la carga de listas desplegables.
    /// </summary>
    public class CitaFormModel
    {
        public int IdCita { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un paciente.")]
        [Display(Name = "Paciente")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un paciente.")]
        public int IdPaciente { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una especialidad.")]
        [Display(Name = "Especialidad")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una especialidad.")]
        public int IdEspecialidad { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un doctor.")]
        [Display(Name = "Doctor")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un doctor.")]
        public int IdDoctor { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una fecha.")]
        [Display(Name = "Fecha de la cita")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una hora.")]
        [Display(Name = "Hora")]
        public TimeSpan Hora { get; set; }

        [Required]
        public int DuracionMinutos { get; set; }

        [Display(Name = "Motivo de consulta")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        public string Motivo { get; set; }

        public string Estado { get; set; }

        // ---------- Combos ----------

        public IEnumerable<SelectListItem> Pacientes { get; set; }

        public IEnumerable<SelectListItem> Especialidades { get; set; }

        public IEnumerable<SelectListItem> Doctores { get; set; }

        public IEnumerable<SelectListItem> HorariosDisponibles { get; set; }
    }
}