using System;
using System.Collections.Generic;

namespace MediCore.Models
{
    public class ExpedienteDetalleModel
    {
        public int IdExpediente { get; set; }

        public string NombrePaciente { get; set; }
        public string CedulaPaciente { get; set; }
        public DateTime FechaNacimientoPaciente { get; set; }
        public string SexoPaciente { get; set; }
        public string TelefonoPaciente { get; set; }
        public string CorreoPaciente { get; set; }
        public string DireccionPaciente { get; set; }

        public string TipoSangre { get; set; }
        public DateTime FechaApertura { get; set; }
        public string Alergias { get; set; }
        public string Antecedentes { get; set; }
        public int ConsultasCount { get; set; }
        public IEnumerable<HistorialResumenModel> Historial { get; set; }
    }

    public class HistorialResumenModel
    {
        public int IdHistorial { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string NombreDoctor { get; set; }
        public string Diagnostico { get; set; }
        public DateTime? ProximaCita { get; set; }
    }
}