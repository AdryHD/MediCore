using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    public class AtencionCitaModel
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public int IdDoctor { get; set; }
        public int IdExpediente { get; set; }

        public string NombrePaciente { get; set; }
        public string NombreDoctor { get; set; }
        public DateTime FechaCita { get; set; }
        public string Motivo { get; set; }

        [Display(Name = "Síntomas")]
        public string Sintomas { get; set; }

        [Required(ErrorMessage = "El diagnóstico es obligatorio.")]
        [Display(Name = "Diagnóstico")]
        public string Diagnostico { get; set; }

        [Display(Name = "Tratamiento indicado")]
        public string Tratamiento { get; set; }

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; }

        [Display(Name = "Medicamentos recetados")]
        public string Medicamentos { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una acción final.")]
        public string AccionFinal { get; set; }
    }
}