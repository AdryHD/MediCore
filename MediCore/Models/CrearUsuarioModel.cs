using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    public class CrearUsuarioModel
    {
        // Rol destino (se pasa por query string y hidden field)
        public string RolNombre { get; set; }
        public int IdRol { get; set; }

        // Datos comunes de usuario
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public System.DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        public string Cedula { get; set; }

        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; }

        // Campos exclusivos para DOCTOR
        public string CodigoColegiado { get; set; }
        public int? IdEspecialidad { get; set; }
    }
}
