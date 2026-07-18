using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    /// <summary>
    /// ViewModel utilizado para los formularios de creación y edición de
    /// especialidades médicas (RF-01). Se mantiene separado de la entidad
    /// EF para poder aplicar validaciones de UI sin afectar el modelo de datos.
    /// </summary>
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
