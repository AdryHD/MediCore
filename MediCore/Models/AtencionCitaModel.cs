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


        // Datos solamente para mostrar
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


        [Display(Name = "Próxima cita de control")]
        [DataType(DataType.Date)]
        public DateTime? ProximaCita { get; set; }
    }
}