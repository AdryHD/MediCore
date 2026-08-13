using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    // Pantalla de administración: permite a un ADMINISTRADOR ver a todos los
    // usuarios internos registrados y cambiarles el rol correspondiente
    // (ADMINISTRADOR, DOCTOR o RECEPCIONISTA).
    [AuthActionFilter]
    [AdminActionFilter]
    public class UsuariosController : Controller
    {
        private const string NombreControlador = "Usuarios";

        // Instancia del servicio de bitácora y utilitarios
        private readonly UtilitarioService _utilitarioService = new UtilitarioService();

        // GET: Usuarios
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Usuarios";
            ViewBag.UsuarioActual = Session["Consecutivo"] as int?;

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
                        NombreRol = rolesPorId.ContainsKey(u.id_rol) ? rolesPorId[u.id_rol] : null
                    })
                    .ToList();

                ViewBag.Roles = roles;
                return View(usuarios);
            }
        }

        // POST: Usuarios/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int idUsuario)
        {
            using (var db = new MediCoreEntities())
            {
                var usuario = db.tbUsuario.FirstOrDefault(u => u.Consecutivo == idUsuario);

                if (usuario == null)
                {
                    TempData["Error"] = "El usuario indicado no existe.";
                    return RedirectToAction("Index");
                }

                int? idActual = Session["Consecutivo"] as int?;
                if (idActual.HasValue && idActual.Value == idUsuario)
                {
                    TempData["Error"] = "No puedes cambiar el estado de tu propia cuenta.";
                    return RedirectToAction("Index");
                }

                try
                {
                    usuario.Estado = !usuario.Estado;
                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    _utilitarioService.RegistrarEvento(
                        NombreControlador,
                        "CambiarEstado",
                        string.Format("Se {0} al usuario '{1}' (Consecutivo: {2}).",
                            usuario.Estado ? "activó" : "desactivó", usuario.Nombre, usuario.Consecutivo)
                    );

                    TempData["Success"] = string.Format("Usuario '{0}' {1} correctamente.",
                        usuario.Nombre, usuario.Estado ? "activado" : "desactivado");
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "CambiarEstado");
                    TempData["Error"] = "Ocurrió un error al cambiar el estado del usuario.";
                }

                return RedirectToAction("Index");
            }
        }

        // POST: Usuarios/AsignarRol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsignarRol(int idUsuario, int idRol)
        {
            using (var db = new MediCoreEntities())
            {
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
                    var rolAsignado = roles.FirstOrDefault(r => r.IdRol == idRol);
                    var nombreRolAsignado = rolAsignado != null ? rolAsignado.NombreRol : "DESCONOCIDO";

                    // Registro de bitácora usando UtilitarioService
                    _utilitarioService.RegistrarEvento(
                        NombreControlador,
                        "AsignarRol",
                        string.Format("Se asignó el rol '{0}' al usuario '{1}' (Consecutivo: {2}).",
                            nombreRolAsignado, usuario.Nombre, usuario.Consecutivo)
                    );

                    TempData["Success"] = string.Format("Rol actualizado correctamente para {0}.", usuario.Nombre);
                }
                catch (Exception ex)
                {
                    // Registro de excepción usando UtilitarioService
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "AsignarRol");

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
            catch (Exception ex)
            {
                _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "ObtenerRoles");
                return new List<RolItem>();
            }
        }
    }
}

