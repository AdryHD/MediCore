using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    public class CitasController : Controller
    {
        private const string NombreControlador = "Citas";
        private const int TamanoPagina = 10;

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var citas = db.Citas
                        .Include(c => c.Pacientes)
                        .Include(c => c.Doctores)
                        .OrderByDescending(c => c.fecha_cita)
                        .ToList();

                    return View(citas);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Index", ex);

                    TempData["Error"] = "Ocurrió un error al cargar las citas.";

                    return View(new List<Citas>());
                }
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                var model = new CitaFormModel();

                CargarCombos(db, model);

                model.Fecha = DateTime.Today.AddDays(1);

                return View(model);
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var cita = db.Citas
                        .Include(c => c.Doctores)
                        .FirstOrDefault(c => c.id_cita == id);

                    if (cita == null)
                    {
                        TempData["Error"] = "La cita no existe.";
                        return RedirectToAction("Index");
                    }

                    if (cita.estado != "PENDIENTE")
                    {
                        TempData["Error"] = "Solo se pueden editar las citas pendientes.";
                        return RedirectToAction("Index");
                    }

                    var model = new CitaFormModel
                    {
                        IdCita = cita.id_cita,
                        IdPaciente = cita.id_paciente,
                        IdEspecialidad = cita.Doctores.id_especialidad,
                        IdDoctor = cita.id_doctor,
                        Fecha = cita.fecha_cita.Date,
                        Hora = cita.fecha_cita.TimeOfDay,
                        DuracionMinutos = cita.duracion_min,
                        Motivo = cita.motivo,
                        Estado = cita.estado
                    };

                    CargarCombos(db, model);

                    RegistrarEvento(db, "Edit GET", $"Edición de la cita #{id}.");

                    return View(model);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Edit GET", ex);

                    TempData["Error"] = "Ocurrió un error al cargar la cita.";

                    return RedirectToAction("Index");
                }
            }
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var cita = db.Citas
                        .Include(c => c.Pacientes)
                        .Include(c => c.Doctores)
                        .Include(c => c.Doctores.Especialidades)
                        .FirstOrDefault(c => c.id_cita == id);

                    if (cita == null)
                    {
                        TempData["Error"] = "La cita no existe.";
                        return RedirectToAction("Index");
                    }

                    RegistrarEvento(
                        db,
                        "Details",
                        $"Consulta de la cita #{id}."
                    );

                    return View(cita);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Details", ex);

                    TempData["Error"] = "Ocurrió un error al consultar la cita.";

                    return RedirectToAction("Index");
                }
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CitaFormModel model)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                CargarCombos(db, model);

                if (!ModelState.IsValid)
                    return View(model);

                try
                {
                    var cita = db.Citas.FirstOrDefault(c => c.id_cita == model.IdCita);

                    if (cita == null)
                    {
                        TempData["Error"] = "La cita no existe.";
                        return RedirectToAction("Index");
                    }

                    if (cita.estado != "PENDIENTE")
                    {
                        TempData["Error"] = "Solo se pueden editar las citas pendientes.";
                        return RedirectToAction("Index");
                    }

                    int diaSemana = (int)model.Fecha.DayOfWeek;
                    diaSemana = diaSemana == 0 ? 7 : diaSemana;

                    var horario = db.HorariosMedicos.FirstOrDefault(h =>
                        h.id_doctor == model.IdDoctor &&
                        h.dia_semana == diaSemana &&
                        h.estado == "ACTIVO");

                    if (horario == null)
                    {
                        ModelState.AddModelError("", "El médico no tiene un horario activo para ese día.");
                        return View(model);
                    }

                    DateTime fechaHora = model.Fecha.Date + model.Hora;

                    bool ocupado = db.Citas.Any(c =>
                        c.id_doctor == model.IdDoctor &&
                        c.id_cita != model.IdCita &&
                        c.estado != "CANCELADA" &&
                        c.fecha_cita == fechaHora);

                    if (ocupado)
                    {
                        ModelState.AddModelError("", "El horario seleccionado ya se encuentra ocupado.");
                        return View(model);
                    }

                    cita.id_paciente = model.IdPaciente;
                    cita.id_doctor = model.IdDoctor;
                    cita.fecha_cita = fechaHora;
                    cita.duracion_min = horario.duracion_cita_min;
                    cita.motivo = model.Motivo;

                    db.SaveChanges();

                    RegistrarEvento(
                        db,
                        "Edit",
                        $"Se actualizó la cita #{cita.id_cita}."
                    );

                    TempData["Success"] = "La cita fue actualizada correctamente.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Edit", ex);

                    ModelState.AddModelError("", "Ocurrió un error al actualizar la cita.");

                    return View(model);
                }
            }
        }



        //esto lo va a usar ajax
        [HttpGet]
        public JsonResult ObtenerDoctores(int idEspecialidad)
        {
            using (var db = new MediCoreEntities())
            {
                try
                {
                    var doctores = db.Doctores
                        .Where(d => d.id_especialidad == idEspecialidad &&
                                    d.estado == "ACTIVO")
                        .OrderBy(d => d.nombre_completo)
                        .Select(d => new
                        {
                            id = d.id_doctor,
                            nombre = d.nombre_completo
                        })
                        .ToList();

                    return Json(doctores, JsonRequestBehavior.AllowGet);
                }
                catch
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public JsonResult ObtenerHorariosDisponibles(int idDoctor, DateTime fecha)
        {
            using (var db = new MediCoreEntities())
            {
                try
                {
                    int diaSemana = (int)fecha.DayOfWeek;
                    diaSemana = (diaSemana == 0) ? 7 : diaSemana;

                    //System.Diagnostics.Debug.WriteLine("===== HORARIOS =====");------------------------- estos eran pruebas
                    //System.Diagnostics.Debug.WriteLine("Doctor recibido: " + idDoctor);
                    //System.Diagnostics.Debug.WriteLine("Fecha recibida: " + fecha.ToString("yyyy-MM-dd"));
                    //System.Diagnostics.Debug.WriteLine("DayOfWeek: " + fecha.DayOfWeek);
                    //System.Diagnostics.Debug.WriteLine("DiaSemana calculado: " + diaSemana);

                    var horario = db.HorariosMedicos.FirstOrDefault(h =>
                        h.id_doctor == idDoctor &&
                        h.dia_semana == diaSemana &&
                        h.estado == "ACTIVO");

                    if (horario == null)
                    {
                        //System.Diagnostics.Debug.WriteLine("NO SE ENCONTRÓ HORARIO");

                        return Json(new List<object>(),
                            JsonRequestBehavior.AllowGet);
                    }

                    System.Diagnostics.Debug.WriteLine("HORARIO ENCONTRADO");
                    System.Diagnostics.Debug.WriteLine("Inicio: " + horario.hora_inicio);
                    System.Diagnostics.Debug.WriteLine("Fin: " + horario.hora_fin);

                    var citasOcupadas = db.Citas
                        .Where(c =>
                            c.id_doctor == idDoctor &&
                            c.estado != "CANCELADA" &&
                            DbFunctions.TruncateTime(c.fecha_cita) == fecha.Date)
                        .ToList()
                        .Select(c => c.fecha_cita.TimeOfDay)
                        .ToList();

                    var horasDisponibles = new List<object>();

                    TimeSpan hora = horario.hora_inicio;

                    while (hora < horario.hora_fin)
                    {
                        if (!citasOcupadas.Contains(hora))
                        {
                            horasDisponibles.Add(new
                            {
                                valor = hora.ToString(@"hh\:mm"),
                                texto = hora.ToString(@"hh\:mm")
                            });
                        }

                        hora = hora.Add(TimeSpan.FromMinutes(horario.duracion_cita_min));
                    }

                    System.Diagnostics.Debug.WriteLine("Cantidad de horas: " + horasDisponibles.Count);

                    return Json(horasDisponibles, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());

                    return Json(new List<object>(),
                        JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CitaFormModel model)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                CargarCombos(db, model);

                if (!ModelState.IsValid)
                    return View(model);

                try
                {
                    var cita = new Citas
                    {
                        id_paciente = model.IdPaciente,
                        id_doctor = model.IdDoctor,
                        fecha_cita = model.Fecha.Date + model.Hora,
                        duracion_min = model.DuracionMinutos,
                        motivo = model.Motivo,
                        estado = "PENDIENTE",
                        fecha_creacion = DateTime.Now
                    };

                    db.Citas.Add(cita);

                    db.SaveChanges();

                    RegistrarEvento(
                        db,
                        "Create",
                        $"Cita #{cita.id_cita} registrada."
                    );

                    TempData["Success"] =
                        "La cita fue registrada correctamente.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Create", ex);

                    ModelState.AddModelError(
                        "",
                        "Ocurrió un error al registrar la cita."
                    );

                    return View(model);
                }
            }
        }

        private void CargarCombos(MediCoreEntities db, CitaFormModel model)
        {
            // Pacientes
            model.Pacientes = db.Pacientes
                .Where(x => x.estado == "ACTIVO")
                .OrderBy(x => x.nombre_completo)
                .Select(x => new SelectListItem
                {
                    Value = x.id_paciente.ToString(),
                    Text = x.nombre_completo
                })
                .ToList();

            // Especialidades
            model.Especialidades = db.Especialidades
                .Where(x => x.estado == "ACTIVO")
                .OrderBy(x => x.nombre)
                .Select(x => new SelectListItem
                {
                    Value = x.id_especialidad.ToString(),
                    Text = x.nombre
                })
                .ToList();

            // Doctores
            if (model.IdEspecialidad > 0)
            {
                model.Doctores = db.Doctores
                    .Where(d => d.id_especialidad == model.IdEspecialidad &&
                                d.estado == "ACTIVO")
                    .OrderBy(d => d.nombre_completo)
                    .Select(d => new SelectListItem
                    {
                        Value = d.id_doctor.ToString(),
                        Text = d.nombre_completo
                    })
                    .ToList();
            }
            else
            {
                model.Doctores = new List<SelectListItem>();
            }

            // Horarios disponibles
            if (model.IdDoctor > 0 && model.Fecha != DateTime.MinValue)
            {
                int diaSemana = (int)model.Fecha.DayOfWeek;
                diaSemana = diaSemana == 0 ? 7 : diaSemana;

                var horario = db.HorariosMedicos.FirstOrDefault(h =>
                    h.id_doctor == model.IdDoctor &&
                    h.dia_semana == diaSemana &&
                    h.estado == "ACTIVO");

                if (horario != null)
                {
                    var citasOcupadas = db.Citas
                        .Where(c =>
                            c.id_doctor == model.IdDoctor &&
                            c.estado != "CANCELADA" &&
                            DbFunctions.TruncateTime(c.fecha_cita) == model.Fecha.Date)
                        .ToList()
                        .Select(c => c.fecha_cita.TimeOfDay)
                        .ToList();

                    var horarios = new List<SelectListItem>();

                    TimeSpan hora = horario.hora_inicio;

                    while (hora < horario.hora_fin)
                    {
                        // En Edit se debe mostrar la hora actual de la cita,
                        // aunque ya esté ocupada por esa misma cita.
                        if (!citasOcupadas.Contains(hora) || hora == model.Hora)
                        {
                            horarios.Add(new SelectListItem
                            {
                                Value = hora.ToString(@"hh\:mm"),
                                Text = hora.ToString(@"hh\:mm"),
                                Selected = (hora == model.Hora)
                            });
                        }

                        hora = hora.Add(TimeSpan.FromMinutes(horario.duracion_cita_min));
                    }

                    model.HorariosDisponibles = horarios;
                }
                else
                {
                    model.HorariosDisponibles = new List<SelectListItem>();
                }
            }
            else
            {
                model.HorariosDisponibles = new List<SelectListItem>();
            }
        }

        [HttpGet]
        public ActionResult Atender(int id)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var cita = db.Citas.FirstOrDefault(c => c.id_cita == id);

                    if (cita == null)
                    {
                        TempData["Error"] = "La cita no existe.";
                        return RedirectToAction("Index");
                    }

                    if (cita.estado != "PENDIENTE")
                    {
                        TempData["Error"] = "Solo se pueden atender citas pendientes.";
                        return RedirectToAction("Index");
                    }

                    cita.estado = "ATENDIDA";

                    db.SaveChanges();

                    RegistrarEvento(
                        db,
                        "Atender",
                        $"La cita #{id} fue marcada como ATENDIDA."
                    );

                    TempData["Success"] = "La cita fue marcada como atendida.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Atender", ex);

                    TempData["Error"] = "Ocurrió un error al atender la cita.";

                    return RedirectToAction("Index");
                }
            }
        }

        [HttpGet]
        public ActionResult Cancelar(int id)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var cita = db.Citas.FirstOrDefault(c => c.id_cita == id);

                    if (cita == null)
                    {
                        TempData["Error"] = "La cita no existe.";
                        return RedirectToAction("Index");
                    }

                    if (cita.estado != "PENDIENTE")
                    {
                        TempData["Error"] = "Solo se pueden cancelar citas pendientes.";
                        return RedirectToAction("Index");
                    }

                    cita.estado = "CANCELADA";

                    db.SaveChanges();

                    RegistrarEvento(
                        db,
                        "Cancelar",
                        $"La cita #{id} fue cancelada."
                    );

                    TempData["Success"] = "La cita fue cancelada correctamente.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Cancelar", ex);

                    TempData["Error"] = "Ocurrió un error al cancelar la cita.";

                    return RedirectToAction("Index");
                }
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
