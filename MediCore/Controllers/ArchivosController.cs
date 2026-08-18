using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    [AuthActionFilter]
    [DoctorOAdminActionFilter]
    public class ArchivosController : Controller
    {
        private const string NombreControlador = "Archivos";
        private readonly UtilitarioService _utilitario = new UtilitarioService();

        private string CarpetaUploads
        {
            get
            {
                var ruta = Path.GetFullPath(
                    Path.Combine(Server.MapPath("~/"), "..", "Uploads", "Archivos"));
                if (!Directory.Exists(ruta))
                    Directory.CreateDirectory(ruta);
                return ruta;
            }
        }

        private void EliminarArchivoDeDisco(byte[] contenido)
        {
            if (contenido == null || contenido.Length == 0) return;
            try
            {
                var nombreArchivo = Encoding.UTF8.GetString(contenido);
                if (!string.IsNullOrWhiteSpace(nombreArchivo))
                {
                    var rutaFisica = Path.Combine(CarpetaUploads, nombreArchivo);
                    if (System.IO.File.Exists(rutaFisica))
                        System.IO.File.Delete(rutaFisica);
                }
            }
            catch { }
        }

        // GET: Archivos
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = NombreControlador;

            using (var db = new MediCoreEntities())
            {
                try
                {
                    var esDoctorRol = (Session["NombreRol"] as string ?? "").ToUpper() == "DOCTOR";
                    var idDoctorSesion = Session["IdDoctor"] as int?;

                    List<ArchivoListaModel> archivos;
                    if (esDoctorRol && idDoctorSesion.HasValue)
                    {
                        var idsPacientes = db.Citas
                            .Where(c => c.id_doctor == idDoctorSesion.Value)
                            .Select(c => c.id_paciente).Distinct().ToList();
                        archivos = db.Archivos
                            .Include("Expedientes")
                            .Where(a => a.id_expediente != null &&
                                        idsPacientes.Contains(a.Expedientes.id_paciente))
                            .OrderBy(a => a.nombre)
                            .Select(a => new ArchivoListaModel
                            {
                                IdArchivo    = a.id_archivo,
                                Nombre       = a.nombre,
                                TipoMime     = a.tipo_mime,
                                TamanioBytes = a.tamano_bytes,
                                FechaCarga   = a.fecha_carga,
                                Estado       = a.estado
                            })
                            .ToList();
                    }
                    else
                    {
                        archivos = db.Archivos
                            .OrderBy(a => a.nombre)
                            .Select(a => new ArchivoListaModel
                            {
                                IdArchivo    = a.id_archivo,
                                Nombre       = a.nombre,
                                TipoMime     = a.tipo_mime,
                                TamanioBytes = a.tamano_bytes,
                                FechaCarga   = a.fecha_carga,
                                Estado       = a.estado
                            })
                            .ToList();
                    }
                    ViewBag.EsDoctor = esDoctorRol;

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
                    return View(new List<ArchivoListaModel>());
                }
            }
        }
        private void CargarExpedientesViewBag()
        {
            using (var db = new MediCoreEntities())
            {
                var esDoctorRol = (Session["NombreRol"] as string ?? "").ToUpper() == "DOCTOR";
                var idDoctorSesion = Session["IdDoctor"] as int?;

                IQueryable<EF.Expedientes> query = db.Expedientes;

                if (esDoctorRol && idDoctorSesion.HasValue)
                {
                    var idsPacientes = db.Citas
                        .Where(c => c.id_doctor == idDoctorSesion.Value)
                        .Select(c => c.id_paciente)
                        .Distinct();

                    query = query.Where(e => idsPacientes.Contains(e.id_paciente));
                }

                ViewBag.ExpedientesList = query
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
                    int? idUsuarioSesion = Session["Consecutivo"] as int?;

                    byte[] contenidoFinal = Encoding.UTF8.GetBytes(string.Empty);
                    string tipoMime = model.Tipo_mime;
                    long tamano = model.Tamano_bytes;
                    string rutaTemporal = null;

                    if (archivoSubido != null && archivoSubido.ContentLength > 0)
                    {
                        // Guardar con nombre temporal (GUID) para obtener path antes del INSERT
                        var ext = Path.GetExtension(archivoSubido.FileName);
                        var nombreTemp = Guid.NewGuid().ToString("N") + ext;
                        rutaTemporal = Path.Combine(CarpetaUploads, nombreTemp);
                        archivoSubido.SaveAs(rutaTemporal);

                        tipoMime = archivoSubido.ContentType;
                        tamano = archivoSubido.ContentLength;
                        contenidoFinal = Encoding.UTF8.GetBytes(nombreTemp);
                    }

                    var archivo = new Archivos
                    {
                        id_expediente = model.Id_Expediente,
                        id_usuario = idUsuarioSesion ?? model.Id_Usuario,
                        nombre = model.Nombre.Trim(),
                        tipo_mime = tipoMime,
                        tamano_bytes = tamano,
                        contenido = contenidoFinal,
                        estado = "ACTIVO",
                        fecha_carga = DateTime.Now
                    };

                    db.Archivos.Add(archivo);
                    db.SaveChanges();

                    // Renombrar archivo en disco con el id real
                    if (rutaTemporal != null && archivoSubido != null)
                    {
                        var ext = Path.GetExtension(archivoSubido.FileName);
                        var nombreFinal = string.Format("{0}_{1}{2}",
                            archivo.id_archivo,
                            Path.GetFileNameWithoutExtension(archivoSubido.FileName).Replace(" ", "_"),
                            ext);
                        var rutaFinal = Path.Combine(CarpetaUploads, nombreFinal);
                        System.IO.File.Move(rutaTemporal, rutaFinal);
                        archivo.contenido = Encoding.UTF8.GetBytes(nombreFinal);
                        db.SaveChanges();
                    }

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
        [AdminActionFilter]
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
        [AdminActionFilter]
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
                    var archivoBD = db.Archivos.Find(model.Id_Archivo);
                    if (archivoBD != null)
                    {
                        archivoBD.id_expediente = model.Id_Expediente;
                        archivoBD.nombre = model.Nombre.Trim();
                        archivoBD.tipo_mime = model.Tipo_mime;
                        archivoBD.estado = model.Estado;

                        if (archivoSubido != null && archivoSubido.ContentLength > 0)
                        {
                            EliminarArchivoDeDisco(archivoBD.contenido);

                            var ext = Path.GetExtension(archivoSubido.FileName);
                            var nombreFinal = string.Format("{0}_{1}{2}",
                                archivoBD.id_archivo,
                                Path.GetFileNameWithoutExtension(archivoSubido.FileName).Replace(" ", "_"),
                                ext);
                            archivoSubido.SaveAs(Path.Combine(CarpetaUploads, nombreFinal));
                            archivoBD.contenido = Encoding.UTF8.GetBytes(nombreFinal);
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

        // GET: Archivos/Descargar/5
        [HttpGet]
        public ActionResult Descargar(int? id)
        {
            if (!id.HasValue) return HttpNotFound();

            using (var db = new MediCoreEntities())
            {
                var archivo = db.Archivos.Find(id.Value);
                if (archivo == null || archivo.contenido == null || archivo.contenido.Length == 0)
                    return HttpNotFound();

                var nombreArchivo = Encoding.UTF8.GetString(archivo.contenido);
                if (string.IsNullOrWhiteSpace(nombreArchivo)
                    || nombreArchivo.IndexOfAny(Path.GetInvalidFileNameChars()) != -1
                    || nombreArchivo.Contains("/") || nombreArchivo.Contains("\\"))
                    return HttpNotFound();

                var rutaFisica = Path.GetFullPath(
                    Path.Combine(Server.MapPath("~/"), "..", "Uploads", "Archivos", nombreArchivo));
                if (!System.IO.File.Exists(rutaFisica))
                    return HttpNotFound();

                var bytes = System.IO.File.ReadAllBytes(rutaFisica);
                var mimeType = string.IsNullOrWhiteSpace(archivo.tipo_mime)
                    ? "application/octet-stream"
                    : archivo.tipo_mime;
                var nombreDescarga = archivo.nombre + Path.GetExtension(rutaFisica);

                return File(bytes, mimeType, nombreDescarga);
            }
        }

        // POST: Archivos/CambiarEstado
        [AdminActionFilter]
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

                bool tieneArchivo = false;
                if (archivo.contenido != null && archivo.contenido.Length > 0)
                {
                    try
                    {
                        var nombreArchivo = Encoding.UTF8.GetString(archivo.contenido);
                        // Solo es un nombre de archivo válido si no contiene caracteres de ruta ni caracteres de control
                        if (!string.IsNullOrWhiteSpace(nombreArchivo)
                            && nombreArchivo.IndexOfAny(Path.GetInvalidFileNameChars()) == -1
                            && !nombreArchivo.Contains("/") && !nombreArchivo.Contains("\\"))
                        {
                            var rutaFisica = Path.GetFullPath(
                                Path.Combine(Server.MapPath("~/"), "..", "Uploads", "Archivos", nombreArchivo));
                            tieneArchivo = System.IO.File.Exists(rutaFisica);
                        }
                    }
                    catch { tieneArchivo = false; }
                }

                var model = new ArchivoDetalleModel
                {
                    IdArchivo      = archivo.id_archivo,
                    Nombre         = archivo.nombre,
                    Estado         = archivo.estado,
                    TipoMime       = archivo.tipo_mime,
                    TamanioBytes   = archivo.tamano_bytes,
                    FechaCarga     = archivo.fecha_carga,
                    IdExpediente   = archivo.id_expediente,
                    NombrePaciente = archivo.Expedientes?.Pacientes?.nombre_completo,
                    IdUsuario      = archivo.id_usuario,
                    NombreUsuario  = archivo.tbUsuario?.Nombre,
                    TieneContenido = tieneArchivo
                };

                return View(model);
            }
        }
    }

}