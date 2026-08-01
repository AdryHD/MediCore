using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    public class HomeController : Controller
    {
        private const string NombreControlador = "Home";

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
        public ActionResult Registro(UsuarioModel model)
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

                    TempData["Success"] = "Usuario registrado correctamente. Ya puede iniciar sesión.";
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
        public ActionResult RecuperarAcceso(string correo)
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

                        EnviarCorreoRecuperacion(correoLimpio, usuario.Nombre, contrasennaTemporal, expiracion);

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

        private void EnviarCorreoRecuperacion(string correoDestino, string nombreUsuario, string contrasennaTemporal, DateTime expiracion)
        {
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPortText = ConfigurationManager.AppSettings["SmtpPort"];
            var smtpEnableSslText = ConfigurationManager.AppSettings["SmtpEnableSsl"];
            var smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            var smtpAppPassword = ConfigurationManager.AppSettings["SmtpAppPassword"];
            var fromName = ConfigurationManager.AppSettings["SmtpFromName"];

            if (string.IsNullOrWhiteSpace(smtpHost)
                || string.IsNullOrWhiteSpace(smtpPortText)
                || string.IsNullOrWhiteSpace(smtpEnableSslText)
                || string.IsNullOrWhiteSpace(smtpUser)
                || string.IsNullOrWhiteSpace(smtpAppPassword))
            {
                throw new InvalidOperationException("La configuración SMTP está incompleta en Web.config.");
            }

            var smtpPort = int.Parse(smtpPortText);
            var smtpEnableSsl = bool.Parse(smtpEnableSslText);

            var nombreMostrar = string.IsNullOrWhiteSpace(nombreUsuario) ? "usuario" : nombreUsuario;
            var horaExpiracion = expiracion.ToString("HH:mm");

            var valoresPlantilla = new Dictionary<string, string>
            {
                { "NombreUsuario", nombreMostrar },
                { "ContrasennaTemporal", contrasennaTemporal },
                { "HoraExpiracion", horaExpiracion }
            };

            var cuerpoHtml = CargarPlantillaCorreo("RecuperacionAcceso.html", valoresPlantilla);
            var cuerpoTexto = CargarPlantillaCorreo("RecuperacionAcceso.txt", valoresPlantilla);

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(smtpUser, string.IsNullOrWhiteSpace(fromName) ? "MediCore" : fromName);
                message.To.Add(correoDestino);
                message.Subject = "Recuperación de acceso - MediCore";
                message.Body = cuerpoTexto;

                var vistaHtml = AlternateView.CreateAlternateViewFromString(cuerpoHtml, null, "text/html");
                message.AlternateViews.Add(vistaHtml);

                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.EnableSsl = smtpEnableSsl;
                    smtp.Credentials = new NetworkCredential(smtpUser, smtpAppPassword);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
        }

        /// <summary>
        /// Carga el contenido de una plantilla de correo desde MediCore/EmailTemplates
        /// y reemplaza los tokens {{Clave}} por los valores provistos. Las plantillas se
        /// mantienen como archivos externos (.html/.txt) para no mezclar marcado con código C#.
        /// </summary>
        private string CargarPlantillaCorreo(string nombreArchivo, Dictionary<string, string> valores)
        {
            var ruta = Server.MapPath("~/EmailTemplates/" + nombreArchivo);

            // Se agrega System.IO. para evitar la ambigüedad con el método del Controller
            var contenido = System.IO.File.ReadAllText(ruta);

            foreach (var valor in valores)
            {
                contenido = contenido.Replace("{{" + valor.Key + "}}", valor.Value ?? string.Empty);
            }

            return contenido;
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