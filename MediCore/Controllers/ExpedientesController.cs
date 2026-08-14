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
    public class ExpedientesController : Controller
    {
        private readonly UtilitarioService _utilitario;
        private const string ControladorNombre = "Expedientes";

        public ExpedientesController()
        {
            _utilitario = new UtilitarioService();
        }

        public ActionResult Index(string buscar, string tipoSangre)
        {
            ViewBag.ActiveMenu = ControladorNombre;

            try
            {
                using (var db = new MediCoreEntities())
                {
                    var expedientes = db.Expedientes
                        .Include(e => e.Pacientes)
                        .Include(e => e.HistorialMedico)
                        .AsQueryable();

                    // Si el usuario es doctor, ver solo expedientes de sus pacientes asignados
                    var idDoctorSesion = Session["IdDoctor"] as int?;
                    var esDoctorRol = (Session["NombreRol"] as string ?? "").ToUpper() == "DOCTOR";
                    if (esDoctorRol && idDoctorSesion.HasValue)
                    {
                        var idsPacientes = db.Citas
                            .Where(c => c.id_doctor == idDoctorSesion.Value)
                            .Select(c => c.id_paciente).Distinct().ToList();
                        expedientes = expedientes.Where(e => idsPacientes.Contains(e.id_paciente));
                    }
                    ViewBag.EsDoctor = esDoctorRol;

                    // Filtro por nombre o cédula
                    if (!string.IsNullOrWhiteSpace(buscar))
                    {
                        expedientes = expedientes.Where(e =>
                            e.Pacientes.nombre_completo.Contains(buscar) ||
                            e.Pacientes.cedula.Contains(buscar));
                    }

                    // Filtro por tipo de sangre
                    if (!string.IsNullOrWhiteSpace(tipoSangre))
                    {
                        expedientes = expedientes.Where(e =>
                            e.tipo_sangre == tipoSangre);
                    }

                    ViewBag.Buscar = buscar;
                    ViewBag.TipoSangre = tipoSangre;

                    var resultado = expedientes
                        .OrderBy(e => e.Pacientes.nombre_completo)
                        .ToList()
                        .Select(e => new ExpedienteListaModel
                        {
                            IdExpediente   = e.id_expediente,
                            NombrePaciente = e.Pacientes.nombre_completo,
                            CedulaPaciente = e.Pacientes.cedula,
                            TipoSangre     = e.tipo_sangre,
                            FechaApertura  = e.fecha_apertura,
                            ConsultasCount = e.HistorialMedico.Count
                        })
                        .ToList();

                    _utilitario.RegistrarEvento(
                        ControladorNombre,
                        "Index",
                        $"Consulta de expedientes realizada. Filtros: buscar='{buscar}', tipoSangre='{tipoSangre}'"
                    );

                    return View(resultado);
                }
            }
            catch (Exception ex)
            {
                _utilitario.RegistrarErrorBitacora(ex, ControladorNombre, "Index");
                TempData["Error"] = "Ocurrió un error al cargar la lista de expedientes.";
                return View(new System.Collections.Generic.List<ExpedienteListaModel>());
            }
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            ViewBag.ActiveMenu = ControladorNombre;

            if (!id.HasValue)
            {
                TempData["Error"] = "Debe seleccionar un expediente.";
                return RedirectToAction("Index");
            }

            try
            {
                using (var db = new MediCoreEntities())
                {
                    var expediente = db.Expedientes
                        .Include(e => e.Pacientes)
                        .Include(e => e.HistorialMedico.Select(h => h.Doctores))
                        .FirstOrDefault(e => e.id_expediente == id.Value);

                    if (expediente == null)
                    {
                        TempData["Error"] = "El expediente solicitado no existe.";
                        _utilitario.RegistrarEvento(
                            ControladorNombre,
                            "Details",
                            $"Intento fallido de ver detalle. Expediente ID {id} no encontrado."
                        );
                        return RedirectToAction("Index");
                    }

                    _utilitario.RegistrarEvento(
                        ControladorNombre,
                        "Details",
                        $"Consulta detallada del expediente ID: {id}"
                    );

                    var model = new ExpedienteDetalleModel
                    {
                        IdExpediente           = expediente.id_expediente,
                        NombrePaciente         = expediente.Pacientes.nombre_completo,
                        CedulaPaciente         = expediente.Pacientes.cedula,
                        FechaNacimientoPaciente = expediente.Pacientes.fecha_nacimiento,
                        SexoPaciente           = expediente.Pacientes.sexo,
                        TelefonoPaciente       = expediente.Pacientes.telefono,
                        CorreoPaciente         = expediente.Pacientes.correo,
                        DireccionPaciente      = expediente.Pacientes.direccion,
                        TipoSangre             = expediente.tipo_sangre,
                        FechaApertura          = expediente.fecha_apertura,
                        Alergias               = expediente.alergias,
                        Antecedentes           = expediente.antecedentes,
                        ConsultasCount         = expediente.HistorialMedico.Count,
                        Historial              = expediente.HistorialMedico
                            .OrderByDescending(h => h.fecha_consulta)
                            .Select(h => new HistorialResumenModel
                            {
                                IdHistorial   = h.id_historial,
                                FechaConsulta = h.fecha_consulta,
                                NombreDoctor  = h.Doctores.nombre_completo,
                                Diagnostico   = h.diagnostico,
                                ProximaCita   = h.proxima_cita
                            }).ToList()
                    };

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _utilitario.RegistrarErrorBitacora(ex, ControladorNombre, "Details");
                TempData["Error"] = "Ocurrió un error al consultar el expediente.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            ViewBag.ActiveMenu = ControladorNombre;

            if (!id.HasValue)
            {
                TempData["Error"] = "Debe seleccionar un expediente.";
                return RedirectToAction("Index");
            }

            try
            {
                using (var db = new MediCoreEntities())
                {
                    var expediente = db.Expedientes
                        .Include(e => e.Pacientes)
                        .FirstOrDefault(e => e.id_expediente == id.Value);

                    if (expediente == null)
                    {
                        TempData["Error"] = "El expediente solicitado no existe.";
                        return RedirectToAction("Index");
                    }

                    var model = new ExpedienteEditModel
                    {
                        IdExpediente   = expediente.id_expediente,
                        NombrePaciente = expediente.Pacientes.nombre_completo,
                        CedulaPaciente = expediente.Pacientes.cedula,
                        FechaApertura  = expediente.fecha_apertura,
                        TipoSangre     = expediente.tipo_sangre,
                        Alergias       = expediente.alergias,
                        Antecedentes   = expediente.antecedentes
                    };

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _utilitario.RegistrarErrorBitacora(ex, ControladorNombre, "Edit [GET]");
                TempData["Error"] = "Ocurrió un error al cargar la edición del expediente.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExpedienteEditModel model)
        {
            ViewBag.ActiveMenu = ControladorNombre;

            try
            {
                using (var db = new MediCoreEntities())
                {
                    var expediente = db.Expedientes
                        .FirstOrDefault(e => e.id_expediente == model.IdExpediente);

                    if (expediente == null)
                    {
                        TempData["Error"] = "El expediente solicitado no existe.";
                        return RedirectToAction("Index");
                    }

                    expediente.tipo_sangre = model.TipoSangre;
                    expediente.alergias    = model.Alergias;
                    expediente.antecedentes = model.Antecedentes;

                    db.SaveChanges();

                    _utilitario.RegistrarEvento(
                        ControladorNombre,
                        "Edit [POST]",
                        $"Se actualizó correctamente el expediente ID: {model.IdExpediente}"
                    );

                    TempData["Success"] = "El expediente fue actualizado correctamente.";

                    return RedirectToAction("Details", new { id = expediente.id_expediente });
                }
            }
            catch (Exception ex)
            {
                _utilitario.RegistrarErrorBitacora(ex, ControladorNombre, "Edit [POST]");
                TempData["Error"] = "Ocurrió un error al actualizar el expediente.";
                return View(model);
            }
        }
    }
}