namespace MediCore.Models
{
    public class GestionUsuarioModel
    {
        public int Consecutivo { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Cedula { get; set; }
        public bool Estado { get; set; }
        public int IdRol { get; set; }
        public string NombreRol { get; set; }
    }
}