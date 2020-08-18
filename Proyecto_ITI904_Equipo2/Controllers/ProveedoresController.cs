using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        
        public ActionResult Index(int? mostrar)
        {
            var vista = db.Database.SqlQuery<Proveedor>("Select * from Proveedors where Estatus = 1");

            if (mostrar == 1)
            {
                ViewBag.Ocultar = 0;
                return View(db.Proveedores.ToList());
            }
            else
            {
                ViewBag.Ocultar = 1;
                return View(vista.ToList());
            }
        }

        
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

        
        public ActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                if (Request.Files.Count > 0)
                { // Condicional que verifica si el usuario ingresó imagen del proveedor
                    var file = Request.Files[0];

                    byte[] bytes = new byte[file.ContentLength];
                    var i = file.InputStream.Read(bytes, 0, file.ContentLength);

                    proveedor.ImageUrl = Convert.ToBase64String(bytes);
                    proveedor.Estatus = true;
                    db.Proveedores.Add(proveedor);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Proveedores", new { proveedor.Id });
                }
                else
                { // Esto es en caso de que no seleccione alguna imagen
                    proveedor.ImageUrl = "~/images/defaultProveedor.png";
                    db.Proveedores.Add(proveedor);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Proveedores", new { proveedor.Id });
                }
            }

            return View(proveedor);
        }

        
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
        public ActionResult Edit(Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                if (proveedor.ImageUrl == null)
                {
                    if (Request.Files.Count > 0)
                    { // Condicional que verifica si el usuario ingresó imagen del proveedor
                        var file = Request.Files[0];

                        byte[] bytes = new byte[file.ContentLength];
                        var i = file.InputStream.Read(bytes, 0, file.ContentLength);

                        proveedor.ImageUrl = Convert.ToBase64String(bytes);
                        proveedor.Estatus = true;
                        db.Entry(proveedor).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    db.Entry(proveedor).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(proveedor);
        }

        
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string nota)
        {
            Proveedor proveedor = db.Proveedores.Find(id);

            if (ModelState.IsValid)
            {
                proveedor.Estatus = false;
                proveedor.Nota += nota;
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





        /*Métodos para mostrar los materiales del proveedor e ingresar nuevos materiales que venda*/
        public ActionResult MostrarProveedoresMateriales(int? id, int? det)
        {
            if (id != null)
            {
                ViewBag.idProveedor = id;
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
                    return PartialView("_MostrarProveedoresMateriales"); // No funciona 
                }
                else
                { // Regresa los materiales que tenga el proveedor
                    var listaMate = db.Materiales.Where(x => idMaterial.Contains(x.Id));

                    if (det == null || det == 0)
                    {
                        return PartialView("_MostrarProveedoresMateriales", listaMate.ToList());
                    }
                    else
                    {
                        /* La variable det nos ayudará a restringirle al programa cuando puede eliminar los
                         materiales del proveedor y cuando no*/
                        ViewBag.Detalle = det;
                        return PartialView("_MostrarProveedoresMateriales", listaMate.ToList());
                    }
                }
            }
            else
            {
                return View();
            }
        }

        public ActionResult MostrarMateriales(int idProv)
        { // Regresamos la lista entera de los materiales y guardamos en un ViewBag el Id del proveedor actual, de modo que tengamos su Id siempre
            ViewBag.idProveedor = idProv;
            var vista = db.Database.SqlQuery<Material>("Select * from Materials where EnUso = 0");
            return View("_MostrarMateriales", vista.ToList());
        }

        public ActionResult InsertarProveedoresMateriales(int idP, int idM)
        {
            // Esta opción si funciona :D
            db.Database.ExecuteSqlCommand("Insert into ProveedorMaterials values (@idProveedor, @idMateriales) ",
                                                                    new SqlParameter("@idProveedor", idP),
                                                                    new SqlParameter("@idMateriales", idM));
            db.SaveChanges();

            db.Database.ExecuteSqlCommand("Update Materials set EnUso = 1 where Id = " + idM);
            db.SaveChanges();

            Proveedor proveedor = db.Proveedores.Find(idP); // Arreglo que guardará los Id

            return View("Edit", proveedor);
            //return View("Index", db.Proveedores.ToList()); // Regresa al menú principal de proveedores
        }

        public ActionResult QuitarMaterialProveedor(int idProveedor, int idMaterial)
        {
            db.Database.ExecuteSqlCommand("delete from ProveedorMaterials where Proveedor_Id = @idProveedor and Material_Id = @idMaterial ",
                                                                    new SqlParameter("@idProveedor", idProveedor),
                                                                    new SqlParameter("@idMaterial", idMaterial));
            db.SaveChanges();

            db.Database.ExecuteSqlCommand("Update Materials set EnUso = 0 where Id = " + idMaterial);
            db.SaveChanges();

            Proveedor proveedor = db.Proveedores.Find(idProveedor); // Arreglo que guardará los Id


            return View("Edit", proveedor);
        }

        public ActionResult MoverAControladorMateriales(int? idProveedor)
        {
            /* Método para redirigir de proveedores a materiales sin tener que hacer la vista y
               un controlador de nuevo, le paso el idProveedor para que cuando regrese a la vista
               de editar de proveedores pueda mostrar la vista que tenía desde un inicio*/
            @ViewBag.vistaProveedor = idProveedor;
            /*Pasamos el id al método get de Create*/
            return RedirectToAction("Create", "Materiales", new { vistaProveedor = idProveedor });
        }
    }
}
