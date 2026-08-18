using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MediCore.Models
{
    public class ArchivosFormModel
    {
        public int Id_Archivo { get; set; }

        [Required(ErrorMessage = "El expediente es requerido.")]
        public int? Id_Expediente { get; set; }

        public int? Id_Usuario { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(255)]
        public string Nombre { get; set; }

        public string Tipo_mime { get; set; }

        public long Tamano_bytes { get; set; }

        public byte[] Contenido { get; set; }

        public string Estado { get; set; }

        public DateTime Fecha_carga { get; set; }
    }
}