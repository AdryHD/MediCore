using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    public class HomeController : Controller
    {
        private const string NombreControlador = "Home";
        private readonly EmailService _emailService = new EmailService();

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                TempData["Error"] = "Debe ingresar correo y contraseña.";
                return View();
            }

            var correoLimpio = correo.Trim();

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var usuario = db.tbUsuario.FirstOrDefault(u => u.Correo == correoLimpio && u.Contrasenna == contrasena);

                    if (usuario == null)
                    {
                        TempData["Error"] = "Correo o contraseña incorrectos.";
                        return View();
                    }

                    if (!usuario.Estado)
                    {
                        TempData["Error"] = "El usuario se encuentra inactivo. Contacte al administrador.";
                        return View();
                    }

                    if (usuario.FechaExpiracionTemp.HasValue && DateTime.Now > usuario.FechaExpiracionTemp.Value)
                    {
                        TempData["Error"] = "La contraseña temporal expiró. Solicita una nueva recuperación de acceso.";
                        return View();
                    }

                    Session["Consecutivo"] = usuario.Consecutivo;
                    Session["Nombre"] = usuario.Nombre;
                    Session["NombreRol"] = ObtenerNombreRol(db, usuario.id_rol);

                    RegistrarEvento(db, usuario.Consecutivo, "Index", string.Format("Inicio de sesión exitoso para el correo '{0}'.", correoLimpio));

                    if (usuario.FechaExpiracionTemp.HasValue)
                    {
                        TempData["Info"] = "Recuerde cambiar su contraseña temporal.";
                        return RedirectToAction("Index", "Perfil");
                    }

                    return RedirectToAction("Principal");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, null, "Index", ex);
                    TempData["Error"] = "Ocurrió un error al iniciar sesión. Intente nuevamente.";
                    return View();
                }
            }
        }

        [HttpGet]
        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registro(UsuarioModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var correoLimpio = model.Correo.Trim();
                    var cedulaLimpia = model.Cedula.Trim();

                    bool existeUsuario = db.tbUsuario.Any(u => u.Correo == correoLimpio || u.Cedula == cedulaLimpia);

                    if (existeUsuario)
                    {
                        ModelState.AddModelError("", "Ya existe un usuario registrado con ese correo o cédula.");
                        return View(model);
                    }

                    // Todo usuario interno debe tener un rol. Quien se registra por este formulario público
                    // ingresa con el rol de menor privilegio (RECEPCIONISTA); el administrador puede cambiarlo luego.
                    var idRolRecepcionista = db.Database.SqlQuery<int>(
                        "SELECT id_rol FROM dbo.tbRol WHERE nombre_rol = 'RECEPCIONISTA'").FirstOrDefault();

                    db.sp_RegistrarUsuario(model.Nombre, cedulaLimpia, model.FechaNacimiento, model.Telefono, correoLimpio, model.Contrasenna, idRolRecepcionista);

                    RegistrarEvento(db, null, "Registro", string.Format("Usuario registrado con correo '{0}' (Cédula: {1}).", correoLimpio, cedulaLimpia));

                    // El envío del correo de bienvenida nunca debe bloquear ni revertir el registro (RF-15).
                    var usuarioRegistrado = db.tbUsuario.FirstOrDefault(u => u.Correo == correoLimpio);
                    bool correoEnviado = await _emailService.EnviarBienvenida(correoLimpio, model.Nombre, usuarioRegistrado?.Consecutivo);

                    TempData["Success"] = correoEnviado
                        ? "Usuario registrado correctamente. Ya puede iniciar sesión."
                        : "Usuario registrado correctamente, aunque no fue posible enviar el correo de bienvenida. Ya puede iniciar sesión.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, null, "Registro", ex);
                    ModelState.AddModelError("", "Ocurrió un error al registrar el usuario. Intente nuevamente.");
                    return View(model);
                }
            }
        }

        // GET: Recuperar Acceso
        public ActionResult RecuperarAcceso()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecuperarAcceso(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                TempData["Error"] = "Debe ingresar un correo electrónico válido.";
                return View();
            }

            var correoLimpio = correo.Trim();

            using (var db = new MediCoreEntities())
            {
                var usuario = db.tbUsuario.FirstOrDefault(u => u.Correo == correoLimpio);

                if (usuario != null)
                {
                    try
                    {
                        var contrasennaTemporal = GenerarContrasennaTemporal();
                        var expiracion = DateTime.Now.AddMinutes(30);

                        usuario.Contrasenna = contrasennaTemporal;
                        usuario.FechaExpiracionTemp = expiracion;
                        db.SaveChanges();

                        await _emailService.EnviarRecuperacion(correoLimpio, usuario.Nombre, contrasennaTemporal, expiracion, usuario.Consecutivo);

                        RegistrarEvento(db, usuario.Consecutivo, "RecuperarAcceso", string.Format("Contraseña temporal generada para el correo '{0}'.", correoLimpio));
                    }
                    catch (Exception ex)
                    {
                        RegistrarError(db, usuario.Consecutivo, "RecuperarAcceso", ex);
                        TempData["Error"] = "No se pudo procesar la recuperación en este momento. Intente nuevamente.";
                        return View();
                    }
                }
            }

            TempData["Success"] = "Si el correo está registrado, recibirás instrucciones para recuperar el acceso.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Panel principal (requiere autenticación)
        [AuthActionFilter]
        public ActionResult Principal()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            // Cerrar sesión y redirigir al login
            return RedirectToAction("Index");
        }

        private string GenerarContrasennaTemporal()
        {
            const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var random = new Random();
            var nuevaContrasenna = new char[8];

            for (int i = 0; i < nuevaContrasenna.Length; i++)
            {
                nuevaContrasenna[i] = caracteres[random.Next(caracteres.Length)];
            }

            return new string(nuevaContrasenna);
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