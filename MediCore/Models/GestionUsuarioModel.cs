namespace MediCore.Models
{
    /// <summary>
    /// ViewModel utilizado en la pantalla de administración de roles (Usuarios)
    /// para listar a los usuarios registrados junto con su rol actual.
    /// </summary>
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
