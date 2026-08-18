using System;

namespace MediCore.Models
{
    public class ArchivoListaModel
    {
        public int IdArchivo { get; set; }
        public string Nombre { get; set; }
        public string TipoMime { get; set; }
        public long TamanioBytes { get; set; }
        public DateTime FechaCarga { get; set; }
        public string Estado { get; set; }
    }
}