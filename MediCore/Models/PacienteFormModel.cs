using System;
using System.ComponentModel.DataAnnotations;

namespace MediCore.Models
{
    /// <summary>
    /// ViewModel utilizado para los formularios de creación y edición de
    /// pacientes (RF-04/RF-09). Combina los datos demográficos (tabla
    /// Pacientes) con los datos clínicos (tabla Expedientes, relación 1 a 1)
    /// en una sola pantalla, sin exponer las entidades EF directamente.
    /// </summary>
    public class PacienteFormModel
    {
        public int IdPaciente { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "La cédula debe tener entre 5 y 20 caracteres.")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio.")]
        [Display(Name = "Sexo")]
        public string Sexo { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string CorreoElectronico { get; set; }

        [StringLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [StringLength(5, ErrorMessage = "El tipo de sangre no puede exceder los 5 caracteres.")]
        [Display(Name = "Tipo de sangre")]
        public string TipoSangre { get; set; }

        [Display(Name = "Alergias")]
        public string Alergias { get; set; }

        [Display(Name = "Antecedentes médicos")]
        public string AntecedentesMedicos { get; set; }
    }
}
