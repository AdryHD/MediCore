using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: Usuarios/Create?rol=DOCTOR|ADMINISTRADOR|RECEPCIONISTA
        public ActionResult Create(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                return RedirectToAction("Index");

            rol = rol.Trim().ToUpper();
            var rolesValidos = new[] { "ADMINISTRADOR", "DOCTOR", "RECEPCIONISTA" };
            if (!rolesValidos.Contains(rol))
                return RedirectToAction("Index");

            using (var db = new MediCoreEntities())
            {
                var roles = ObtenerRoles(db);
                var rolItem = roles.FirstOrDefault(r => r.NombreRol.ToUpper() == rol);
                if (rolItem == null)
                {
                    TempData["Error"] = "Rol no encontrado.";
                    return RedirectToAction("Index");
                }

                if (rol == "DOCTOR")
                {
                    ViewBag.Especialidades = new SelectList(
                        db.Especialidades.Where(e => e.estado == "ACTIVO").OrderBy(e => e.nombre).ToList(),
                        "id_especialidad", "nombre");
                }

                var model = new CrearUsuarioModel { RolNombre = rol, IdRol = rolItem.IdRol };
                return View(model);
            }
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CrearUsuarioModel model)
        {
            var rol = (model.RolNombre ?? "").Trim().ToUpper();

            if (rol == "DOCTOR")
            {
                if (string.IsNullOrWhiteSpace(model.CodigoColegiado))
                    ModelState.AddModelError("CodigoColegiado", "El código colegiado es obligatorio.");
                if (!model.IdEspecialidad.HasValue || model.IdEspecialidad.Value <= 0)
                    ModelState.AddModelError("IdEspecialidad", "Debe seleccionar una especialidad.");
            }

            using (var db = new MediCoreEntities())
            {
                if (!ModelState.IsValid)
                {
                    if (rol == "DOCTOR")
                        ViewBag.Especialidades = new SelectList(
                            db.Especialidades.Where(e => e.estado == "ACTIVO").OrderBy(e => e.nombre).ToList(),
                            "id_especialidad", "nombre", model.IdEspecialidad);
                    return View(model);
                }

                var correo = model.Correo.Trim();
                var cedula = model.Cedula.Trim();

                if (db.tbUsuario.Any(u => u.Correo == correo || u.Cedula == cedula))
                {
                    ModelState.AddModelError("", "Ya existe un usuario con ese correo o cédula.");
                    if (rol == "DOCTOR")
                        ViewBag.Especialidades = new SelectList(
                            db.Especialidades.Where(e => e.estado == "ACTIVO").OrderBy(e => e.nombre).ToList(),
                            "id_especialidad", "nombre", model.IdEspecialidad);
                    return View(model);
                }

                try
                {
                    string contrasenna = GenerarContrasennaTemporal();
                    DateTime expiracion = DateTime.Now.AddHours(24);

                    if (rol == "DOCTOR")
                    {
                        int resultado = db.spRegistrarDoctor(
                            model.Nombre.Trim(), cedula,
                            model.CodigoColegiado.Trim(), correo,
                            string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim(),
                            model.IdEspecialidad.Value, contrasenna).FirstOrDefault() ?? -1;

                        if (resultado != 0)
                        {
                            string[] errDr = { "", "Especialidad inválida.", "Cédula duplicada.", "Código colegiado duplicado.", "Correo duplicado." };
                            ModelState.AddModelError("", resultado < errDr.Length ? errDr[resultado] : "Error al registrar el doctor.");
                            ViewBag.Especialidades = new SelectList(
                                db.Especialidades.Where(e => e.estado == "ACTIVO").OrderBy(e => e.nombre).ToList(),
                                "id_especialidad", "nombre", model.IdEspecialidad);
                            return View(model);
                        }
                    }
                    else
                    {
                        db.sp_RegistrarUsuario(model.Nombre.Trim(), cedula, null,
                            string.IsNullOrWhiteSpace(model.Telefono) ? "" : model.Telefono.Trim(),
                            correo, contrasenna, model.IdRol);
                    }

                    // Establecer FechaExpiracionTemp (24h)
                    var nuevoUsuario = db.tbUsuario.FirstOrDefault(u => u.Correo == correo);
                    if (nuevoUsuario != null)
                    {
                        nuevoUsuario.FechaExpiracionTemp = expiracion;
                        db.Entry(nuevoUsuario).State = EntityState.Modified;
                        db.SaveChanges();

                        var emailService = new EmailService();
                        await emailService.EnviarCredencialesAdmin(correo, model.Nombre.Trim(), rol, contrasenna, expiracion, nuevoUsuario.Consecutivo);
                    }

                    _utilitarioService.RegistrarEvento(NombreControlador, "Create",
                        string.Format("Usuario '{0}' ({1}) creado con rol '{2}'.", model.Nombre, correo, rol));

                    TempData["Success"] = string.Format(
                        "Usuario '{0}' creado con rol {1}. Se envió el correo con credenciales (válidas 24 horas).",
                        model.Nombre.Trim(), rol);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "Create");
                    ModelState.AddModelError("", "Ocurrió un error al crear el usuario. Intente nuevamente.");
                    if (rol == "DOCTOR")
                        ViewBag.Especialidades = new SelectList(
                            db.Especialidades.Where(e => e.estado == "ACTIVO").OrderBy(e => e.nombre).ToList(),
                            "id_especialidad", "nombre", model.IdEspecialidad);
                    return View(model);
                }
            }
        }

        private static string GenerarContrasennaTemporal()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var rng = new Random();
            return new string(Enumerable.Range(0, 8).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
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

