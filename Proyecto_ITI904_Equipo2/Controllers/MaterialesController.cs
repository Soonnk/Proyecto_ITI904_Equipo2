﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_ITI904_Equipo2.Models;
using Proyecto_ITI904_Equipo2.Models.Inventario;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    public class MaterialesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Materiales
        public async Task<ActionResult> Index(int? mostrar)
        {
            var vista = db.Database.SqlQuery<Material>("Select * from Materials where Estatus = 1");

            if (mostrar == 1)
            {
                ViewBag.Ocultar = 0;
                return View(db.Materiales.ToList());
            }
            else
            {
                ViewBag.Ocultar = 1;
                return View(vista.ToList());
            }
            //return View(await db.Materiales.ToListAsync());
        }

        // GET: Materiales/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Material material = await db.Materiales.FindAsync(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }

        // GET: Materiales/Create
        public ActionResult Create(int? vistaProveedor)
        {
            /*Aquí le pasamos el id del proveedor desde el action link de mostrar materiales 
              del controlador de proveedores, de forma que le pasemos por medio de un viewBag
              a la vista de create el id del proveedor para no perderlo*/
            ViewBag.vistaProveedor = vistaProveedor;
            return View();
        }

        // POST: Materiales/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Nombre,Descripcion,Precio,Costo,DisponibleAPublico,Contenido,UnidadInventario, UnidadVenta")] Material material, int? vistaIdProveedor)
        {
            if (ModelState.IsValid)
            {
                if (vistaIdProveedor > 0 && vistaIdProveedor != null)
                {
                    material.Estatus = true;
                    db.Materiales.Add(material);
                    await db.SaveChangesAsync();

                    /*Creamos esta condicional de forma de que cuando se use este método desde materiales
                      sin usarlo desde proveedores*/
                    return RedirectToAction("MostrarMateriales", "Proveedores", new { idProv = vistaIdProveedor });
                }
                else
                {
                    material.Estatus = true;
                    db.Materiales.Add(material);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return View(material);
        }

        // GET: Materiales/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Material material = await db.Materiales.FindAsync(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }

        // POST: Materiales/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Nombre,Descripcion,Precio,Costo,DisponibleAPublico,Existencia")] Material material)
        {
            if (ModelState.IsValid)
            {
                material.Estatus = true;
                db.Entry(material).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(material);
        }

        // GET: Materiales/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Material material = await db.Materiales.FindAsync(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }

        // POST: Materiales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Material material = await db.Materiales.FindAsync(id);

            if (ModelState.IsValid)
            {
                material.Estatus = false;
                db.Entry(material).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(material);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
