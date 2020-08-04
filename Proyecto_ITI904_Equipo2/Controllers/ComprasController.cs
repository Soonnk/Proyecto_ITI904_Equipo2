using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Proyecto_ITI904_Equipo2.Models;
using Proyecto_ITI904_Equipo2.Models.Compras;
using Proyecto_ITI904_Equipo2.Models.Inventario;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    public class ComprasController : Controller
    {
        public List<Material> Materiales;
        public List<int> ListaMateriales;
        public List<int> Cantidades;
        public double subTotal = 0;
        public double totalCompra = 0;
        private ApplicationDbContext db = new ApplicationDbContext();
        Proveedor proveedor = null;
        Compra compra = null;


        #region Metodos generados por default
        // GET: Compras/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Compra compra = db.Compras.Find(id);
            if (compra == null)
            {
                return HttpNotFound();
            }
            return View(compra);
        }

        // GET: Compras/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Compras/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Recibida,FechaSolicitud,FechaRecepción")] Compra compra)
        {
            if (ModelState.IsValid)
            {
                db.Compras.Add(compra);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(compra);
        }

        // GET: Compras/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Compra compra = db.Compras.Find(id);
            if (compra == null)
            {
                return HttpNotFound();
            }
            return View(compra);
        }

        // POST: Compras/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Recibida,FechaSolicitud,FechaRecepción")] Compra compra)
        {
            if (ModelState.IsValid)
            {
                db.Entry(compra).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(compra);
        }

        // GET: Compras/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Compra compra = db.Compras.Find(id);
            if (compra == null)
            {
                return HttpNotFound();
            }
            return View(compra);
        }

        // POST: Compras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Compra compra = db.Compras.Find(id);
            db.Compras.Remove(compra);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion





        #region Métodos de vistas
        public ActionResult Index()
        { // Regresa los proveedores actuales en el sistema
            return View("Index", db.Proveedores.ToList());
        }

        public ActionResult MostrarProveedores()
        {
            return PartialView("_MostrarProveedores", db.Proveedores.ToList());
        }

        public ActionResult MostrarCompras()
        {
            return View("_MostrarCompras", db.Compras.ToList());
        }

        public ActionResult MostrarCarritoCompras()
        {
            ViewBag.TotalCompras = Session["TotalCompra"];

            var cantidades = Session["Cantidades"] as List<int>;
            ViewBag.Cantidades = cantidades;

            var materialesId = Session["IdMaterialesAgregados"] as List<int>;
            var materialesSeleccionados = db.Materiales.Where(x => materialesId.Contains(x.Id));
            Session["MaterialesAgregados"] = materialesSeleccionados; // Guardamos la lista de los materiales para usarlos al momento de crear los detalles de compra
            return View("_MostrarCarritoCompras", materialesSeleccionados.ToList());
        }
        #endregion


        #region Funcionalidad de Nueva Compra
        public ActionResult MostrarProveedoresMateriales(int? id, int? idM, int? cantidad, string btnOpcion)
        {
            if (id == null)
            {
                ListaMateriales = Session["IdMaterialesAgregados"] as List<int>;
                Cantidades = Session["Cantidades"] as List<int>;
                //Materiales = Session["Materiales"] as List<Material>;
                if (ListaMateriales != null && Cantidades != null && Materiales != null)
                {
                    ViewBag.Total = CalcularTotal();
                    ViewBag.Agregados = Session["IdMaterialesAgregados"];
                    return HttpNotFound();
                }
                return HttpNotFound();
            }
            else
            {
                if (cantidad == null && idM == null && btnOpcion == null)
                { // Visualización
                    var listaProvMate = db.Database.SqlQuery<ProveedorMaterials>("Select * from ProveedorMaterials where Proveedor_Id =" + id + "");
                    // Condicional 2.1 Verifica si la consulta regresó Ids o no
                    if (listaProvMate.Count() == 0)
                    { // Regresa la vista cuando no tienen datos
                        return PartialView("_MostrarProveedoresMateriales");
                    }
                    else
                    { // Regresa los materiales que tenga el proveedor
                        int[] Materiales = new int[listaProvMate.Count()]; // Arreglo que guardará los Id
                        int val = 0;

                        foreach (var item in listaProvMate)
                        { // Guardar los ID de materiales del proveedor
                            Materiales[val] = item.Material_Id;
                            val++;
                        }

                        Session["IdProveedor"] = id;
                        ViewBag.idProveedor = Session["IdProveedor"];
                        ViewBag.Agregados = Session["IdMaterialesAgregados"];
                        /*  Los TempData (Session) Guardan los datos por siempre que dure el programa, pero una vez que se leen (no importa donde mientras siga la secuencia)
                            ya no contendrán nada, por lo que solo debo llamarlos una vez en este método para guardarlo en el hidden de la vista
                            De forma que obtenga los datos para comparar si el material ha sido agregado o no
                         */
                        var listaMate = db.Materiales.Where(x => Materiales.Contains(x.Id));
                        return PartialView("_MostrarProveedoresMateriales", listaMate.ToList());
                    }
                }
                else
                { // Inserción Session

                    if ((idM == null || cantidad == null || cantidad <= 0) && btnOpcion == "Agregar")
                    {
                        ViewBag.Total = CalcularTotal();
                        ViewBag.Agregados = Session["IdMaterialesAgregados"];
                        return RedirectToAction("Index");
                    }
                    else if (idM != null && cantidad == -1 && btnOpcion == "Quitar")
                    {
                        QuitarDeLista(idM);
                        ViewBag.Total = CalcularTotal();
                        ViewBag.Agregados = Session["IdMaterialesAgregados"];
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var materialSeleccionado = db.Materiales.Find(idM);
                        if (materialSeleccionado?.Existencia < cantidad)
                        {
                            ViewBag.Total = CalcularTotal();
                            ViewBag.Agregados = Session["IdMaterialesAgregados"];
                            return RedirectToAction("Index");
                        }
                        Materiales = Session["Materiales"] as List<Material>;
                        if (Materiales == null)
                        {
                            Materiales = new List<Material>();
                        }
                        Materiales.Add(materialSeleccionado);
                        Session["Materiales"] = Materiales;

                        AgregarMaterialALista(idM, cantidad);
                        Session["TotalCompra"] = CalcularTotal();

                        return RedirectToAction("Index", "Compras");
                    }
                }
            }
        }

        public double CalcularTotal()
        {
            List<int> ids = Session["IdMaterialesAgregados"] as List<int>;
            if (ids == null)
                return subTotal;
            subTotal = 0;
            Cantidades = Session["Cantidades"] as List<int>;
            for (int i = 0; i < ids.Count; i++)
            {
                var material = db.Materiales.Find(ids[i]);
                subTotal += (material.Precio * Cantidades[i]);
            }
            return subTotal;
        }

        public void AgregarMaterialALista(int? idM, int? cantidad)
        {
            Material material = db.Materiales.Find(idM);
            if (material != null)
            {
                if (Session["IdMaterialesAgregados"] == null)
                {
                    ListaMateriales = new List<int>();
                    Cantidades = new List<int>();
                }
                else
                {
                    ListaMateriales = Session["IdMaterialesAgregados"] as List<int>;
                    Cantidades = Session["Cantidades"] as List<int>;
                }
                ListaMateriales.Add(material.Id);
                Cantidades.Add(Convert.ToInt32(cantidad));
                Session["IdMaterialesAgregados"] = ListaMateriales;
                Session["Cantidades"] = Cantidades;

            }
        }

        public void QuitarDeLista(int? idM)
        {
            Material material = db.Materiales.Find(idM);
            if (material != null)
            {
                ListaMateriales = Session["IdMaterialesAgregados"] as List<int>;
                Cantidades = Session["Cantidades"] as List<int>;
                var index = ListaMateriales.IndexOf(Convert.ToInt32(idM));

                ListaMateriales.RemoveAt(index);
                Cantidades.RemoveAt(index);
                ListaMateriales.RemoveAll(cadena => string.IsNullOrEmpty(Convert.ToString(cadena)));
                Cantidades.RemoveAll(cadena => string.IsNullOrEmpty(Convert.ToString(cadena)));
            }
        }



        public ActionResult AgregarNuevaCompra()
        {
            //var idProveedor = Convert.ToInt32(Session["IdProveedor"].ToString());

            //db.Database.ExecuteSqlCommand("Insert into Compras " +
            //                                   //"(Recibida, FechaSolicitud, FechaRecepción, Encargado_Id, Proveedor_Id)" +
            //                                   "values (" + false + ", " + DateTime.Now + ", " + DateTime.Now + ", " + null + ", " + idProveedor + " )");
            //db.SaveChanges();

            //var idCompra = db.Compras.Max(x => x.Id); // Obtener el ID del último dato insertado

            /*No usé EF porque al momento de poner el Id del proveedor lo mandaba a la base de datos
             Como una inserción de proveedores, lo cual hacía que solo me llenara espacios vacíos */

            //Proveedor proveedor = new Proveedor();
            //proveedor.Id = idProveedor;

            //Compra compra = new Compra();
            //compra.Recibida = false;
            //compra.FechaRecepción = DateTime.Now;
            //compra.FechaSolicitud = DateTime.Now;
            //compra.Encargado = null;
            //compra.Proveedor = proveedor;

            //db.Compras.Add(compra);
            //db.SaveChanges();

            //var idCompra = compra.Id;

            //var Materiales = Session["Materiales"] as List<Material>;
            //var Cantidades = Session["Cantidades"] as List<int>;

            //DetalleCompra detalleCompra = null;

            //for (int i = 0; i < Materiales.Count; i++) {
            //    //detalleCompra = new DetalleCompra();
            //    //detalleCompra.Cantidad = Cantidades[i];
            //    //detalleCompra.Costo = Materiales[i].Costo;
            //    //detalleCompra.Material = Materiales[i].Id;
            //    //detalleCompra.c
            //    db.Database.ExecuteSqlCommand("Insert into DetalleCompras " +
            //                                    "values " +
            //                                    "(@cantidad, @costo, @MaterialId, @CompraId) ",
            //                                                        new SqlParameter("@cantidad", ),
            //                                                        new SqlParameter("@costo", Materiales[i].Costo),
            //                                                        new SqlParameter("@MaterialId", Materiales[i].Id),
            //                                                        new SqlParameter("@CompraId", idCompra));
            //    db.Database.ExecuteSqlCommand("UPDATE Materials SET " +
            //        "Existencia = " + (Materiales[i].Existencia - Cantidades[i]) + "" +
            //        "where Id = " + Materiales[i].Id + "");
                    
            //}
            //db.SaveChanges();
            //Session["Cantidades"] = null;
            //Session["Materiales"] = null;
            //Session["IdMaterialesAgregados"] = null;

            return RedirectToAction("Index", "Compras"); 
        }
        #endregion

    }
}