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

                    var doctor = db.Doctores.FirstOrDefault(d => d.id_usuario == usuario.Consecutivo && d.estado == "ACTIVO");
                    if (doctor != null)
                        Session["IdDoctor"] = doctor.id_doctor;

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

                    var idRolRecepcionista = db.Database.SqlQuery<int>(
                        "SELECT id_rol FROM dbo.tbRol WHERE nombre_rol = 'RECEPCIONISTA'").FirstOrDefault();

                    db.sp_RegistrarUsuario(model.Nombre, cedulaLimpia, model.FechaNacimiento, model.Telefono, correoLimpio, model.Contrasenna, idRolRecepcionista);

                    RegistrarEvento(db, null, "Registro", string.Format("Usuario registrado con correo '{0}' (Cédula: {1}).", correoLimpio, cedulaLimpia));

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

        [AuthActionFilter]
        public ActionResult Principal()
        {
            return View();
        }

        [AuthActionFilter]
        public ActionResult GetIndicadores()
        {
            using (var db = new MediCoreEntities())
            {
                try
                {
                    var hoy = DateTime.Today;
                    var manana = hoy.AddDays(1);

                    var rol = (Session["NombreRol"] as string ?? "").ToUpper();
                    bool esDoctor = rol == "DOCTOR";
                    int? idDoctor = Session["IdDoctor"] as int?;

                    if (esDoctor)
                    {

                        if (!idDoctor.HasValue)
                        {
                            int? idUsuario = Session["Consecutivo"] as int?;
                            if (idUsuario.HasValue)
                            {
                                var doc = db.Doctores.FirstOrDefault(d => d.id_usuario == idUsuario.Value && d.estado == "ACTIVO");
                                if (doc != null)
                                {
                                    Session["IdDoctor"] = doc.id_doctor;
                                    idDoctor = doc.id_doctor;
                                }
                            }
                        }

                        if (!idDoctor.HasValue)
                            return Json(new { esDoctor = true, citasHoy = 0, citasPendientes = 0, proximasCitas = new List<object>() }, JsonRequestBehavior.AllowGet);

                        var citasHoyDoc = db.Citas.Count(c =>
                            c.id_doctor == idDoctor.Value &&
                            c.fecha_cita >= hoy && c.fecha_cita < manana);

                        var citasPendientesDoc = db.Citas.Count(c =>
                            c.id_doctor == idDoctor.Value &&
                            c.estado == "PENDIENTE");

                        var proximasCitasDoc = db.Citas
                            .Where(c => c.id_doctor == idDoctor.Value && c.estado == "PENDIENTE" && c.fecha_cita >= hoy)
                            .OrderBy(c => c.fecha_cita)
                            .Take(10)
                            .Select(c => new
                            {
                                id_cita    = c.id_cita,
                                paciente   = c.Pacientes.nombre_completo,
                                doctor     = c.Doctores.nombre_completo,
                                especialidad = c.Doctores.Especialidades.nombre,
                                fecha_cita = c.fecha_cita,
                                estado     = c.estado
                            })
                            .ToList()
                            .Select(c => new
                            {
                                c.id_cita, c.paciente, c.doctor, c.especialidad,
                                fecha_cita = c.fecha_cita.ToString("dd/MM/yyyy HH:mm"),
                                c.estado
                            });

                        return Json(new
                        {
                            esDoctor          = true,
                            citasHoy          = citasHoyDoc,
                            citasPendientes   = citasPendientesDoc,
                            proximasCitas     = proximasCitasDoc
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var totalPacientes = db.Pacientes.Count();
                    var totalDoctores = db.Doctores.Count();
                    var citasHoy = db.Citas.Count(c => c.fecha_cita >= hoy && c.fecha_cita < manana);
                    var citasPendientes = db.Citas.Count(c => c.estado == "PENDIENTE");

                    var proximasCitas = db.Citas
                        .Where(c => c.estado == "PENDIENTE" && c.fecha_cita >= hoy)
                        .OrderBy(c => c.fecha_cita)
                        .Select(c => new
                        {
                            id_cita = c.id_cita,
                            paciente = c.Pacientes.nombre_completo,
                            doctor = c.Doctores.nombre_completo,
                            especialidad = c.Doctores.Especialidades.nombre,
                            fecha_cita = c.fecha_cita,
                            estado = c.estado
                        })
                        .ToList()
                        .Select(c => new
                        {
                            c.id_cita,
                            c.paciente,
                            c.doctor,
                            c.especialidad,
                            fecha_cita = c.fecha_cita.ToString("dd/MM/yyyy HH:mm"),
                            c.estado
                        });

                    return Json(new
                    {
                        esDoctor        = false,
                        totalPacientes,
                        totalDoctores,
                        citasHoy,
                        citasPendientes,
                        proximasCitas
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, null, "GetIndicadores", ex);
                    return Json(new { error = "Error al obtener indicadores." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

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

            }
        }

        #endregion
    }
}