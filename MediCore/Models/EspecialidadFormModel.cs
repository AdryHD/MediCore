using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{

    public class EspecialidadFormModel
    {
        public int IdEspecialidad { get; set; }

        [Required(ErrorMessage = "El nombre de la especialidad es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(255, ErrorMessage = "La descripción no puede exceder los 255 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
    }
}