using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    /// <summary>
    /// Módulo de auditoría: permite a los administradores consultar el historial
    /// de eventos y errores registrados en el sistema.
    /// </summary>
    [AuthActionFilter]
    [AdminActionFilter]
    public class BitacoraController : Controller
    {
        private const string NombreControlador = "Bitacora";
        private const int TamanoPagina = 20;

        private readonly UtilitarioService _utilitario = new UtilitarioService();

        // GET: Bitacora
        public ActionResult Index(
            string nivel,
            string controlador,
            string usuario,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int page = 1)
        {
            ViewBag.ActiveMenu = "Bitacora";

            try
            {
                using (var db = new MediCoreEntities())
                {
                    var consulta = db.Bitacora
                        .Include(b => b.tbUsuario)
                        .AsQueryable();

                    // Filtro por nivel (INFO / ERROR)
                    if (!string.IsNullOrWhiteSpace(nivel))
                    {
                        consulta = consulta.Where(b => b.nivel == nivel);
                    }

                    // Filtro por controlador
                    if (!string.IsNullOrWhiteSpace(controlador))
                    {
                        consulta = consulta.Where(b => b.controlador.Contains(controlador));
                    }

                    // Filtro por nombre de usuario
                    if (!string.IsNullOrWhiteSpace(usuario))
                    {
                        consulta = consulta.Where(b =>
                            b.tbUsuario != null && b.tbUsuario.Nombre.Contains(usuario));
                    }

                    // Filtro por fecha desde
                    if (fechaDesde.HasValue)
                    {
                        consulta = consulta.Where(b =>
                            DbFunctions.TruncateTime(b.fecha) >=
                            DbFunctions.TruncateTime(fechaDesde.Value));
                    }

                    // Filtro por fecha hasta
                    if (fechaHasta.HasValue)
                    {
                        consulta = consulta.Where(b =>
                            DbFunctions.TruncateTime(b.fecha) <=
                            DbFunctions.TruncateTime(fechaHasta.Value));
                    }

                    // Total de registros para la paginación
                    int totalRegistros = consulta.Count();
                    int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanoPagina);

                    // Garantizar que la página esté dentro del rango válido
                    page = Math.Max(1, Math.Min(page, Math.Max(1, totalPaginas)));

                    // Proyección al DTO
                    var registros = consulta
                        .OrderByDescending(b => b.fecha)
                        .Skip((page - 1) * TamanoPagina)
                        .Take(TamanoPagina)
                        .Select(b => new BitacoraListaModel
                        {
                            IdBitacora   = b.id_bitacora,
                            Fecha        = b.fecha,
                            Nivel        = b.nivel,
                            NombreUsuario = b.tbUsuario != null ? b.tbUsuario.Nombre : null,
                            Controlador  = b.controlador,
                            Accion       = b.accion,
                            Mensaje      = b.mensaje,
                            StackTrace   = b.stack_trace,
                            IpOrigen     = b.ip_origen
                        })
                        .ToList();

                    // Pasar filtros activos a la vista para repoblar el formulario
                    ViewBag.FiltroNivel        = nivel;
                    ViewBag.FiltroControlador  = controlador;
                    ViewBag.FiltroUsuario      = usuario;
                    ViewBag.FiltroFechaDesde   = fechaDesde;
                    ViewBag.FiltroFechaHasta   = fechaHasta;
                    ViewBag.PaginaActual       = page;
                    ViewBag.TotalPaginas       = totalPaginas;
                    ViewBag.TotalRegistros     = totalRegistros;

                    return View(registros);
                }
            }
            catch (Exception ex)
            {
                _utilitario.RegistrarErrorBitacora(ex, NombreControlador, "Index");
                TempData["Error"] = "Ocurrió un error al cargar la bitácora. Intente de nuevo.";
                return View(Enumerable.Empty<BitacoraListaModel>());
            }
        }
    }
}
