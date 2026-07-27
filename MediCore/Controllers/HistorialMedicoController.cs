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
    public class HistorialMedicoController : Controller
    {
        public ActionResult Index(
            int? idPaciente,
            int? idDoctor,
            int? idEspecialidad,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            ViewBag.ActiveMenu = "HistorialMedico";

            using (var db = new MediCoreEntities())
            {
                var historial = db.HistorialMedico
                    .Include(h => h.Expedientes.Pacientes)
                    .Include(h => h.Doctores)
                    .Include(h => h.Doctores.Especialidades)
                    .Include(h => h.Citas)
                    .AsQueryable();

                // Paciente
                if (idPaciente.HasValue)
                {
                    historial = historial.Where(h =>
                        h.Expedientes.id_paciente == idPaciente.Value);
                }

                // Doctor
                if (idDoctor.HasValue)
                {
                    historial = historial.Where(h =>
                        h.id_doctor == idDoctor.Value);
                }

                // Especialidad
                if (idEspecialidad.HasValue)
                {
                    historial = historial.Where(h =>
                        h.Doctores.id_especialidad == idEspecialidad.Value);
                }

                // Fecha desde
                if (fechaDesde.HasValue)
                {
                    historial = historial.Where(h =>
                        DbFunctions.TruncateTime(h.fecha_consulta) >=
                        DbFunctions.TruncateTime(fechaDesde.Value));
                }

                // Fecha hasta
                if (fechaHasta.HasValue)
                {
                    historial = historial.Where(h =>
                        DbFunctions.TruncateTime(h.fecha_consulta) <=
                        DbFunctions.TruncateTime(fechaHasta.Value));
                }

                // Combos
                ViewBag.Pacientes = new SelectList(
                    db.Pacientes
                        .Where(p => p.estado == "ACTIVO")
                        .OrderBy(p => p.nombre_completo)
                        .ToList(),
                    "id_paciente",
                    "nombre_completo",
                    idPaciente
                );

                ViewBag.Doctores = new SelectList(
                    db.Doctores
                        .Where(d => d.estado == "ACTIVO")
                        .OrderBy(d => d.nombre_completo)
                        .ToList(),
                    "id_doctor",
                    "nombre_completo",
                    idDoctor
                );

                ViewBag.Especialidades = new SelectList(
                    db.Especialidades
                        .Where(e => e.estado == "ACTIVO")
                        .OrderBy(e => e.nombre)
                        .ToList(),
                    "id_especialidad",
                    "nombre",
                    idEspecialidad
                );

                ViewBag.FechaDesde = fechaDesde;
                ViewBag.FechaHasta = fechaHasta;

                return View(
                    historial
                        .OrderByDescending(h => h.fecha_consulta)
                        .ToList()
                );
            }
        }


        [HttpGet]
        public ActionResult Details(int? id)
        {
            ViewBag.ActiveMenu = "HistorialMedico";

            if (!id.HasValue)
            {
                TempData["Error"] =
                    "Debe seleccionar un registro del historial.";

                return RedirectToAction("Index");
            }

            using (var db = new MediCoreEntities())
            {
                var historial = db.HistorialMedico
                    .Include(h => h.Expedientes.Pacientes)
                    .Include(h => h.Doctores)
                    .Include(h => h.Doctores.Especialidades)
                    .Include(h => h.Citas)
                    .FirstOrDefault(h =>
                        h.id_historial == id.Value);

                if (historial == null)
                {
                    TempData["Error"] =
                        "El registro del historial médico no existe.";

                    return RedirectToAction("Index");
                }

                return View(historial);
            }
        }
    }
}