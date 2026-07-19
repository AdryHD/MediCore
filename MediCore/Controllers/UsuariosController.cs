using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    // Pantalla de administración: permite a un ADMINISTRADOR ver a todos los
    // usuarios registrados (incluyendo los que se registraron por el formulario
    // público sin rol asignado) y asignarles/cambiarles el rol correspondiente.
    [AuthActionFilter]
    [AdminActionFilter]
    public class UsuariosController : Controller
    {
        private const string NombreControlador = "Usuarios";

        // GET: Usuarios
        public ActionResult Index()
        {
            using (var db = new MediCoreEntities())
            {
                var roles = ObtenerRoles(db);
                var rolesPorId = roles.ToDictionary(r => r.IdRol, r => r.NombreRol);

                var usuarios = db.tbUsuario
                    .ToList()
                    .OrderBy(u => u.Nombre)
                    .Select(u => new GestionUsuarioModel
                    {
                        Consecutivo = u.Consecutivo,
                        Nombre = u.Nombre,
                        Correo = u.Correo,
                        Cedula = u.Cedula,
                        Estado = u.Estado,
                        IdRol = u.id_rol,
                        NombreRol = (u.id_rol.HasValue && rolesPorId.ContainsKey(u.id_rol.Value))
                            ? rolesPorId[u.id_rol.Value]
                            : null
                    })
                    .ToList();

                ViewBag.Roles = roles;
                return View(usuarios);
            }
        }

        // POST: Usuarios/AsignarRol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsignarRol(int idUsuario, int? idRol)
        {
            using (var db = new MediCoreEntities())
            {
                var idAdmin = Session["Consecutivo"] as int?;
                var usuario = db.tbUsuario.FirstOrDefault(u => u.Consecutivo == idUsuario);

                if (usuario == null)
                {
                    TempData["Error"] = "El usuario indicado no existe.";
                    return RedirectToAction("Index");
                }

                try
                {
                    usuario.id_rol = idRol;

                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    var roles = ObtenerRoles(db);
                    var rolAsignado = roles.FirstOrDefault(r => idRol.HasValue && r.IdRol == idRol.Value);
                    var nombreRolAsignado = rolAsignado != null ? rolAsignado.NombreRol : "SIN ASIGNAR";

                    RegistrarEvento(db, idAdmin, "AsignarRol", string.Format(
                        "Se asignó el rol '{0}' al usuario '{1}' (Consecutivo: {2}).",
                        nombreRolAsignado, usuario.Nombre, usuario.Consecutivo));

                    TempData["Success"] = string.Format("Rol actualizado correctamente para {0}.", usuario.Nombre);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, idAdmin, "AsignarRol", ex);
                    TempData["Error"] = "Ocurrió un error al actualizar el rol. Intente nuevamente.";
                }

                return RedirectToAction("Index");
            }
        }

        private List<RolItem> ObtenerRoles(MediCoreEntities db)
        {
            try
            {
                return db.Database.SqlQuery<RolItem>(
                    "SELECT id_rol AS IdRol, nombre_rol AS NombreRol FROM dbo.tbRol ORDER BY nombre_rol").ToList();
            }
            catch
            {
                return new List<RolItem>();
            }
        }

        #region Bitácora

        private string ObtenerIp()
        {
            return Request != null ? Request.UserHostAddress : null;
        }

        private void RegistrarEvento(MediCoreEntities db, int? idUsuario, string accion, string mensaje)
        {
            try
            {
                db.spRegistrarBitacora("INFO", idUsuario, NombreControlador, accion, mensaje, null, ObtenerIp());
            }
            catch
            {
                // La bitácora nunca debe interrumpir el flujo principal de la aplicación.
            }
        }

        private void RegistrarError(MediCoreEntities db, int? idUsuario, string accion, Exception ex)
        {
            try
            {
                db.spRegistrarBitacora("ERROR", idUsuario, NombreControlador, accion, ex.Message, ex.StackTrace, ObtenerIp());
            }
            catch
            {
                // La bitácora nunca debe interrumpir el flujo principal de la aplicación.
            }
        }

        #endregion
    }
}
