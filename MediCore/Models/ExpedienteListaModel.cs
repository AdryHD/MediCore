using System;

namespace MediCore.Models
{
    public class ExpedienteListaModel
    {
        public int IdExpediente { get; set; }
        public string NombrePaciente { get; set; }
        public string CedulaPaciente { get; set; }
        public string TipoSangre { get; set; }
        public DateTime FechaApertura { get; set; }
        public int ConsultasCount { get; set; }
    }
}
