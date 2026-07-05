using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediCore.Models
{
    public class UsuarioModel
    {
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Contrasenna { get; set;}
        public string ConfirmarContrasenna { get; set; }
    }
}