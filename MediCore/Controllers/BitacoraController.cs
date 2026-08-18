using MediCore.EF;
using MediCore.Models;
using MediCore.Servicios;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MediCore.Controllers
{

    [AuthActionFilter]
    [AdminActionFilter]
    public class BitacoraController : Controller
    {
        private const string NombreControlador = "Bitacora";
        private const int TamanoPagina = 20;

        private readonly UtilitarioService _utilitario = new UtilitarioService();

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

                    if (!string.IsNullOrWhiteSpace(nivel))
                    {
                        consulta = consulta.Where(b => b.nivel == nivel);
                    }

                    if (!string.IsNullOrWhiteSpace(controlador))
                    {
                        consulta = consulta.Where(b => b.controlador.Contains(controlador));
                    }

                    if (!string.IsNullOrWhiteSpace(usuario))
                    {
                        consulta = consulta.Where(b =>
                            b.tbUsuario != null && b.tbUsuario.Nombre.Contains(usuario));
                    }

                    if (fechaDesde.HasValue)
                    {
                        consulta = consulta.Where(b =>
                            DbFunctions.TruncateTime(b.fecha) >=
                            DbFunctions.TruncateTime(fechaDesde.Value));
                    }

                    if (fechaHasta.HasValue)
                    {
                        consulta = consulta.Where(b =>
                            DbFunctions.TruncateTime(b.fecha) <=
                            DbFunctions.TruncateTime(fechaHasta.Value));
                    }

                    int totalRegistros = consulta.Count();
                    int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanoPagina);

                    page = Math.Max(1, Math.Min(page, Math.Max(1, totalPaginas)));

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