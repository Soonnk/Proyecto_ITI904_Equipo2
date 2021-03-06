﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Empleado"))
                return RedirectToAction("Index", "Compras");
            return View();
        }

        public ActionResult UnderConstruction()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}