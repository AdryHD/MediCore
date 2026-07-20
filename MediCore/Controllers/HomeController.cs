using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            return RedirectToAction("RecuperarAcceso");
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

            var cuerpoHtml = string.Format(
                @"<!DOCTYPE html>
<html lang=""es"">
<body style=""margin:0;padding:0;background-color:#f2f4f8;font-family:Segoe UI,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f2f4f8;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""480"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);"">
          <tr>
            <td style=""background-color:#2d499d;padding:24px 32px;"">
              <span style=""color:#ffffff;font-size:20px;font-weight:600;"">MediCore</span>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <h2 style=""margin:0 0 16px 0;color:#1f2937;font-size:20px;"">Recuperación de acceso</h2>
              <p style=""margin:0 0 16px 0;color:#374151;font-size:15px;line-height:1.5;"">Hola <strong>{0}</strong>,</p>
              <p style=""margin:0 0 20px 0;color:#374151;font-size:15px;line-height:1.5;"">Hemos generado una contraseña temporal para tu cuenta en MediCore:</p>
              <div style=""background-color:#f2f4f8;border-radius:8px;padding:16px;text-align:center;margin-bottom:20px;"">
                <span style=""font-size:24px;font-weight:700;letter-spacing:2px;color:#2d499d;"">{1}</span>
              </div>
              <div style=""background-color:#fff4e5;border-left:4px solid #f59e0b;border-radius:4px;padding:12px 16px;margin-bottom:20px;"">
                <p style=""margin:0;color:#92400e;font-size:14px;line-height:1.5;"">
                  <strong>⏱ Válida por 30 minutos</strong><br>
                  Vence hoy a las <strong>{2}</strong>. Pasado ese tiempo dejará de funcionar y deberás solicitar una nueva recuperación de acceso.
                </p>
              </div>
              <p style=""margin:0 0 16px 0;color:#374151;font-size:15px;line-height:1.5;"">Te recomendamos iniciar sesión y cambiarla inmediatamente.</p>
              <p style=""margin:0;color:#6b7280;font-size:13px;line-height:1.5;"">Si no solicitaste este cambio, comunícate con el administrador.</p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f9fafb;padding:16px 32px;text-align:center;"">
              <span style=""color:#9ca3af;font-size:12px;"">MediCore &middot; Sistema de Gestión Médica</span>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>",
                nombreMostrar,
                contrasennaTemporal,
                horaExpiracion);

            var cuerpoTexto = string.Format(
                "Hola {0},\n\n" +
                "Hemos generado una contraseña temporal para tu cuenta en MediCore.\n\n" +
                "Contraseña temporal: {1}\n\n" +
                "Válida por 30 minutos (vence hoy a las {2}). Pasado ese tiempo dejará de funcionar y deberás solicitar una nueva recuperación de acceso.\n\n" +
                "Te recomendamos iniciar sesión y cambiarla inmediatamente.\n\n" +
                "Si no solicitaste este cambio, comunícate con el administrador.",
                nombreMostrar,
                contrasennaTemporal,
                horaExpiracion);

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