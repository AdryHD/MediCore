using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MediCore.Controllers
{
    public class HomeController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        // GET: Registro
        public ActionResult Registro()
        {
            return View();
        }

        // GET: Recuperar Acceso
        public ActionResult RecuperarAcceso()
        {
            return View();
        }

        // GET: Panel principal (requiere autenticación)
        public ActionResult Principal()
        {
            return View();
        }
    }
}