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
                return View(new System.Collections.Generic.List<Expedientes>());
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

                    return View(expediente);
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

                    return View(expediente);
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
        public ActionResult Edit(Expedientes model)
        {
            ViewBag.ActiveMenu = ControladorNombre;

            try
            {
                using (var db = new MediCoreEntities())
                {
                    var expediente = db.Expedientes
                        .FirstOrDefault(e => e.id_expediente == model.id_expediente);

                    if (expediente == null)
                    {
                        TempData["Error"] = "El expediente solicitado no existe.";
                        return RedirectToAction("Index");
                    }

                    expediente.tipo_sangre = model.tipo_sangre;
                    expediente.alergias = model.alergias;
                    expediente.antecedentes = model.antecedentes;

                    db.SaveChanges();

                    _utilitario.RegistrarEvento(
                        ControladorNombre,
                        "Edit [POST]",
                        $"Se actualizó correctamente el expediente ID: {model.id_expediente}"
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