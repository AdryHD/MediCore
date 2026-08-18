using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{

    public class PerfilModel
    {
        public int Consecutivo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(250, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 250 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        public string Cedula { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string NombreRol { get; set; }
    }

    public class CambiarContrasennaModel
    {
        [Required(ErrorMessage = "Debe ingresar la contraseña nueva.")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres (máximo 10).")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña nueva")]
        public string ContrasennaNueva { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña nueva.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarContrasenna { get; set; }
    }
}