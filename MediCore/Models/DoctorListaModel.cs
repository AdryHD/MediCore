namespace MediCore.Models
{
    public class DoctorListaModel
    {
        public int IdDoctor { get; set; }
        public string NombreCompleto { get; set; }
        public string Cedula { get; set; }
        public string CodigoColegiado { get; set; }
        public string NombreEspecialidad { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Estado { get; set; }
    }
}