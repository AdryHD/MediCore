using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediCore.Servicios
{
    public class UtilitarioService
    {
        private int? ObtenerIdUsuarioActual()
        {
            if (HttpContext.Current?.Session != null)
            {
                return HttpContext.Current.Session["Consecutivo"] as int?;
            }
            return null;
        }

        public int? ObtenerIdDoctor()
        {
            return HttpContext.Current?.Session?["IdDoctor"] as int?;
        }

        public bool EsDoctor()
        {
            var rol = (HttpContext.Current?.Session?["NombreRol"] as string ?? "").ToUpper();
            return rol == "DOCTOR";
        }

        private string ObtenerIp()
        {
            return HttpContext.Current?.Request?.UserHostAddress;
        }

        // Método principal de 3 parámetros que están llamando Citas, Pacientes, Doctores, etc.
        public void RegistrarEvento(string controlador, string accion, string mensaje)
        {
            try
            {
                using (var dbContext = new MediCoreEntities())
                {
                    dbContext.spRegistrarBitacora(
                        "INFO",
                        ObtenerIdUsuarioActual(),
                        controlador,
                        accion,
                        mensaje,
                        null,
                        ObtenerIp()
                    );
                }
            }
            catch
            {
            }
        }

        public void RegistrarErrorBitacora(Exception ex, string controlador, string accion)
        {
            try
            {
                using (var dbContext = new MediCoreEntities())
                {
                    dbContext.spRegistrarBitacora(
                        "ERROR",
                        ObtenerIdUsuarioActual(),
                        controlador,
                        accion,
                        ex.Message,
                        ex.StackTrace,
                        ObtenerIp()
                    );
                }
            }
            catch
            {
            }
        }
    }
}