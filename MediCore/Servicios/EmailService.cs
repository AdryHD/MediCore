using MediCore.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace MediCore.Servicios
{
    /// <summary>
    /// Servicio centralizado de envío de correos (RF-15). Reutiliza la configuración SMTP
    /// de Web.config, las plantillas externas de MediCore/EmailTemplates y registra cada
    /// intento de envío en dbo.Notificaciones. Los errores de envío nunca se propagan al
    /// llamador: se registran en la Bitácora (vía UtilitarioService) y en Notificaciones
    /// como FALLIDO, para que la operación principal (registro, cita, etc.) no se vea afectada.
    /// </summary>
    public class EmailService
    {
        private readonly UtilitarioService _utilitarioService = new UtilitarioService();

        /// <summary>
        /// Envía un correo genérico de forma asíncrona y registra el resultado en
        /// dbo.Notificaciones. No lanza excepciones: si algo falla, queda registrado
        /// como FALLIDO en Notificaciones y como ERROR en la Bitácora.
        /// </summary>
        public async Task<bool> EnviarCorreoAsync(string correoDestino, string asunto, string cuerpoHtml, string cuerpoTexto, string tipoNotificacion, int? idUsuarioDestino = null)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || !EsCorreoValido(correoDestino))
            {
                _utilitarioService.RegistrarEvento("EmailService", tipoNotificacion, string.Format("No se envió el correo '{0}': el destinatario está vacío o no es válido.", correoDestino));
                return false;
            }

            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPortText = ConfigurationManager.AppSettings["SmtpPort"];
            var smtpEnableSslText = ConfigurationManager.AppSettings["SmtpEnableSsl"];
            var smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            var smtpAppPassword = ConfigurationManager.AppSettings["SmtpAppPassword"];
            var fromName = ConfigurationManager.AppSettings["SmtpFromName"];

            int smtpPort = 0;
            bool smtpEnableSsl = false;

            bool configuracionCompleta =
                !string.IsNullOrWhiteSpace(smtpHost)
                && !string.IsNullOrWhiteSpace(smtpUser)
                && !string.IsNullOrWhiteSpace(smtpAppPassword)
                && int.TryParse(smtpPortText, out smtpPort)
                && bool.TryParse(smtpEnableSslText, out smtpEnableSsl);

            if (!configuracionCompleta)
            {
                _utilitarioService.RegistrarEvento("EmailService", tipoNotificacion, "La configuración SMTP está incompleta en Web.config. No se envió el correo.");
                RegistrarNotificacion(correoDestino, tipoNotificacion, asunto, cuerpoHtml, "FALLIDO", idUsuarioDestino);
                return false;
            }

            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(smtpUser, string.IsNullOrWhiteSpace(fromName) ? "MediCore" : fromName);
                    message.To.Add(correoDestino);
                    message.Subject = asunto;
                    message.Body = cuerpoTexto;

                    var vistaHtml = AlternateView.CreateAlternateViewFromString(cuerpoHtml, null, "text/html");
                    message.AlternateViews.Add(vistaHtml);

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.EnableSsl = smtpEnableSsl;
                        smtp.Credentials = new NetworkCredential(smtpUser, smtpAppPassword);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                        await smtp.SendMailAsync(message);
                    }
                }

                RegistrarNotificacion(correoDestino, tipoNotificacion, asunto, cuerpoHtml, "ENVIADO", idUsuarioDestino);

                return true;
            }
            catch (Exception ex)
            {
                _utilitarioService.RegistrarErrorBitacora(ex, "EmailService", tipoNotificacion);
                RegistrarNotificacion(correoDestino, tipoNotificacion, asunto, cuerpoHtml, "FALLIDO", idUsuarioDestino);
                return false;
            }
        }

        public Task<bool> EnviarBienvenida(string correoDestino, string nombreUsuario, int? idUsuarioDestino = null)
        {
            var valores = new Dictionary<string, string>
            {
                { "NombreUsuario", string.IsNullOrWhiteSpace(nombreUsuario) ? "usuario" : nombreUsuario },
                { "Correo", correoDestino }
            };

            var cuerpoHtml = CargarPlantillaCorreo("Bienvenida.html", valores);
            var cuerpoTexto = CargarPlantillaCorreo("Bienvenida.txt", valores);

            return EnviarCorreoAsync(correoDestino, "Bienvenido a MediCore", cuerpoHtml, cuerpoTexto, "REGISTRO", idUsuarioDestino);
        }

        public Task<bool> EnviarRecuperacion(string correoDestino, string nombreUsuario, string contrasennaTemporal, DateTime expiracion, int? idUsuarioDestino = null)
        {
            var valores = new Dictionary<string, string>
            {
                { "NombreUsuario", string.IsNullOrWhiteSpace(nombreUsuario) ? "usuario" : nombreUsuario },
                { "ContrasennaTemporal", contrasennaTemporal },
                { "HoraExpiracion", expiracion.ToString("HH:mm") }
            };

            var cuerpoHtml = CargarPlantillaCorreo("RecuperacionAcceso.html", valores);
            var cuerpoTexto = CargarPlantillaCorreo("RecuperacionAcceso.txt", valores);

            return EnviarCorreoAsync(correoDestino, "Recuperación de acceso - MediCore", cuerpoHtml, cuerpoTexto, "RECUPERACION", idUsuarioDestino);
        }

        public Task<bool> EnviarConfirmacionCita(string correoDestino, string nombrePaciente, string nombreDoctor, string especialidad, int numeroCita, DateTime fecha, TimeSpan hora, string estado)
        {
            var cuerpoHtml = CargarPlantillaCorreo("CitaConfirmacion.html", ArmarValoresCita(nombrePaciente, nombreDoctor, especialidad, numeroCita, fecha, hora, estado));
            var cuerpoTexto = CargarPlantillaCorreo("CitaConfirmacion.txt", ArmarValoresCita(nombrePaciente, nombreDoctor, especialidad, numeroCita, fecha, hora, estado));

            return EnviarCorreoAsync(correoDestino, string.Format("Cita #{0} confirmada - MediCore", numeroCita), cuerpoHtml, cuerpoTexto, "CITA_PROGRAMADA");
        }

        public Task<bool> EnviarCancelacion(string correoDestino, string nombrePaciente, string nombreDoctor, string especialidad, int numeroCita, DateTime fecha, TimeSpan hora, string estado)
        {
            var cuerpoHtml = CargarPlantillaCorreo("CitaCancelacion.html", ArmarValoresCita(nombrePaciente, nombreDoctor, especialidad, numeroCita, fecha, hora, estado));
            var cuerpoTexto = CargarPlantillaCorreo("CitaCancelacion.txt", ArmarValoresCita(nombrePaciente, nombreDoctor, especialidad, numeroCita, fecha, hora, estado));

            return EnviarCorreoAsync(correoDestino, string.Format("Cita #{0} cancelada - MediCore", numeroCita), cuerpoHtml, cuerpoTexto, "CITA_CANCELADA");
        }

        public Task<bool> EnviarReprogramacion(string correoDestino, string nombrePaciente, string nombreDoctor, string especialidad, int numeroCita, DateTime fecha, TimeSpan hora, string estado)
        {
            var cuerpoHtml = CargarPlantillaCorreo("CitaReprogramacion.html", ArmarValoresCita(nombrePaciente, nombreDoctor, especialidad, numeroCita, fecha, hora, estado));
            var cuerpoTexto = CargarPlantillaCorreo("CitaReprogramacion.txt", ArmarValoresCita(nombrePaciente, nombreDoctor, especialidad, numeroCita, fecha, hora, estado));

            return EnviarCorreoAsync(correoDestino, string.Format("Cita #{0} reprogramada - MediCore", numeroCita), cuerpoHtml, cuerpoTexto, "CITA_REPROGRAMADA");
        }

        private static Dictionary<string, string> ArmarValoresCita(string nombrePaciente, string nombreDoctor, string especialidad, int numeroCita, DateTime fecha, TimeSpan hora, string estado)
        {
            return new Dictionary<string, string>
            {
                { "NombrePaciente", nombrePaciente },
                { "NombreDoctor", nombreDoctor },
                { "Especialidad", especialidad },
                { "NumeroCita", numeroCita.ToString() },
                { "Fecha", fecha.ToString("dd/MM/yyyy") },
                { "Hora", hora.ToString(@"hh\:mm") },
                { "Estado", estado }
            };
        }

        private static bool EsCorreoValido(string correo)
        {
            try
            {
                var direccion = new MailAddress(correo);
                return direccion.Address == correo.Trim();
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Carga el contenido de una plantilla de correo desde MediCore/EmailTemplates
        /// y reemplaza los tokens {{Clave}} por los valores provistos.
        /// </summary>
        private static string CargarPlantillaCorreo(string nombreArchivo, Dictionary<string, string> valores)
        {
            var ruta = HttpContext.Current.Server.MapPath("~/EmailTemplates/" + nombreArchivo);
            var contenido = System.IO.File.ReadAllText(ruta);

            foreach (var valor in valores)
            {
                contenido = contenido.Replace("{{" + valor.Key + "}}", valor.Value ?? string.Empty);
            }

            return contenido;
        }

        private void RegistrarNotificacion(string correoDestino, string tipo, string asunto, string cuerpoHtml, string estado, int? idUsuarioDestino)
        {
            try
            {
                using (var db = new MediCoreEntities())
                {
                    db.Notificaciones.Add(new Notificaciones
                    {
                        id_usuario_destino = idUsuarioDestino,
                        correo_destino = correoDestino,
                        tipo = tipo,
                        asunto = asunto,
                        cuerpo = cuerpoHtml,
                        estado = estado,
                        fecha_envio = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Si ni siquiera se puede dejar constancia en Notificaciones, se registra en Bitácora.
                _utilitarioService.RegistrarErrorBitacora(ex, "EmailService", "RegistrarNotificacion");
            }
        }
    }
}
