using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    [DoctorOAdminActionFilter]
    public class HistorialMedicoController : Controller
    {
        private readonly UtilitarioService _utilitario;
        private const string ControladorNombre = "HistorialMedico";

        public HistorialMedicoController()
        {
            _utilitario = new UtilitarioService();
        }

        public ActionResult Index(
            int? idPaciente,
            int? idDoctor,
            int? idEspecialidad,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            ViewBag.ActiveMenu = ControladorNombre;

            try
            {
                using (var db = new MediCoreEntities())
                {
                    var historial = db.HistorialMedico
                        .Include(h => h.Expedientes.Pacientes)
                        .Include(h => h.Doctores)
                        .Include(h => h.Doctores.Especialidades)
                        .Include(h => h.Citas)
                        .AsQueryable();

                    var idDoctorSesion = Session["IdDoctor"] as int?;
                    var esDoctorRol = (Session["NombreRol"] as string ?? "").ToUpper() == "DOCTOR";
                    if (esDoctorRol && idDoctorSesion.HasValue)
                        historial = historial.Where(h => h.id_doctor == idDoctorSesion.Value);
                    ViewBag.EsDoctor = esDoctorRol;

                    if (idPaciente.HasValue)
                    {
                        historial = historial.Where(h =>
                            h.Expedientes.id_paciente == idPaciente.Value);
                    }

                    if (idDoctor.HasValue)
                    {
                        historial = historial.Where(h =>
                            h.id_doctor == idDoctor.Value);
                    }

                    if (idEspecialidad.HasValue)
                    {
                        historial = historial.Where(h =>
                            h.Doctores.id_especialidad == idEspecialidad.Value);
                    }

                    if (fechaDesde.HasValue)
                    {
                        historial = historial.Where(h =>
                            DbFunctions.TruncateTime(h.fecha_consulta) >=
                            DbFunctions.TruncateTime(fechaDesde.Value));
                    }

                    if (fechaHasta.HasValue)
                    {
                        historial = historial.Where(h =>
                            DbFunctions.TruncateTime(h.fecha_consulta) <=
                            DbFunctions.TruncateTime(fechaHasta.Value));
                    }

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

                    var resultado = historial
                        .OrderByDescending(h => h.fecha_consulta)
                        .Select(h => new HistorialListaModel
                        {
                            IdHistorial        = h.id_historial,
                            FechaConsulta      = h.fecha_consulta,
                            NombrePaciente     = h.Expedientes.Pacientes.nombre_completo,
                            NombreDoctor       = h.Doctores.nombre_completo,
                            NombreEspecialidad = h.Doctores.Especialidades.nombre,
                            Diagnostico        = h.diagnostico
                        })
                        .ToList();

                    _utilitario.RegistrarEvento(
                        ControladorNombre,
                        "Index",
                        $"Consulta de historial médico realizada. Filtros: Paciente={idPaciente}, Doctor={idDoctor}, Especialidad={idEspecialidad}"
                    );

                    return View(resultado);
                }
            }
            catch (Exception ex)
            {
                _utilitario.RegistrarErrorBitacora(ex, ControladorNombre, "Index");
                TempData["Error"] = "Ocurrió un error al cargar la consulta del historial médico.";
                return View(new System.Collections.Generic.List<HistorialListaModel>());
            }
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            ViewBag.ActiveMenu = ControladorNombre;

            if (!id.HasValue)
            {
                TempData["Error"] = "Debe seleccionar un registro del historial.";
                return RedirectToAction("Index");
            }

            try
            {
                using (var db = new MediCoreEntities())
                {
                    var historial = db.HistorialMedico
                        .Include(h => h.Expedientes.Pacientes)
                        .Include(h => h.Doctores)
                        .Include(h => h.Doctores.Especialidades)
                        .Include(h => h.Citas)
                        .FirstOrDefault(h => h.id_historial == id.Value);

                    if (historial == null)
                    {
                        TempData["Error"] = "El registro del historial médico no existe.";
                        _utilitario.RegistrarEvento(
                            ControladorNombre,
                            "Details",
                            $"Intento fallido de ver detalle. Historial ID {id} no encontrado."
                        );
                        return RedirectToAction("Index");
                    }

                    _utilitario.RegistrarEvento(
                        ControladorNombre,
                        "Details",
                        $"Consulta detallada del registro de historial ID: {id}"
                    );

                    var model = new HistorialDetalleModel
                    {
                        IdHistorial        = historial.id_historial,
                        IdExpediente       = historial.id_expediente,
                        IdCita             = historial.id_cita,
                        NombrePaciente     = historial.Expedientes.Pacientes.nombre_completo,
                        CedulaPaciente     = historial.Expedientes.Pacientes.cedula,
                        SexoPaciente       = historial.Expedientes.Pacientes.sexo,
                        FechaConsulta      = historial.fecha_consulta,
                        NombreDoctor       = historial.Doctores.nombre_completo,
                        NombreEspecialidad = historial.Doctores.Especialidades.nombre,
                        Sintomas           = historial.sintomas,
                        Diagnostico        = historial.diagnostico,
                        Tratamiento        = historial.tratamiento,
                        Medicamentos       = historial.medicamentos,
                        Observaciones      = historial.observaciones,
                        ProximaCita        = historial.proxima_cita
                    };

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _utilitario.RegistrarErrorBitacora(ex, ControladorNombre, "Details");
                TempData["Error"] = "Ocurrió un error al consultar el registro del historial.";
                return RedirectToAction("Index");
            }
        }
    }
}