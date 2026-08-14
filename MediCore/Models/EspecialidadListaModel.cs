using System;

namespace MediCore.Models
{
    public class EspecialidadListaModel
    {
        public int IdEspecialidad { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
