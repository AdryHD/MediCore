using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    /// <summary>
    /// ViewModel utilizado para los formularios de creación y edición de
    /// doctores (RF-02). Se mantiene separado de la entidad EF para poder
    /// aplicar validaciones de UI sin afectar el modelo de datos.
    /// </summary>
    public class DoctorFormModel
    {
        public int IdDoctor { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "La cédula profesional es obligatoria.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "La cédula debe tener entre 5 y 20 caracteres.")]
        [Display(Name = "Cédula profesional")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "El código colegiado es obligatorio.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "El código colegiado debe tener entre 3 y 30 caracteres.")]
        [Display(Name = "Código colegiado")]
        public string CodigoColegiado { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una especialidad.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una especialidad.")]
        [Display(Name = "Especialidad principal")]
        public int IdEspecialidad { get; set; }
    }
}
