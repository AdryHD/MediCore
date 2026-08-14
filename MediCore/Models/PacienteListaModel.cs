using System;

namespace MediCore.Models
{
    public class PacienteListaModel
    {
        public int IdPaciente { get; set; }
        public string NombreCompleto { get; set; }
        public string Cedula { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Estado { get; set; }
    }
}
