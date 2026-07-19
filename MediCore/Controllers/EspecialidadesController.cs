using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    public class EspecialidadesController : Controller
    {
        private const string NombreControlador = "Especialidades";

        // GET: Especialidades
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Especialidades";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var especialidades = db.Especialidades
                        .OrderBy(e => e.nombre)
                        .ToList();

                    return View(especialidades);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Index", ex);
                    TempData["Error"] = "Ocurrió un error al cargar las especialidades.";
                    return View(new List<Especialidades>());
                }
            }
        }

        // GET: Especialidades/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Especialidades";
            return View(new EspecialidadFormModel());
        }

        // POST: Especialidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EspecialidadFormModel model)
        {
            ViewBag.ActiveMenu = "Especialidades";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new MediCoreEntities())
            {
                try
                {
                    bool existeNombre = db.Especialidades
                        .Any(e => e.nombre.Trim().ToUpper() == model.Nombre.Trim().ToUpper());

                    if (existeNombre)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe una especialidad registrada con ese nombre.");
                        return View(model);
                    }

                    var especialidad = new Especialidades
                    {
                        nombre = model.Nombre.Trim(),
                        descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim(),
                        estado = "ACTIVO",
                        fecha_creacion = DateTime.Now
                    };

                    db.Especialidades.Add(especialidad);
                    db.SaveChanges();

                    RegistrarEvento(db, "Create", string.Format("Especialidad '{0}' creada (Id: {1}).", especialidad.nombre, especialidad.id_especialidad));

                    TempData["Success"] = "Especialidad registrada correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Create", ex);
                    ModelState.AddModelError("", "Ocurrió un error al registrar la especialidad. Intente nuevamente.");
                    return View(model);
                }
            }
        }

        // GET: Especialidades/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Especialidades";

            using (var db = new MediCoreEntities())
            {
                var especialidad = db.Especialidades.FirstOrDefault(e => e.id_especialidad == id);

                if (especialidad == null)
                {
                    TempData["Error"] = "La especialidad solicitada no existe.";
                    return RedirectToAction("Index");
                }

                var model = new EspecialidadFormModel
                {
                    IdEspecialidad = especialidad.id_especialidad,
                    Nombre = especialidad.nombre,
                    Descripcion = especialidad.descripcion
                };

                return View(model);
            }
        }

        // POST: Especialidades/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EspecialidadFormModel model)
        {
            ViewBag.ActiveMenu = "Especialidades";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var especialidad = db.Especialidades.FirstOrDefault(e => e.id_especialidad == model.IdEspecialidad);

                    if (especialidad == null)
                    {
                        TempData["Error"] = "La especialidad solicitada no existe.";
                        return RedirectToAction("Index");
                    }

                    bool existeNombre = db.Especialidades
                        .Any(e => e.id_especialidad != model.IdEspecialidad
                                  && e.nombre.Trim().ToUpper() == model.Nombre.Trim().ToUpper());

                    if (existeNombre)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe una especialidad registrada con ese nombre.");
                        return View(model);
                    }

                    especialidad.nombre = model.Nombre.Trim();
                    especialidad.descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim();

                    db.SaveChanges();

                    RegistrarEvento(db, "Edit", string.Format("Especialidad '{0}' (Id: {1}) actualizada.", especialidad.nombre, especialidad.id_especialidad));

                    TempData["Success"] = "Especialidad actualizada correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Edit", ex);
                    ModelState.AddModelError("", "Ocurrió un error al actualizar la especialidad. Intente nuevamente.");
                    return View(model);
                }
            }
        }

        // POST: Especialidades/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, string nuevoEstado)
        {
            using (var db = new MediCoreEntities())
            {
                try
                {
                    int resultado = db.spCambiarEstadoEspecialidad(id, nuevoEstado, ObtenerIdUsuarioActual());

                    switch (resultado)
                    {
                        case 0:
                            RegistrarEvento(db, "CambiarEstado", string.Format("Especialidad Id: {0} cambiada a estado '{1}'.", id, nuevoEstado));
                            TempData["Success"] = nuevoEstado == "ACTIVO"
                                ? "La especialidad fue activada correctamente."
                                : "La especialidad fue desactivada correctamente.";
                            break;

                        case 1:
                            TempData["Error"] = "No se puede desactivar la especialidad porque tiene doctores activos asignados.";
                            break;

                        default:
                            TempData["Error"] = "La especialidad solicitada no existe.";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "CambiarEstado", ex);
                    TempData["Error"] = "Ocurrió un error al cambiar el estado de la especialidad.";
                }
            }

            return RedirectToAction("Index");
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
