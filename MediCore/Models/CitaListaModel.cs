using System;

namespace MediCore.Models
{
    public class CitaListaModel
    {
        public int IdCita { get; set; }
        public string NombrePaciente { get; set; }
        public string NombreDoctor { get; set; }
        public DateTime FechaCita { get; set; }
        public int DuracionMin { get; set; }
        public string Estado { get; set; }
    }
}