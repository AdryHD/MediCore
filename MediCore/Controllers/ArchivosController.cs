using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    public class ArchivosController : Controller
    {
        private const string NombreControlador = "Archivos";
        private readonly UtilitarioService _utilitario = new UtilitarioService();

        // GET: Archivos
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = NombreControlador;

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var archivos = db.Archivos
                        .OrderBy(a => a.nombre)
                        .ToList();

                    _utilitario.RegistrarEvento(
                        NombreControlador,
                        MethodBase.GetCurrentMethod().Name,
                        "Consulta de lista de archivos realizada."
                    );

                    return View(archivos);
                }
                catch (Exception ex)
                {
                    _utilitario.RegistrarErrorBitacora(ex, NombreControlador, MethodBase.GetCurrentMethod().Name);
                    TempData["Error"] = "Ocurrió un error al cargar los archivos.";
                    return View(new List<Archivos>());
                }
            }
        }
        private void CargarExpedientesViewBag()
        {
            using (var db = new MediCoreEntities())
            {
                ViewBag.ExpedientesList = db.Expedientes
                    .Select(e => new SelectListItem
                    {
                        Value = e.id_expediente.ToString(),
                        Text = "Expediente #" + e.id_expediente + " - " + e.Pacientes.nombre_completo + " (Cédula: " + e.Pacientes.cedula + ")"
                    })
                    .ToList();
            }
        }
        // GET: Archivos/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = NombreControlador;
            CargarExpedientesViewBag(); 
            return View(new ArchivosFormModel());
        }

        // POST: Archivos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ArchivosFormModel model, HttpPostedFileBase archivoSubido)
        {
            ViewBag.ActiveMenu = NombreControlador;

            if (!ModelState.IsValid)
            {
                CargarExpedientesViewBag(); 
                return View(model);
            }

            using (var db = new MediCoreEntities())
            {
                try
                {
                    bool existeNombre = db.Archivos
                        .Any(a => a.nombre.Trim().ToUpper() == model.Nombre.Trim().ToUpper());

                    if (existeNombre)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un archivo registrado con ese nombre.");
                        CargarExpedientesViewBag(); 
                        return View(model);
                    }

                    int? idUsuarioSesion = Session["Consecutivo"] as int?;

                    byte[] contenidoBytes = null;
                    if (archivoSubido != null && archivoSubido.ContentLength > 0)
                    {
                        contenidoBytes = new byte[archivoSubido.ContentLength];
                        archivoSubido.InputStream.Read(contenidoBytes, 0, archivoSubido.ContentLength);
                        model.Tipo_mime = archivoSubido.ContentType;
                        model.Tamano_bytes = archivoSubido.ContentLength;
                    }

                    var archivo = new Archivos
                    {
                        id_expediente = model.Id_Expediente,
                        id_usuario = idUsuarioSesion ?? model.Id_Usuario,
                        nombre = model.Nombre.Trim(),
                        tipo_mime = model.Tipo_mime,
                        tamano_bytes = model.Tamano_bytes,
                        contenido = contenidoBytes ?? model.Contenido,
                        estado = "ACTIVO",
                        fecha_carga = DateTime.Now
                    };

                    db.Archivos.Add(archivo);
                    db.SaveChanges();

                    _utilitario.RegistrarEvento(
                        NombreControlador,
                        MethodBase.GetCurrentMethod().Name,
                        string.Format("Archivo '{0}' creado (Id: {1}).", archivo.nombre, archivo.id_archivo)
                    );

                    TempData["Success"] = "Archivo registrado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _utilitario.RegistrarErrorBitacora(ex, NombreControlador, MethodBase.GetCurrentMethod().Name);
                    ModelState.AddModelError("", "Ocurrió un error al registrar el archivo. Intente nuevamente.");

                    CargarExpedientesViewBag(); 
                    return View(model);
                }
            }
        }

        // GET: Archivos/Edit
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = NombreControlador;

            using (var db = new MediCoreEntities())
            {
                var archivo = db.Archivos.Find(id);
                if (archivo == null)
                {
                    return HttpNotFound();
                }

                var model = new ArchivosFormModel
                {
                    Id_Archivo = archivo.id_archivo,
                    Id_Expediente = archivo.id_expediente,
                    Id_Usuario = archivo.id_usuario,
                    Nombre = archivo.nombre,
                    Tipo_mime = archivo.tipo_mime,
                    Tamano_bytes = archivo.tamano_bytes,
                    Estado = archivo.estado,
                    Fecha_carga = archivo.fecha_carga
                };

                CargarExpedientesViewBag(); 
                return View(model);
            }
        }

        // POST: Archivos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ArchivosFormModel model, HttpPostedFileBase archivoSubido)
        {
            ViewBag.ActiveMenu = NombreControlador;

            if (!ModelState.IsValid)
            {
                CargarExpedientesViewBag(); 
                return View(model);
            }

            using (var db = new MediCoreEntities())
            {
                try
                {

                    bool existeNombre = db.Archivos
                        .Any(a => a.nombre.Trim().ToUpper() == model.Nombre.Trim().ToUpper() && a.id_archivo != model.Id_Archivo);

                    if (existeNombre)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe otro archivo registrado con ese nombre.");
                        CargarExpedientesViewBag(); 
                        return View(model);
                    }

                    var archivoBD = db.Archivos.Find(model.Id_Archivo);
                    if (archivoBD != null)
                    {
                        archivoBD.id_expediente = model.Id_Expediente;
                        archivoBD.nombre = model.Nombre.Trim();
                        archivoBD.tipo_mime = model.Tipo_mime;
                        archivoBD.estado = model.Estado;

                        if (archivoSubido != null && archivoSubido.ContentLength > 0)
                        {
                            byte[] contenidoBytes = new byte[archivoSubido.ContentLength];
                            archivoSubido.InputStream.Read(contenidoBytes, 0, archivoSubido.ContentLength);

                            archivoBD.contenido = contenidoBytes;
                            archivoBD.tipo_mime = archivoSubido.ContentType;
                            archivoBD.tamano_bytes = archivoSubido.ContentLength;
                        }

                        db.SaveChanges();

                        _utilitario.RegistrarEvento(
                            NombreControlador,
                            MethodBase.GetCurrentMethod().Name,
                            string.Format("Archivo '{0}' (Id: {1}) actualizado.", archivoBD.nombre, archivoBD.id_archivo)
                        );

                        TempData["Success"] = "Archivo actualizado correctamente.";
                        return RedirectToAction("Index");
                    }

                    return HttpNotFound();
                }
                catch (Exception ex)
                {
                    _utilitario.RegistrarErrorBitacora(ex, NombreControlador, MethodBase.GetCurrentMethod().Name);
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el archivo. Intente nuevamente.");

                    CargarExpedientesViewBag(); 
                    return View(model);
                }
            }
        }

        // POST: Archivos/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, string nuevoEstado)
        {
            using (var db = new MediCoreEntities())
            {
                try
                {
                    var archivo = db.Archivos.FirstOrDefault(a => a.id_archivo == id);

                    if (archivo == null)
                    {
                        TempData["Error"] = "El archivo solicitado no existe.";
                        return RedirectToAction("Index");
                    }

                    archivo.estado = nuevoEstado;
                    db.SaveChanges();

                    _utilitario.RegistrarEvento(
                        NombreControlador,
                        MethodBase.GetCurrentMethod().Name,
                        string.Format("Archivo ID {0} cambió su estado a '{1}'.", id, nuevoEstado)
                    );

                    TempData["Success"] = nuevoEstado == "ACTIVO"
                        ? "El archivo fue activado correctamente."
                        : "El archivo fue desactivado correctamente.";
                }
                catch (Exception ex)
                {
                    _utilitario.RegistrarErrorBitacora(ex, NombreControlador, MethodBase.GetCurrentMethod().Name);
                    TempData["Error"] = "Ocurrió un error al cambiar el estado del archivo.";
                }
            }

            return RedirectToAction("Index");
        }

        // GET: Archivos/Details/5
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Debe especificar un identificador de archivo válido.";
                return RedirectToAction("Index");
            }

            using (var db = new MediCoreEntities())
            {
                var archivo = db.Archivos
                .Include("Expedientes.Pacientes")
                .Include("tbUsuario") 
                .FirstOrDefault(a => a.id_archivo == id.Value);

                if (archivo == null)
                {
                    TempData["Error"] = "El archivo solicitado no fue encontrado.";
                    return RedirectToAction("Index");
                }

                ViewBag.ActiveMenu = NombreControlador;
                return View(archivo);
            }
        }
    }

}