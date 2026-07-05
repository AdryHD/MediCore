using MediCore.EF;
using MediCore.Models;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string correo, string contrasena)
        {
            return RedirectToAction("Principal");
        }

        [HttpGet]
        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registro(UsuarioModel model)
        {
            using (var db = new MediCoreEntities())
            {
                db.sp_RegistrarUsuario(model.Nombre, model.Cedula, model.FechaNacimiento, model.Telefono, model.Correo, model.Contrasenna);
                return RedirectToAction("Index", "Home");
            }
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

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            // Cerrar sesión y redirigir al login
            return RedirectToAction("Index");
        }
    }
}