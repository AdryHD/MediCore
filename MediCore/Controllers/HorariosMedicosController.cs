using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    public class HorariosMedicosController : Controller
    {
        private const string NombreControlador = "HorariosMedicos";
        private const int TamanoPagina = 10;

        private static readonly Dictionary<byte, string> DiasSemana = new Dictionary<byte, string>
        {
            { 1, "Lunes" },
            { 2, "Martes" },
            { 3, "Miércoles" },
            { 4, "Jueves" },
            { 5, "Viernes" },
            { 6, "Sábado" },
            { 7, "Domingo" }
        };

        // GET: HorariosMedicos
        public ActionResult Index(int? idDoctor, byte? diaSemana, string estado, int page = 1)
        {
            ViewBag.ActiveMenu = "Horarios";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var query = db.HorariosMedicos.AsQueryable();

                    if (idDoctor.HasValue && idDoctor.Value > 0)
                    {
                        query = query.Where(h => h.id_doctor == idDoctor.Value);
                    }

                    if (diaSemana.HasValue && diaSemana.Value > 0)
                    {
                        query = query.Where(h => h.dia_semana == diaSemana.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(estado))
                    {
                        query = query.Where(h => h.estado == estado);
                    }

                    int totalRegistros = query.Count();
                    if (page < 1) page = 1;
                    int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanoPagina);

                    var horarios = query
                        .OrderBy(h => h.id_doctor)
                        .ThenBy(h => h.dia_semana)
                        .ThenBy(h => h.hora_inicio)
                        .Skip((page - 1) * TamanoPagina)
                        .Take(TamanoPagina)
                        .ToList();

                    var doctores = db.Doctores.OrderBy(d => d.nombre_completo).ToList();

                    ViewBag.NombreDoctor = doctores.ToDictionary(d => d.id_doctor, d => d.nombre_completo);
                    ViewBag.DiasSemana = DiasSemana;
                    ViewBag.Doctores = new SelectList(doctores, "id_doctor", "nombre_completo", idDoctor);
                    ViewBag.FiltroDoctor = idDoctor;
                    ViewBag.FiltroDiaSemana = diaSemana;
                    ViewBag.FiltroEstado = estado;
                    ViewBag.PaginaActual = page;
                    ViewBag.TotalPaginas = totalPaginas;
                    ViewBag.TotalRegistros = totalRegistros;

                    return View(horarios);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Index", ex);
                    TempData["Error"] = "Ocurrió un error al cargar los horarios médicos.";
                    return View(new List<HorariosMedicos>());
                }
            }
        }

        // GET: HorariosMedicos/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Horarios";

            using (var db = new MediCoreEntities())
            {
                CargarDoctoresActivos(db, null);
            }

            return View(new HorarioMedicoFormModel());
        }

        // POST: HorariosMedicos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HorarioMedicoFormModel model)
        {
            ViewBag.ActiveMenu = "Horarios";

            using (var db = new MediCoreEntities())
            {
                ValidarHorario(db, model);

                if (!ModelState.IsValid)
                {
                    CargarDoctoresActivos(db, model.IdDoctor);
                    return View(model);
                }

                try
                {
                    var horario = new HorariosMedicos
                    {
                        id_doctor = model.IdDoctor,
                        dia_semana = model.DiaSemana,
                        hora_inicio = model.HoraInicio,
                        hora_fin = model.HoraFin,
                        duracion_cita_min = model.DuracionCita,
                        estado = "ACTIVO"
                    };

                    db.HorariosMedicos.Add(horario);
                    db.SaveChanges();

                    RegistrarEvento(db, "Create", string.Format("Horario Id: {0} creado para el doctor Id: {1}.", horario.id_horario, horario.id_doctor));

                    TempData["Success"] = "Horario médico registrado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Create", ex);
                    ModelState.AddModelError("", "Ocurrió un error al registrar el horario médico. Intente nuevamente.");
                    CargarDoctoresActivos(db, model.IdDoctor);
                    return View(model);
                }
            }
        }

        // GET: HorariosMedicos/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Horarios";

            using (var db = new MediCoreEntities())
            {
                var horario = db.HorariosMedicos.FirstOrDefault(h => h.id_horario == id);

                if (horario == null)
                {
                    TempData["Error"] = "El horario solicitado no existe.";
                    return RedirectToAction("Index");
                }

                var model = new HorarioMedicoFormModel
                {
                    IdHorario = horario.id_horario,
                    IdDoctor = horario.id_doctor,
                    DiaSemana = horario.dia_semana,
                    HoraInicio = horario.hora_inicio,
                    HoraFin = horario.hora_fin,
                    DuracionCita = horario.duracion_cita_min
                };

                CargarDoctoresActivos(db, model.IdDoctor);

                return View(model);
            }
        }

        // POST: HorariosMedicos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HorarioMedicoFormModel model)
        {
            ViewBag.ActiveMenu = "Horarios";

            using (var db = new MediCoreEntities())
            {
                ValidarHorario(db, model);

                if (!ModelState.IsValid)
                {
                    CargarDoctoresActivos(db, model.IdDoctor);
                    return View(model);
                }

                try
                {
                    var horario = db.HorariosMedicos.FirstOrDefault(h => h.id_horario == model.IdHorario);

                    if (horario == null)
                    {
                        TempData["Error"] = "El horario solicitado no existe.";
                        return RedirectToAction("Index");
                    }

                    horario.id_doctor = model.IdDoctor;
                    horario.dia_semana = model.DiaSemana;
                    horario.hora_inicio = model.HoraInicio;
                    horario.hora_fin = model.HoraFin;
                    horario.duracion_cita_min = model.DuracionCita;

                    db.SaveChanges();

                    RegistrarEvento(db, "Edit", string.Format("Horario Id: {0} actualizado.", horario.id_horario));

                    TempData["Success"] = "Horario médico actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Edit", ex);
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el horario médico. Intente nuevamente.");
                    CargarDoctoresActivos(db, model.IdDoctor);
                    return View(model);
                }
            }
        }

        // GET: HorariosMedicos/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Horarios";

            using (var db = new MediCoreEntities())
            {
                var horario = db.HorariosMedicos.FirstOrDefault(h => h.id_horario == id);

                if (horario == null)
                {
                    TempData["Error"] = "El horario solicitado no existe.";
                    return RedirectToAction("Index");
                }

                var doctor = db.Doctores.FirstOrDefault(d => d.id_doctor == horario.id_doctor);

                ViewBag.NombreDoctor = doctor != null ? doctor.nombre_completo : "—";
                ViewBag.NombreDiaSemana = DiasSemana.ContainsKey(horario.dia_semana) ? DiasSemana[horario.dia_semana] : "—";

                return View(horario);
            }
        }

        // GET: HorariosMedicos/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            ViewBag.ActiveMenu = "Horarios";

            using (var db = new MediCoreEntities())
            {
                var horario = db.HorariosMedicos.FirstOrDefault(h => h.id_horario == id);

                if (horario == null)
                {
                    TempData["Error"] = "El horario solicitado no existe.";
                    return RedirectToAction("Index");
                }

                var doctor = db.Doctores.FirstOrDefault(d => d.id_doctor == horario.id_doctor);

                ViewBag.NombreDoctor = doctor != null ? doctor.nombre_completo : "—";
                ViewBag.NombreDiaSemana = DiasSemana.ContainsKey(horario.dia_semana) ? DiasSemana[horario.dia_semana] : "—";

                return View(horario);
            }
        }

        // POST: HorariosMedicos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new MediCoreEntities())
            {
                var horario = db.HorariosMedicos.FirstOrDefault(h => h.id_horario == id);

                if (horario == null)
                {
                    TempData["Error"] = "El horario solicitado no existe.";
                    return RedirectToAction("Index");
                }

                try
                {
                    horario.estado = "INACTIVO";
                    db.SaveChanges();

                    RegistrarEvento(db, "Delete", string.Format("Horario Id: {0} desactivado.", horario.id_horario));

                    TempData["Success"] = "El horario médico fue desactivado correctamente y ya no aparecerá disponible para nuevas citas.";
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Delete", ex);
                    TempData["Error"] = "Ocurrió un error al desactivar el horario médico.";
                }

                return RedirectToAction("Index");
            }
        }

        // POST: HorariosMedicos/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, string nuevoEstado)
        {
            using (var db = new MediCoreEntities())
            {
                var horario = db.HorariosMedicos.FirstOrDefault(h => h.id_horario == id);

                if (horario == null)
                {
                    TempData["Error"] = "El horario solicitado no existe.";
                    return RedirectToAction("Index");
                }

                try
                {
                    horario.estado = nuevoEstado;
                    db.SaveChanges();

                    RegistrarEvento(db, "CambiarEstado", string.Format("Horario Id: {0} cambiado a estado '{1}'.", id, nuevoEstado));
                    TempData["Success"] = nuevoEstado == "ACTIVO"
                        ? "El horario fue activado correctamente."
                        : "El horario fue desactivado correctamente.";
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "CambiarEstado", ex);
                    TempData["Error"] = "Ocurrió un error al cambiar el estado del horario.";
                }

                return RedirectToAction("Index");
            }
        }

        private void ValidarHorario(MediCoreEntities db, HorarioMedicoFormModel model)
        {
            if (model.HoraFin <= model.HoraInicio)
            {
                ModelState.AddModelError("HoraFin", "La hora de fin debe ser mayor que la hora de inicio.");
                return;
            }

            bool doctorValido = db.Doctores.Any(d => d.id_doctor == model.IdDoctor && d.estado == "ACTIVO");
            if (!doctorValido)
            {
                ModelState.AddModelError("IdDoctor", "El doctor seleccionado no existe o está inactivo.");
                return;
            }

            bool traslapa = db.HorariosMedicos.Any(h =>
                h.id_horario != model.IdHorario
                && h.id_doctor == model.IdDoctor
                && h.dia_semana == model.DiaSemana
                && h.estado == "ACTIVO"
                && model.HoraInicio < h.hora_fin
                && model.HoraFin > h.hora_inicio);

            if (traslapa)
            {
                ModelState.AddModelError("", "Ya existe un horario activo que se traslapa para este doctor en el día y rango de horas seleccionados.");
            }
        }

        private void CargarDoctoresActivos(MediCoreEntities db, int? seleccionado)
        {
            var doctores = db.Doctores
                .Where(d => d.estado == "ACTIVO")
                .OrderBy(d => d.nombre_completo)
                .ToList();

            ViewBag.Doctores = new SelectList(doctores, "id_doctor", "nombre_completo", seleccionado);
            ViewBag.DiasSemana = DiasSemana;
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
