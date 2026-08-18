using System;

namespace MediCore.Models
{
    public class PacienteDetalleModel
    {
        public int IdPaciente { get; set; }
        public string NombreCompleto { get; set; }
        public string Cedula { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Direccion { get; set; }
        public string Estado { get; set; }

        public string TipoSangre { get; set; }
        public string Alergias { get; set; }
        public string Antecedentes { get; set; }
    }
}