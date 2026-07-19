using MediCore.EF;
using MediCore.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    public class PerfilController : Controller
    {
        private const string NombreControlador = "Perfil";

        // GET: Perfil
        public ActionResult Index()
        {
            using (var db = new MediCoreEntities())
            {
                var idUsuario = ObtenerIdUsuarioActual();
                var usuario = db.tbUsuario.FirstOrDefault(u => u.Consecutivo == idUsuario);

                if (usuario == null)
                {
                    TempData["Error"] = "No se pudo cargar la información del usuario.";
                    return RedirectToAction("Principal", "Home");
                }

                var model = new PerfilModel
                {
                    Consecutivo = usuario.Consecutivo,
                    Nombre = usuario.Nombre,
                    Correo = usuario.Correo,
                    Telefono = usuario.Telefono,
                    Cedula = usuario.Cedula,
                    FechaNacimiento = usuario.FechaNacimiento,
                    NombreRol = ObtenerNombreRol(db, usuario.id_rol)
                };

                return View(model);
            }
        }

        // POST: Perfil/CambiarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarPerfil(PerfilModel model)
        {
            using (var db = new MediCoreEntities())
            {
                var idUsuario = ObtenerIdUsuarioActual();
                var usuario = db.tbUsuario.FirstOrDefault(u => u.Consecutivo == idUsuario);

                if (usuario == null)
                {
                    TempData["Error"] = "No se pudo cargar la información del usuario.";
                    return RedirectToAction("Principal", "Home");
                }

                model.Cedula = usuario.Cedula;
                model.FechaNacimiento = usuario.FechaNacimiento;
                model.NombreRol = ObtenerNombreRol(db, usuario.id_rol);

                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                try
                {
                    var correoLimpio = model.Correo.Trim();

                    bool correoEnUso = db.tbUsuario.Any(u => u.Consecutivo != idUsuario && u.Correo == correoLimpio);

                    if (correoEnUso)
                    {
                        ModelState.AddModelError("Correo", "Ese correo electrónico ya está en uso por otro usuario.");
                        return View("Index", model);
                    }

                    usuario.Nombre = model.Nombre.Trim();
                    usuario.Correo = correoLimpio;
                    usuario.Telefono = model.Telefono.Trim();

                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    Session["Nombre"] = usuario.Nombre;

                    RegistrarEvento(db, idUsuario, "CambiarPerfil", "El usuario actualizó su información de perfil.");

                    TempData["SuccessPerfil"] = "Su información se actualizó correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, idUsuario, "CambiarPerfil", ex);
                    ModelState.AddModelError("", "Ocurrió un error al actualizar su perfil. Intente nuevamente.");
                    return View("Index", model);
                }
            }
        }

        // POST: Perfil/CambiarContrasenna
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarContrasenna(CambiarContrasennaModel model)
        {
            using (var db = new MediCoreEntities())
            {
                var idUsuario = ObtenerIdUsuarioActual();
                var usuario = db.tbUsuario.FirstOrDefault(u => u.Consecutivo == idUsuario);

                if (usuario == null)
                {
                    TempData["Error"] = "No se pudo cargar la información del usuario.";
                    return RedirectToAction("Principal", "Home");
                }

                var perfilModel = new PerfilModel
                {
                    Consecutivo = usuario.Consecutivo,
                    Nombre = usuario.Nombre,
                    Correo = usuario.Correo,
                    Telefono = usuario.Telefono,
                    Cedula = usuario.Cedula,
                    FechaNacimiento = usuario.FechaNacimiento,
                    NombreRol = ObtenerNombreRol(db, usuario.id_rol)
                };

                if (!ModelState.IsValid)
                {
                    return View("Index", perfilModel);
                }

                if (model.ContrasennaNueva != model.ConfirmarContrasenna)
                {
                    ModelState.AddModelError("ConfirmarContrasenna", "Las contraseñas no coinciden.");
                    return View("Index", perfilModel);
                }

                try
                {
                    usuario.Contrasenna = model.ContrasennaNueva;
                    usuario.FechaExpiracionTemp = null;

                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    RegistrarEvento(db, idUsuario, "CambiarContrasenna", "El usuario actualizó su contraseña.");

                    TempData["SuccessSeguridad"] = "Su contraseña se actualizó correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, idUsuario, "CambiarContrasenna", ex);
                    ModelState.AddModelError("", "Ocurrió un error al actualizar su contraseña. Intente nuevamente.");
                    return View("Index", perfilModel);
                }
            }
        }

        private string ObtenerNombreRol(MediCoreEntities db, int? idRol)
        {
            if (!idRol.HasValue)
            {
                return null;
            }

            try
            {
                return db.Database.SqlQuery<string>(
                    "SELECT nombre_rol FROM dbo.tbRol WHERE id_rol = @p0", idRol.Value).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        #region Bitácora

        private int? ObtenerIdUsuarioActual()
        {
            return Session["Consecutivo"] as int?;
        }

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
