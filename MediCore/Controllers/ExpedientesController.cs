using MediCore.EF;
using MediCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace MediCore.Controllers
{
    public class ExpedientesController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Expedientes";
            return View();
        }
    }
}
