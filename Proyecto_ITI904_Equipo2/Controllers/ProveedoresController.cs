﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_ITI904_Equipo2.Models;
using Proyecto_ITI904_Equipo2.Models.Compras;
using Proyecto_ITI904_Equipo2.Models.Inventario;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    public class ProveedoresController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Proveedores
        public ActionResult Index(int? todos)
        {
            var vista = db.Database.SqlQuery<Proveedor>("Select * from Proveedors where Estatus = 1");

            if (todos == 1)
            {
                return View(db.Proveedores.ToList());
            }
            else
            {
                return View(vista.ToList());
            }
        }

        // GET: Proveedores/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proveedor proveedor = db.Proveedores.Find(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }
            return View(proveedor);
        }

        // GET: Proveedores/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,RFC,Telefono,Direccion,ImageUrl,Estatus,Nota")] Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                db.Proveedores.Add(proveedor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(proveedor);
        }

        // GET: Proveedores/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proveedor proveedor = db.Proveedores.Find(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }
            return View(proveedor);
        }

        // POST: Proveedores/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,RFC,Telefono,Direccion,ImageUrl,Estatus,Nota")] Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(proveedor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        // GET: Proveedores/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proveedor proveedor = db.Proveedores.Find(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }
            return View(proveedor);
        }

        // POST: Proveedores/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Proveedor proveedor = db.Proveedores.Find(id);
        //    db.Proveedores.Remove(proveedor);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}


        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Proveedor proveedor = db.Proveedores.Find(id);

            if (ModelState.IsValid)
            {
                proveedor.Estatus = false;
                db.Entry(proveedor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        public ActionResult MostrarProveedoresMateriales(int? id)
        {
            if (id != null)
            {
                var listaProvMate = db.Database.SqlQuery<ProveedorMaterials>("Select * from ProveedorMaterials where Proveedor_Id = " + id + "");
                int[] idMaterial = new int[listaProvMate.Count()]; // Arreglo que guardará los Id
                int val = 0;

                foreach (var item in listaProvMate)
                { // Guardar los ID de materiales del proveedor
                    idMaterial[val] = item.Material_Id;
                    val++;
                }

                if (listaProvMate.Count() <= 0)
                { // Regresa la vista cuando no tienen datos
                    return View();
                }
                else
                { // Regresa dos o más materiales que tenga el proveedor
                  //  var listaMate = db.Database.SqlQuery<Material>("Select * from Materials where Id in @Ids", new SqlParameter("@Ids", idMaterial));
                    var listaMate = db.Materiales.Where(x => idMaterial.Contains(x.Id));

                    //return View(listaMate.ToList());
                    return PartialView("MostrarProveedoresMateriales", listaMate.ToList());
                }
            }
            else
            {
                return View();
            }
            
            //var listaMateriales = System.Data.Entity.Infrastructure.DbRawSqlQuery<Material>();
            //for (int i = 0; i < idProveedor.Count(); i++)
            //{
            //    listaMateriales += db.Database.SqlQuery<Proveedor>("Select * from Materials where Id =  @Id", new SqlParameter("@Id", idProveedor[i]));
            //}
        }
    }
}