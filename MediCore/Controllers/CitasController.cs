using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    public class CitasController : Controller
    {
        private const string NombreControlador = "Citas";
        private const int TamanoPagina = 10;
        private readonly UtilitarioService _utilitarioService = new UtilitarioService();
        private readonly EmailService _emailService = new EmailService();

        public ActionResult Index(
            int? idPaciente,
            int? idDoctor,
            string estado,
            DateTime? fecha)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                var citas = db.Citas
                    .Include(c => c.Pacientes)
                    .Include(c => c.Doctores)
                    .AsQueryable();

                // Filtro por paciente
                if (idPaciente.HasValue)
                {
                    citas = citas.Where(c => c.id_paciente == idPaciente.Value);
                }

                // Filtro por doctor
                if (idDoctor.HasValue)
                {
                    citas = citas.Where(c => c.id_doctor == idDoctor.Value);
                }

                // Filtro por estado
                if (!string.IsNullOrWhiteSpace(estado))
                {
                    citas = citas.Where(c => c.estado == estado);
                }

                // Filtro por fecha
                if (fecha.HasValue)
                {
                    citas = citas.Where(c =>
                        DbFunctions.TruncateTime(c.fecha_cita) ==
                        DbFunctions.TruncateTime(fecha.Value));
                }

                // Si el usuario es doctor, ver solo sus propias citas
                var idDoctorSesion = Session["IdDoctor"] as int?;
                var esDoctorRol = (Session["NombreRol"] as string ?? "").ToUpper() == "DOCTOR";
                if (esDoctorRol && idDoctorSesion.HasValue)
                    citas = citas.Where(c => c.id_doctor == idDoctorSesion.Value);
                ViewBag.EsDoctor = esDoctorRol;

                // Combos para los filtros
                var pacientes = db.Pacientes
    .Where(p => p.estado == "ACTIVO")
    .OrderBy(p => p.nombre_completo)
    .Select(p => new SelectListItem
    {
        Value = p.id_paciente.ToString(),
        Text = p.nombre_completo
    })
    .ToList();

                var doctores = db.Doctores
                    .Where(d => d.estado == "ACTIVO")
                    .OrderBy(d => d.nombre_completo)
                    .Select(d => new SelectListItem
                    {
                        Value = d.id_doctor.ToString(),
                        Text = d.nombre_completo
                    })
                    .ToList();

                ViewBag.Pacientes = pacientes;
                ViewBag.Doctores = doctores;

                return View(
                    citas
                        .OrderByDescending(c => c.fecha_cita)
                        .Select(c => new CitaListaModel
                        {
                            IdCita         = c.id_cita,
                            NombrePaciente = c.Pacientes.nombre_completo,
                            NombreDoctor   = c.Doctores.nombre_completo,
                            FechaCita      = c.fecha_cita,
                            DuracionMin    = c.duracion_min,
                            Estado         = c.estado
                        })
                        .ToList());
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

                    _utilitarioService.RegistrarEvento(NombreControlador, "Edit GET", $"Edición de la cita #{id}.");

                    return View(model);
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "Edit GET");

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

                    _utilitarioService.RegistrarEvento(NombreControlador, "Details", $"Consulta de la cita #{id}.");

                    var model = new CitaDetalleModel
                    {
                        IdCita            = cita.id_cita,
                        NombrePaciente    = cita.Pacientes.nombre_completo,
                        NombreDoctor      = cita.Doctores.nombre_completo,
                        NombreEspecialidad = cita.Doctores.Especialidades.nombre,
                        FechaCita         = cita.fecha_cita,
                        DuracionMin       = cita.duracion_min,
                        Motivo            = cita.motivo,
                        MotivoCancelacion = cita.motivo_cancelacion,
                        Estado            = cita.estado,
                        FechaCreacion     = cita.fecha_creacion
                    };

                    return View(model);
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "Details");

                    TempData["Error"] = "Ocurrió un error al consultar la cita.";

                    return RedirectToAction("Index");
                }
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CitaFormModel model)
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

                    _utilitarioService.RegistrarEvento(NombreControlador, "Edit GET", $"Edición de la cita #{model.IdCita}.");

                    // El envío de la notificación nunca debe revertir la reprogramación (RF-15).
                    bool correoEnviado = await _emailService.EnviarReprogramacion(
                        cita.Pacientes.correo,
                        cita.Pacientes.nombre_completo,
                        cita.Doctores.nombre_completo,
                        cita.Doctores.Especialidades.nombre,
                        cita.id_cita,
                        cita.fecha_cita.Date,
                        cita.fecha_cita.TimeOfDay,
                        cita.estado);

                    TempData["Success"] = correoEnviado
                        ? "La cita fue actualizada correctamente. Se notificó al paciente de la reprogramación."
                        : "La cita fue actualizada correctamente, aunque no fue posible notificar al paciente.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "Edit");

                    ModelState.AddModelError("", "Ocurrió un error al actualizar la cita.");

                    return View(model);
                }
            }
        }



        //esto lo va a usar ajax
        // Devuelve los próximos 60 días en los que el doctor tiene horario activo
        [HttpGet]
        public JsonResult ObtenerFechasDisponibles(int idDoctor)
        {
            using (var db = new MediCoreEntities())
            {
                try
                {
                    // Días de la semana en que el doctor tiene horario
                    var diasConHorario = db.HorariosMedicos
                        .Where(h => h.id_doctor == idDoctor && h.estado == "ACTIVO")
                        .Select(h => h.dia_semana)
                        .ToList();

                    if (!diasConHorario.Any())
                        return Json(new List<object>(), JsonRequestBehavior.AllowGet);

                    var nombresdia = new string[] { "", "Lun", "Mar", "Mi\u00e9", "Jue", "Vie", "S\u00e1b", "Dom" };
                    var fechas = new List<object>();
                    DateTime hoy = DateTime.Today.AddDays(1); // desde mañana

                    for (int i = 0; i < 60; i++)
                    {
                        DateTime dia = hoy.AddDays(i);
                        int diaSemana = (int)dia.DayOfWeek == 0 ? 7 : (int)dia.DayOfWeek;

                        if (diasConHorario.Contains((byte)diaSemana))
                        {
                            fechas.Add(new
                            {
                                valor = dia.ToString("yyyy-MM-dd"),
                                texto = nombresdia[diaSemana] + " " + dia.ToString("dd/MM/yyyy")
                            });
                        }
                    }

                    return Json(fechas, JsonRequestBehavior.AllowGet);
                }
                catch
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> Create(CitaFormModel model)
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

                    db.Entry(cita).Reference(c => c.Pacientes).Load();
                    db.Entry(cita).Reference(c => c.Doctores).Load();
                    db.Entry(cita.Doctores).Reference(d => d.Especialidades).Load();

                    _utilitarioService.RegistrarEvento(NombreControlador, "Create", $"Cita #{cita.id_cita} registrada.");

                    // El envío de la confirmación nunca debe revertir el registro de la cita (RF-15).
                    bool correoEnviado = await _emailService.EnviarConfirmacionCita(
                        cita.Pacientes.correo,
                        cita.Pacientes.nombre_completo,
                        cita.Doctores.nombre_completo,
                        cita.Doctores.Especialidades.nombre,
                        cita.id_cita,
                        cita.fecha_cita.Date,
                        cita.fecha_cita.TimeOfDay,
                        cita.estado);

                    TempData["Success"] = correoEnviado
                        ? "La cita fue registrada correctamente. Se envió un correo de confirmación al paciente."
                        : "La cita fue registrada correctamente, aunque no fue posible enviar el correo de confirmación.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "Create");

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


        //abre la vista para atender la cita, donde se llenan los datos de la atención médica
        [DoctorOAdminActionFilter]
        [HttpGet]
        public ActionResult Atender(int id)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var cita = db.Citas
                        .Include(c => c.Pacientes)
                        .Include(c => c.Doctores)
                        .FirstOrDefault(c => c.id_cita == id);

                    if (cita == null)
                    {
                        TempData["Error"] = "La cita no existe.";
                        return RedirectToAction("Index");
                    }

                    if (cita.estado != "PENDIENTE")
                    {
                        TempData["Error"] =
                            "Solo se pueden atender citas pendientes.";

                        return RedirectToAction("Index");
                    }

                    // Buscar expediente del paciente
                    var expediente = db.Expedientes
                        .FirstOrDefault(e =>
                            e.id_paciente == cita.id_paciente);

                    if (expediente == null)
                    {
                        TempData["Error"] =
                            "El paciente no tiene un expediente clínico.";

                        return RedirectToAction("Index");
                    }

                    var model = new AtencionCitaModel
                    {
                        IdCita = cita.id_cita,
                        IdPaciente = cita.id_paciente,
                        IdDoctor = cita.id_doctor,
                        IdExpediente = expediente.id_expediente,

                        NombrePaciente = cita.Pacientes.nombre_completo,
                        NombreDoctor = cita.Doctores.nombre_completo,
                        FechaCita = cita.fecha_cita,
                        Motivo = cita.motivo
                    };

                    return View(model);
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "Atender GET");

                    TempData["Error"] =
                        "Ocurrió un error al cargar la atención de la cita.";

                    return RedirectToAction("Index");
                }
            }
        }

        //procesa la atención de la cita, registrando el historial médico y marcando la cita como atendida
        [DoctorOAdminActionFilter]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Atender(AtencionCitaModel model)
        {
            ViewBag.ActiveMenu = "Citas";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.AccionFinal != "ALTA" && model.AccionFinal != "NUEVA_CITA")
            {
                ModelState.AddModelError("AccionFinal", "Debe seleccionar Dar de Alta o Solicitar seguimiento.");
                return View(model);
            }

            using (var db = new MediCoreEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cita = db.Citas
                            .Include(c => c.Pacientes)
                            .Include(c => c.Doctores)
                            .FirstOrDefault(c => c.id_cita == model.IdCita);

                        if (cita == null)
                        {
                            TempData["Error"] = "La cita no existe.";
                            return RedirectToAction("Index");
                        }

                        if (cita.estado != "PENDIENTE")
                        {
                            TempData["Error"] = "La cita ya fue atendida o cancelada.";
                            return RedirectToAction("Index");
                        }

                        var expediente = db.Expedientes
                            .FirstOrDefault(e => e.id_paciente == cita.id_paciente);

                        if (expediente == null)
                        {
                            TempData["Error"] = "El paciente no tiene un expediente clínico.";
                            return RedirectToAction("Index");
                        }

                        bool historialExistente = db.HistorialMedico.Any(h => h.id_cita == cita.id_cita);
                        if (historialExistente)
                        {
                            TempData["Error"] = "Esta cita ya tiene un registro en el historial médico.";
                            return RedirectToAction("Index");
                        }

                        var historial = new HistorialMedico
                        {
                            id_expediente = expediente.id_expediente,
                            id_cita       = cita.id_cita,
                            id_doctor     = cita.id_doctor,
                            fecha_consulta = DateTime.Now,
                            sintomas      = string.IsNullOrWhiteSpace(model.Sintomas) ? null : model.Sintomas.Trim(),
                            diagnostico   = model.Diagnostico.Trim(),
                            tratamiento   = string.IsNullOrWhiteSpace(model.Tratamiento) ? null : model.Tratamiento.Trim(),
                            observaciones = string.IsNullOrWhiteSpace(model.Observaciones) ? null : model.Observaciones.Trim(),
                            medicamentos  = string.IsNullOrWhiteSpace(model.Medicamentos) ? null : model.Medicamentos.Trim(),
                        };

                        db.HistorialMedico.Add(historial);
                        cita.estado = "ATENDIDA";

                        // Si el doctor solicita seguimiento, crear cita pendiente de programación
                        if (model.AccionFinal == "NUEVA_CITA")
                        {
                            var citaSeguimiento = new Citas
                            {
                                id_paciente     = cita.id_paciente,
                                id_doctor       = cita.id_doctor,
                                fecha_cita      = DateTime.Now.AddDays(7), // fecha provisional; el recepcionista la asigna
                                duracion_min    = cita.duracion_min,
                                motivo          = "Seguimiento: " + model.Diagnostico.Trim(),
                                estado          = "SOLICITUD",
                                id_cita_anterior = cita.id_cita,
                                fecha_creacion  = DateTime.Now
                            };
                            db.Citas.Add(citaSeguimiento);
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        _utilitarioService.RegistrarEvento(NombreControlador, "Atender POST",
                            string.Format("Cita #{0} atendida. Acción: {1}.", model.IdCita, model.AccionFinal));

                        TempData["Success"] = model.AccionFinal == "ALTA"
                            ? "Paciente dado de alta correctamente."
                            : "Atención registrada. Se generó una solicitud de seguimiento para el recepcionista.";

                        return RedirectToAction("Details", "Expedientes", new { id = expediente.id_expediente });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "Atender POST");
                        ModelState.AddModelError("", "Ocurrió un error al registrar la atención médica.");

                        var cita = db.Citas
                            .Include(c => c.Pacientes)
                            .Include(c => c.Doctores)
                            .FirstOrDefault(c => c.id_cita == model.IdCita);

                        if (cita != null)
                        {
                            model.NombrePaciente = cita.Pacientes.nombre_completo;
                            model.NombreDoctor   = cita.Doctores.nombre_completo;
                            model.FechaCita      = cita.fecha_cita;
                            model.Motivo         = cita.motivo;
                        }

                        return View(model);
                    }
                }
            }
        }

        // GET: Citas/ProgramarSeguimiento/5 — solo recepcionista o admin
        [HttpGet]
        public ActionResult ProgramarSeguimiento(int id)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var cita = db.Citas
                        .Include(c => c.Pacientes)
                        .Include(c => c.Doctores)
                        .FirstOrDefault(c => c.id_cita == id);

                    if (cita == null || cita.estado != "SOLICITUD")
                    {
                        TempData["Error"] = "La cita de seguimiento no existe o ya fue programada.";
                        return RedirectToAction("Index");
                    }

                    // Obtener diagnóstico de la cita anterior para mostrar contexto
                    string diagnosticoAnterior = null;
                    if (cita.id_cita_anterior.HasValue)
                    {
                        var historial = db.HistorialMedico
                            .FirstOrDefault(h => h.id_cita == cita.id_cita_anterior.Value);
                        diagnosticoAnterior = historial?.diagnostico;
                    }

                    var model = new ProgramarSeguimientoModel
                    {
                        IdCita            = cita.id_cita,
                        IdCitaAnterior    = cita.id_cita_anterior ?? 0,
                        IdPaciente        = cita.id_paciente,
                        IdDoctor          = cita.id_doctor,
                        NombrePaciente    = cita.Pacientes.nombre_completo,
                        NombreDoctor      = cita.Doctores.nombre_completo,
                        DiagnosticoAnterior = diagnosticoAnterior,
                        FechaCitaAnterior = cita.fecha_cita,
                        Motivo            = cita.motivo
                    };

                    return View(model);
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "ProgramarSeguimiento GET");
                    TempData["Error"] = "Ocurrió un error al cargar la programación del seguimiento.";
                    return RedirectToAction("Index");
                }
            }
        }

        // POST: Citas/ProgramarSeguimiento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProgramarSeguimiento(ProgramarSeguimientoModel model)
        {
            ViewBag.ActiveMenu = "Citas";

            if (!ModelState.IsValid)
                return View(model);

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var cita = db.Citas
                        .Include(c => c.Pacientes)
                        .Include(c => c.Doctores)
                        .Include(c => c.Doctores.Especialidades)
                        .FirstOrDefault(c => c.id_cita == model.IdCita);

                    if (cita == null || cita.estado != "SOLICITUD")
                    {
                        TempData["Error"] = "La cita de seguimiento no existe o ya fue programada.";
                        return RedirectToAction("Index");
                    }

                    DateTime fechaHora = model.FechaCita.Value.Date.Add(model.HoraCita.Value);

                    cita.fecha_cita = fechaHora;
                    cita.estado     = "PENDIENTE";
                    cita.motivo     = string.IsNullOrWhiteSpace(model.Motivo) ? cita.motivo : model.Motivo.Trim();

                    db.Entry(cita).State = EntityState.Modified;
                    db.SaveChanges();

                    _utilitarioService.RegistrarEvento(NombreControlador, "ProgramarSeguimiento POST",
                        string.Format("Cita de seguimiento #{0} programada para {1}.", model.IdCita, fechaHora.ToString("dd/MM/yyyy HH:mm")));

                    bool correoEnviado = await _emailService.EnviarConfirmacionCita(
                        cita.Pacientes.correo,
                        cita.Pacientes.nombre_completo,
                        cita.Doctores.nombre_completo,
                        cita.Doctores.Especialidades.nombre,
                        cita.id_cita,
                        cita.fecha_cita.Date,
                        cita.fecha_cita.TimeOfDay,
                        cita.estado,
                        esSeguimiento: true);

                    TempData["Success"] = correoEnviado
                        ? string.Format("Cita de seguimiento programada para el {0} a las {1}. Se envió un correo de confirmación al paciente.", fechaHora.ToString("dd/MM/yyyy"), fechaHora.ToString("HH:mm"))
                        : string.Format("Cita de seguimiento programada para el {0} a las {1}, aunque no fue posible enviar el correo de confirmación.", fechaHora.ToString("dd/MM/yyyy"), fechaHora.ToString("HH:mm"));

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "ProgramarSeguimiento POST");
                    ModelState.AddModelError("", "Ocurrió un error al programar el seguimiento.");
                    return View(model);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancelar(int id, string motivoCancelacion)
        {
            ViewBag.ActiveMenu = "Citas";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var cita = db.Citas
                        .Include(c => c.Pacientes)
                        .Include(c => c.Doctores.Especialidades)
                        .FirstOrDefault(c => c.id_cita == id);

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
                    cita.motivo_cancelacion = string.IsNullOrWhiteSpace(motivoCancelacion) ? null : motivoCancelacion.Trim();

                    db.SaveChanges();

                    _utilitarioService.RegistrarEvento(NombreControlador, "Cancelar", $"La cita #{id} fue cancelada.");

                    // El envío de la notificación nunca debe revertir la cancelación (RF-15).
                    bool correoEnviado = await _emailService.EnviarCancelacion(
                        cita.Pacientes.correo,
                        cita.Pacientes.nombre_completo,
                        cita.Doctores.nombre_completo,
                        cita.Doctores.Especialidades.nombre,
                        cita.id_cita,
                        cita.fecha_cita.Date,
                        cita.fecha_cita.TimeOfDay,
                        cita.estado);

                    TempData["Success"] = correoEnviado
                        ? "La cita fue cancelada correctamente. Se notificó al paciente."
                        : "La cita fue cancelada correctamente, aunque no fue posible notificar al paciente.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _utilitarioService.RegistrarErrorBitacora(ex, NombreControlador, "Cancelar");

                    TempData["Error"] = "Ocurrió un error al cancelar la cita.";

                    return RedirectToAction("Index");
                }
            }
        }

    }
}