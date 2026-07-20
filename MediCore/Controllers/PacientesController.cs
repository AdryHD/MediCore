using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    public class PacientesController : Controller
    {
        private const string NombreControlador = "Pacientes";
        private const int TamanoPagina = 10;

        // GET: Pacientes
        public ActionResult Index(string q, string estado, int page = 1)
        {
            ViewBag.ActiveMenu = "Pacientes";

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var query = db.Pacientes.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        var texto = q.Trim().ToUpper();
                        query = query.Where(p => p.nombre_completo.ToUpper().Contains(texto)
                                                  || p.cedula.ToUpper().Contains(texto));
                    }

                    if (!string.IsNullOrWhiteSpace(estado))
                    {
                        query = query.Where(p => p.estado == estado);
                    }

                    int totalRegistros = query.Count();
                    if (page < 1) page = 1;
                    int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanoPagina);

                    var pacientes = query
                        .OrderBy(p => p.nombre_completo)
                        .Skip((page - 1) * TamanoPagina)
                        .Take(TamanoPagina)
                        .ToList();

                    ViewBag.FiltroTexto = q;
                    ViewBag.FiltroEstado = estado;
                    ViewBag.PaginaActual = page;
                    ViewBag.TotalPaginas = totalPaginas;
                    ViewBag.TotalRegistros = totalRegistros;

                    return View(pacientes);
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Index", ex);
                    TempData["Error"] = "Ocurrió un error al cargar los pacientes.";
                    return View(new List<Pacientes>());
                }
            }
        }

        // GET: Pacientes/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Pacientes";
            return View(new PacienteFormModel());
        }

        // POST: Pacientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PacienteFormModel model)
        {
            ViewBag.ActiveMenu = "Pacientes";

            using (var db = new MediCoreEntities())
            {
                ValidarPaciente(db, model);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var paciente = new Pacientes
                        {
                            nombre_completo = model.NombreCompleto.Trim(),
                            cedula = model.Cedula.Trim(),
                            fecha_nacimiento = model.FechaNacimiento.Date,
                            sexo = model.Sexo,
                            telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim(),
                            correo = model.CorreoElectronico.Trim(),
                            direccion = string.IsNullOrWhiteSpace(model.Direccion) ? null : model.Direccion.Trim(),
                            estado = "ACTIVO",
                            fecha_registro = DateTime.Now
                        };

                        db.Pacientes.Add(paciente);
                        db.SaveChanges();

                        var expediente = new Expedientes
                        {
                            id_paciente = paciente.id_paciente,
                            tipo_sangre = string.IsNullOrWhiteSpace(model.TipoSangre) ? null : model.TipoSangre.Trim(),
                            alergias = string.IsNullOrWhiteSpace(model.Alergias) ? null : model.Alergias.Trim(),
                            antecedentes = string.IsNullOrWhiteSpace(model.AntecedentesMedicos) ? null : model.AntecedentesMedicos.Trim(),
                            fecha_apertura = DateTime.Now
                        };

                        db.Expedientes.Add(expediente);
                        db.SaveChanges();

                        transaction.Commit();

                        RegistrarEvento(db, "Create", string.Format("Paciente '{0}' (Cédula: {1}) creado con Id: {2}.", paciente.nombre_completo, paciente.cedula, paciente.id_paciente));

                        TempData["Success"] = "Paciente registrado correctamente.";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        RegistrarError(db, "Create", ex);
                        ModelState.AddModelError("", "Ocurrió un error al registrar el paciente. Intente nuevamente.");
                        return View(model);
                    }
                }
            }
        }

        // GET: Pacientes/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Pacientes";

            using (var db = new MediCoreEntities())
            {
                var paciente = db.Pacientes.FirstOrDefault(p => p.id_paciente == id);

                if (paciente == null)
                {
                    TempData["Error"] = "El paciente solicitado no existe.";
                    return RedirectToAction("Index");
                }

                var expediente = db.Expedientes.FirstOrDefault(e => e.id_paciente == id);

                var model = new PacienteFormModel
                {
                    IdPaciente = paciente.id_paciente,
                    NombreCompleto = paciente.nombre_completo,
                    Cedula = paciente.cedula,
                    FechaNacimiento = paciente.fecha_nacimiento,
                    Sexo = paciente.sexo,
                    Telefono = paciente.telefono,
                    CorreoElectronico = paciente.correo,
                    Direccion = paciente.direccion,
                    TipoSangre = expediente != null ? expediente.tipo_sangre : null,
                    Alergias = expediente != null ? expediente.alergias : null,
                    AntecedentesMedicos = expediente != null ? expediente.antecedentes : null
                };

                return View(model);
            }
        }

        // POST: Pacientes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PacienteFormModel model)
        {
            ViewBag.ActiveMenu = "Pacientes";

            using (var db = new MediCoreEntities())
            {
                ValidarPaciente(db, model);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                try
                {
                    var paciente = db.Pacientes.FirstOrDefault(p => p.id_paciente == model.IdPaciente);

                    if (paciente == null)
                    {
                        TempData["Error"] = "El paciente solicitado no existe.";
                        return RedirectToAction("Index");
                    }

                    paciente.nombre_completo = model.NombreCompleto.Trim();
                    paciente.cedula = model.Cedula.Trim();
                    paciente.fecha_nacimiento = model.FechaNacimiento.Date;
                    paciente.sexo = model.Sexo;
                    paciente.telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();
                    paciente.correo = model.CorreoElectronico.Trim();
                    paciente.direccion = string.IsNullOrWhiteSpace(model.Direccion) ? null : model.Direccion.Trim();

                    var expediente = db.Expedientes.FirstOrDefault(e => e.id_paciente == model.IdPaciente);

                    if (expediente == null)
                    {
                        expediente = new Expedientes
                        {
                            id_paciente = paciente.id_paciente,
                            fecha_apertura = DateTime.Now
                        };
                        db.Expedientes.Add(expediente);
                    }

                    expediente.tipo_sangre = string.IsNullOrWhiteSpace(model.TipoSangre) ? null : model.TipoSangre.Trim();
                    expediente.alergias = string.IsNullOrWhiteSpace(model.Alergias) ? null : model.Alergias.Trim();
                    expediente.antecedentes = string.IsNullOrWhiteSpace(model.AntecedentesMedicos) ? null : model.AntecedentesMedicos.Trim();

                    db.SaveChanges();

                    RegistrarEvento(db, "Edit", string.Format("Paciente '{0}' (Id: {1}) actualizado.", paciente.nombre_completo, paciente.id_paciente));

                    TempData["Success"] = "Paciente actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Edit", ex);
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el paciente. Intente nuevamente.");
                    return View(model);
                }
            }
        }

        // GET: Pacientes/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Pacientes";

            using (var db = new MediCoreEntities())
            {
                var paciente = db.Pacientes.FirstOrDefault(p => p.id_paciente == id);

                if (paciente == null)
                {
                    TempData["Error"] = "El paciente solicitado no existe.";
                    return RedirectToAction("Index");
                }

                ViewBag.Expediente = db.Expedientes.FirstOrDefault(e => e.id_paciente == id);

                return View(paciente);
            }
        }

        // GET: Pacientes/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            ViewBag.ActiveMenu = "Pacientes";

            using (var db = new MediCoreEntities())
            {
                var paciente = db.Pacientes.FirstOrDefault(p => p.id_paciente == id);

                if (paciente == null)
                {
                    TempData["Error"] = "El paciente solicitado no existe.";
                    return RedirectToAction("Index");
                }

                return View(paciente);
            }
        }

        // POST: Pacientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new MediCoreEntities())
            {
                var paciente = db.Pacientes.FirstOrDefault(p => p.id_paciente == id);

                if (paciente == null)
                {
                    TempData["Error"] = "El paciente solicitado no existe.";
                    return RedirectToAction("Index");
                }

                try
                {
                    paciente.estado = "INACTIVO";
                    db.SaveChanges();

                    RegistrarEvento(db, "Delete", string.Format("Paciente Id: {0} desactivado.", paciente.id_paciente));

                    TempData["Success"] = "El paciente fue desactivado correctamente y ya no aparecerá disponible para nuevas citas.";
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "Delete", ex);
                    TempData["Error"] = "Ocurrió un error al desactivar el paciente.";
                }

                return RedirectToAction("Index");
            }
        }

        // POST: Pacientes/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, string nuevoEstado)
        {
            using (var db = new MediCoreEntities())
            {
                var paciente = db.Pacientes.FirstOrDefault(p => p.id_paciente == id);

                if (paciente == null)
                {
                    TempData["Error"] = "El paciente solicitado no existe.";
                    return RedirectToAction("Index");
                }

                try
                {
                    paciente.estado = nuevoEstado;
                    db.SaveChanges();

                    RegistrarEvento(db, "CambiarEstado", string.Format("Paciente Id: {0} cambiado a estado '{1}'.", id, nuevoEstado));
                    TempData["Success"] = nuevoEstado == "ACTIVO"
                        ? "El paciente fue activado correctamente."
                        : "El paciente fue desactivado correctamente.";
                }
                catch (Exception ex)
                {
                    RegistrarError(db, "CambiarEstado", ex);
                    TempData["Error"] = "Ocurrió un error al cambiar el estado del paciente.";
                }

                return RedirectToAction("Index");
            }
        }

        private void ValidarPaciente(MediCoreEntities db, PacienteFormModel model)
        {
            if (model.Sexo != "M" && model.Sexo != "F" && model.Sexo != "OTRO")
            {
                ModelState.AddModelError("Sexo", "Debe seleccionar un sexo válido.");
            }

            if (model.FechaNacimiento.Date > DateTime.Today)
            {
                ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser una fecha futura.");
            }

            if (!ModelState.IsValid)
            {
                return;
            }

            bool existeCedula = db.Pacientes.Any(p => p.id_paciente != model.IdPaciente
                                      && p.cedula.Trim().ToUpper() == model.Cedula.Trim().ToUpper());
            if (existeCedula)
            {
                ModelState.AddModelError("Cedula", "Ya existe un paciente registrado con esa cédula.");
            }

            bool existeCorreo = db.Pacientes.Any(p => p.id_paciente != model.IdPaciente
                                      && p.correo.Trim().ToUpper() == model.CorreoElectronico.Trim().ToUpper());
            if (existeCorreo)
            {
                ModelState.AddModelError("CorreoElectronico", "Ya existe un paciente registrado con ese correo electrónico.");
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
