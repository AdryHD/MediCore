using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    public class DoctoresController : Controller
    {
        private const string NombreControlador = "Doctores";
        private const int TamanoPagina = 10;

        // GET: Doctores
        public ActionResult Index(string q, int? idEspecialidad, string estado, int page = 1)
        {
            ViewBag.ActiveMenu = "Doctores";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var query = db.Doctores.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        var texto = q.Trim().ToUpper();
                        query = query.Where(d => d.nombre_completo.ToUpper().Contains(texto)
                                                  || d.cedula.ToUpper().Contains(texto)
                                                  || d.codigo_colegiado.ToUpper().Contains(texto));
                    }

                    if (idEspecialidad.HasValue && idEspecialidad.Value > 0)
                    {
                        query = query.Where(d => d.id_especialidad == idEspecialidad.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(estado))
                    {
                        query = query.Where(d => d.estado == estado);
                    }

                    int totalRegistros = query.Count();
                    if (page < 1) page = 1;
                    int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanoPagina);

                    var doctores = query
                        .OrderBy(d => d.nombre_completo)
                        .Skip((page - 1) * TamanoPagina)
                        .Take(TamanoPagina)
                        .ToList();

                    var especialidades = db.Especialidades.OrderBy(e => e.nombre).ToList();

                    ViewBag.NombreEspecialidad = especialidades.ToDictionary(e => e.id_especialidad, e => e.nombre);
                    ViewBag.Especialidades = new SelectList(especialidades, "id_especialidad", "nombre", idEspecialidad);
                    ViewBag.FiltroTexto = q;
                    ViewBag.FiltroEspecialidad = idEspecialidad;
                    ViewBag.FiltroEstado = estado;
                    ViewBag.PaginaActual = page;
                    ViewBag.TotalPaginas = totalPaginas;
                    ViewBag.TotalRegistros = totalRegistros;

                    return View(doctores);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Index", ex);
                    TempData["Error"] = "Ocurrió un error al cargar los doctores.";
                    return View(new List<Doctores>());
                }
            }
        }

        // GET: Doctores/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Doctores";

            using (var db = new MediCoreEntities())
            {
                CargarEspecialidades(db, null);
            }

            return View(new DoctorFormModel());
        }

        // POST: Doctores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DoctorFormModel model)
        {
            ViewBag.ActiveMenu = "Doctores";

            using (var db = new MediCoreEntities())
            {
                if (!ModelState.IsValid)
                {
                    CargarEspecialidades(db, model.IdEspecialidad);
                    return View(model);
                }

                try
                {
                    string contrasenna = GenerarContrasennaTemporal();

                    int resultado = db.spRegistrarDoctor(
                        model.NombreCompleto.Trim(),
                        model.Cedula.Trim(),
                        model.CodigoColegiado.Trim(),
                        model.Correo.Trim(),
                        string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim(),
                        model.IdEspecialidad,
                        contrasenna);

                    switch (resultado)
                    {
                        case 0:
                            RegistrarEvento(db, "Create", string.Format("Doctor '{0}' (Cédula: {1}) creado con cuenta de usuario asociada.", model.NombreCompleto, model.Cedula));
                            TempData["Success"] = string.Format(
                                "Doctor registrado correctamente. Cuenta de usuario creada — Correo: {0} / Contraseña temporal: {1}. Compártala de forma segura, no volverá a mostrarse.",
                                model.Correo.Trim(), contrasenna);
                            return RedirectToAction("Index");

                        case 1:
                            ModelState.AddModelError("IdEspecialidad", "La especialidad seleccionada no existe o está inactiva.");
                            break;

                        case 2:
                            ModelState.AddModelError("Cedula", "Ya existe un doctor registrado con esa cédula profesional.");
                            break;

                        case 3:
                            ModelState.AddModelError("CodigoColegiado", "Ya existe un doctor registrado con ese código colegiado.");
                            break;

                        case 4:
                            ModelState.AddModelError("Correo", "Ya existe un usuario registrado con ese correo electrónico.");
                            break;

                        default:
                            ModelState.AddModelError("", "Ocurrió un error al registrar el doctor. Intente nuevamente.");
                            break;
                    }

                    CargarEspecialidades(db, model.IdEspecialidad);
                    return View(model);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Create", ex);
                    ModelState.AddModelError("", "Ocurrió un error al registrar el doctor. Intente nuevamente.");
                    CargarEspecialidades(db, model.IdEspecialidad);
                    return View(model);
                }
            }
        }

        // GET: Doctores/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Doctores";

            using (var db = new MediCoreEntities())
            {
                var doctor = db.Doctores.FirstOrDefault(d => d.id_doctor == id);

                if (doctor == null)
                {
                    TempData["Error"] = "El doctor solicitado no existe.";
                    return RedirectToAction("Index");
                }

                var model = new DoctorFormModel
                {
                    IdDoctor = doctor.id_doctor,
                    NombreCompleto = doctor.nombre_completo,
                    Cedula = doctor.cedula,
                    CodigoColegiado = doctor.codigo_colegiado,
                    Correo = doctor.correo,
                    Telefono = doctor.telefono,
                    IdEspecialidad = doctor.id_especialidad
                };

                CargarEspecialidades(db, model.IdEspecialidad);

                return View(model);
            }
        }

        // POST: Doctores/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DoctorFormModel model)
        {
            ViewBag.ActiveMenu = "Doctores";

            using (var db = new MediCoreEntities())
            {
                if (!ModelState.IsValid)
                {
                    CargarEspecialidades(db, model.IdEspecialidad);
                    return View(model);
                }

                try
                {
                    var doctor = db.Doctores.FirstOrDefault(d => d.id_doctor == model.IdDoctor);

                    if (doctor == null)
                    {
                        TempData["Error"] = "El doctor solicitado no existe.";
                        return RedirectToAction("Index");
                    }

                    bool especialidadValida = db.Especialidades.Any(e => e.id_especialidad == model.IdEspecialidad && e.estado == "ACTIVO");
                    if (!especialidadValida)
                    {
                        ModelState.AddModelError("IdEspecialidad", "La especialidad seleccionada no existe o está inactiva.");
                        CargarEspecialidades(db, model.IdEspecialidad);
                        return View(model);
                    }

                    bool existeCedula = db.Doctores.Any(d => d.id_doctor != model.IdDoctor
                                              && d.cedula.Trim().ToUpper() == model.Cedula.Trim().ToUpper());
                    if (existeCedula)
                    {
                        ModelState.AddModelError("Cedula", "Ya existe un doctor registrado con esa cédula profesional.");
                        CargarEspecialidades(db, model.IdEspecialidad);
                        return View(model);
                    }

                    bool existeCodigo = db.Doctores.Any(d => d.id_doctor != model.IdDoctor
                                              && d.codigo_colegiado.Trim().ToUpper() == model.CodigoColegiado.Trim().ToUpper());
                    if (existeCodigo)
                    {
                        ModelState.AddModelError("CodigoColegiado", "Ya existe un doctor registrado con ese código colegiado.");
                        CargarEspecialidades(db, model.IdEspecialidad);
                        return View(model);
                    }

                    bool existeCorreo = db.Doctores.Any(d => d.id_doctor != model.IdDoctor
                                              && d.correo.Trim().ToUpper() == model.Correo.Trim().ToUpper());
                    if (existeCorreo)
                    {
                        ModelState.AddModelError("Correo", "Ya existe un doctor registrado con ese correo electrónico.");
                        CargarEspecialidades(db, model.IdEspecialidad);
                        return View(model);
                    }

                    doctor.nombre_completo = model.NombreCompleto.Trim();
                    doctor.cedula = model.Cedula.Trim();
                    doctor.codigo_colegiado = model.CodigoColegiado.Trim();
                    doctor.correo = model.Correo.Trim();
                    doctor.telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();
                    doctor.id_especialidad = model.IdEspecialidad;

                    if (doctor.id_usuario.HasValue)
                    {
                        var usuario = db.tbUsuario.FirstOrDefault(u => u.Consecutivo == doctor.id_usuario.Value);
                        if (usuario != null)
                        {
                            usuario.Nombre = doctor.nombre_completo;
                            usuario.Correo = doctor.correo;
                            usuario.Telefono = string.IsNullOrWhiteSpace(doctor.telefono) ? "" : doctor.telefono;
                        }
                    }

                    db.SaveChanges();

                    RegistrarEvento(db, "Edit", string.Format("Doctor '{0}' (Id: {1}) actualizado.", doctor.nombre_completo, doctor.id_doctor));

                    TempData["Success"] = "Doctor actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Edit", ex);
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el doctor. Intente nuevamente.");
                    CargarEspecialidades(db, model.IdEspecialidad);
                    return View(model);
                }
            }
        }

        // POST: Doctores/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, string nuevoEstado)
        {
            using (var db = new MediCoreEntities())
            {
                try
                {
                    int resultado = db.spCambiarEstadoDoctor(id, nuevoEstado, ObtenerIdUsuarioActual());

                    switch (resultado)
                    {
                        case 0:
                            RegistrarEvento(db, "CambiarEstado", string.Format("Doctor Id: {0} cambiado a estado '{1}'.", id, nuevoEstado));
                            TempData["Success"] = nuevoEstado == "ACTIVO"
                                ? "El doctor fue activado correctamente."
                                : "El doctor fue desactivado correctamente y ya no aparecerá disponible para nuevas citas.";
                            break;

                        default:
                            TempData["Error"] = "El doctor solicitado no existe.";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "CambiarEstado", ex);
                    TempData["Error"] = "Ocurrió un error al cambiar el estado del doctor.";
                }
            }

            return RedirectToAction("Index");
        }

        private void CargarEspecialidades(MediCoreEntities db, int? seleccionado)
        {
            var especialidades = db.Especialidades
                .Where(e => e.estado == "ACTIVO")
                .OrderBy(e => e.nombre)
                .ToList();

            ViewBag.Especialidades = new SelectList(especialidades, "id_especialidad", "nombre", seleccionado);
        }

        private static readonly Random RandomGenerator = new Random();

        private string GenerarContrasennaTemporal()
        {
            const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var chars = new char[8];

            lock (RandomGenerator)
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    chars[i] = caracteres[RandomGenerator.Next(caracteres.Length)];
                }
            }

            return new string(chars);
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

        private void RegistrarEvento(MediCoreEntities db, string accion, string mensaje)
        {
            try
            {
                db.spRegistrarBitacora("INFO", ObtenerIdUsuarioActual(), NombreControlador, accion, mensaje, null, ObtenerIp());
            }
            catch
            {
                // La bitácora nunca debe interrumpir el flujo principal de la aplicación.
            }
        }

        private void RegistrarError(MediCoreEntities db, string accion, Exception ex)
        {
            try
            {
                db.spRegistrarBitacora("ERROR", ObtenerIdUsuarioActual(), NombreControlador, accion, ex.Message, ex.StackTrace, ObtenerIp());
            }
            catch
            {
                // La bitácora nunca debe interrumpir el flujo principal de la aplicación.
            }
        }

        #endregion
    }
}
