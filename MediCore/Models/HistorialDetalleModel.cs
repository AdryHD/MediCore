using System;

namespace MediCore.Models
{
    public class HistorialDetalleModel
    {
        public int IdHistorial { get; set; }
        public int IdExpediente { get; set; }
        public int? IdCita { get; set; }
        // Datos del paciente
        public string NombrePaciente { get; set; }
        public string CedulaPaciente { get; set; }
        public string SexoPaciente { get; set; }
        // Datos de la consulta
        public DateTime FechaConsulta { get; set; }
        public string NombreDoctor { get; set; }
        public string NombreEspecialidad { get; set; }
        public string Sintomas { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
        public string Medicamentos { get; set; }
        public string Observaciones { get; set; }
        public DateTime? ProximaCita { get; set; }
    }
}
