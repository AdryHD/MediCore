using System;

namespace MediCore.Models
{
    public class ArchivoDetalleModel
    {
        public int IdArchivo { get; set; }
        public string Nombre { get; set; }
        public string Estado { get; set; }
        public string TipoMime { get; set; }
        public long TamanioBytes { get; set; }
        public DateTime FechaCarga { get; set; }
        public int? IdExpediente { get; set; }
        public string NombrePaciente { get; set; }
        public int? IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public bool TieneContenido { get; set; }
    }
}
