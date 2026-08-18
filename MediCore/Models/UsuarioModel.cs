using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    public class UsuarioModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La cédula o documento de identidad es obligatorio.")]
        [MinLength(9, ErrorMessage = "La cédula debe tener al menos 9 dígitos.")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [MinLength(8, ErrorMessage = "El teléfono debe tener al menos 8 dígitos.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Contrasenna { get; set; }

        [Required(ErrorMessage = "Debe confirmar su contraseña.")]
        [Compare("Contrasenna", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasenna { get; set; }
    }
}