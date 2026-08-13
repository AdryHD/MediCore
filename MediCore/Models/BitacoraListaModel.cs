using System;

namespace MediCore.Models
{
    /// <summary>
    /// DTO para mostrar registros de la bitácora en la vista.
    /// Evita exponer la entidad EF directamente.
    /// </summary>
    public class BitacoraListaModel
    {
        public long IdBitacora { get; set; }
        public DateTime Fecha { get; set; }
        public string Nivel { get; set; }
        public string NombreUsuario { get; set; }
        public string Controlador { get; set; }
        public string Accion { get; set; }
        public string Mensaje { get; set; }
        public string StackTrace { get; set; }
        public string IpOrigen { get; set; }
    }
}
