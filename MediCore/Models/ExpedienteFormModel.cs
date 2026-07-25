using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MediCore.Models
{
    public class ExpedienteFormModel
    {
        public int IdExpediente { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un paciente.")]
        [Display(Name = "Paciente")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un paciente.")]
        public int IdPaciente { get; set; }

        [Display(Name = "Tipo de sangre")]
        public string TipoSangre { get; set; }

        [Display(Name = "Alergias")]
        [StringLength(2000, ErrorMessage = "Las alergias no pueden superar los 2000 caracteres.")]
        public string Alergias { get; set; }

        [Display(Name = "Antecedentes médicos")]
        [StringLength(4000, ErrorMessage = "Los antecedentes no pueden superar los 4000 caracteres.")]
        public string Antecedentes { get; set; }

        public IEnumerable<SelectListItem> Pacientes { get; set; }
    }
}