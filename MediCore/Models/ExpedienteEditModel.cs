using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    /// <summary>
    /// Modelo para la vista de edición de expediente clínico.
    /// </summary>
    public class ExpedienteEditModel
    {
        public int IdExpediente { get; set; }

        // Campos de solo lectura (visualización, no se persisten desde aquí)
        public string NombrePaciente { get; set; }
        public string CedulaPaciente { get; set; }
        public DateTime FechaApertura { get; set; }

        // Campos editables
        [Display(Name = "Tipo de sangre")]
        public string TipoSangre { get; set; }

        [Display(Name = "Alergias")]
        [StringLength(2000, ErrorMessage = "Las alergias no pueden superar los 2000 caracteres.")]
        public string Alergias { get; set; }

        [Display(Name = "Antecedentes médicos")]
        [StringLength(4000, ErrorMessage = "Los antecedentes no pueden superar los 4000 caracteres.")]
        public string Antecedentes { get; set; }
    }
}
