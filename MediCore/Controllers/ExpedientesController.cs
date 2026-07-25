using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;


namespace MediCore.Controllers
{
    public class ExpedientesController : Controller
    {
        public ActionResult Index(string buscar, string tipoSangre)
        {
            ViewBag.ActiveMenu = "Expedientes";

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

                return View(resultado);
            }
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            ViewBag.ActiveMenu = "Expedientes";

            if (!id.HasValue)
            {
                TempData["Error"] = "Debe seleccionar un expediente.";
                return RedirectToAction("Index");
            }

            using (var db = new MediCoreEntities())
            {
                var expediente = db.Expedientes
                    .Include(e => e.Pacientes)
                    .Include(e => e.HistorialMedico.Select(h => h.Doctores))
                    .FirstOrDefault(e => e.id_expediente == id.Value);

                if (expediente == null)
                {
                    TempData["Error"] = "El expediente solicitado no existe.";
                    return RedirectToAction("Index");
                }

                return View(expediente);
            }
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            ViewBag.ActiveMenu = "Expedientes";

            if (!id.HasValue)
            {
                TempData["Error"] = "Debe seleccionar un expediente.";
                return RedirectToAction("Index");
            }

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Expedientes model)
        {
            ViewBag.ActiveMenu = "Expedientes";

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

                TempData["Success"] = "El expediente fue actualizado correctamente.";

                return RedirectToAction(
                    "Details",
                    new { id = expediente.id_expediente }
                );
            }
        }




    }
}
