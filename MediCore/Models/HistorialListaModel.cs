using System;

namespace MediCore.Models
{
    public class HistorialListaModel
    {
        public int IdHistorial { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string NombrePaciente { get; set; }
        public string NombreDoctor { get; set; }
        public string NombreEspecialidad { get; set; }
        public string Diagnostico { get; set; }
    }
}
