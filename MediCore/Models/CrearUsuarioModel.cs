using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    public class CrearUsuarioModel
    {

        public string RolNombre { get; set; }
        public int IdRol { get; set; }

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

        public string CodigoColegiado { get; set; }
        public int? IdEspecialidad { get; set; }
    }
}