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
        public List<int> Costos;
        public double subTotal = 0;
        public double totalCompra = 0;
        private ApplicationDbContext db = new ApplicationDbContext();
        Proveedor proveedor = null;
        Compra compra = null;


        #region Metodos generados por default
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Recibida,FechaSolicitud,FechaRecepción,Proveedor_Id")] Compra compra)
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
            return RedirectToAction("MostrarCompras");
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
            return View("Index");
        }

        public ActionResult MostrarProveedores()
        {
            var vista = db.Database.SqlQuery<Proveedor>("Select * from Proveedors where Estatus = 1");

            return PartialView("_MostrarProveedores", vista.ToList());
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
        public ActionResult MostrarProveedoresMateriales(int? id, int? idM, int? cantidad, string btnOpcion, int? costoMaterial)
        {
            if (Session["IdProveedor"] == null && id == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (cantidad == null && idM == null && costoMaterial == null)
                { // Visualización
                    
                    var listaProvMate = db.Database.SqlQuery<ProveedorMaterials>("Select * from ProveedorMaterials where Proveedor_Id = " + id + "");
                    // Condicional 2.1 Verifica si la consulta regresó Ids o no
                    if (listaProvMate.Count() == 0)
                    { // Regresa la vista cuando no tienen datos
                        return View("_MostrarProveedoresMateriales");
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
                        ViewBag.Agregados = Session["IdMaterialesAgregados"]; // Se usa para decirle a la vista que datos están en el carrito ya agregados, si no hay ninguno
                        // entonces se hace nulo

                        /*  Los TempData (Session) Guardan los datos por siempre que dure el programa, pero una vez que se leen (no importa donde mientras siga la secuencia)
                            ya no contendrán nada, por lo que solo debo llamarlos una vez en este método para guardarlo en el hidden de la vista
                            De forma que obtenga los datos para comparar si el material ha sido agregado o no
                         */
                        var listaMate = db.Materiales.Where(x => Materiales.Contains(x.Id));
                        return View("_MostrarProveedoresMateriales", listaMate.ToList());
                    }
                }
                else
                { // Inserción Session

                    if ((idM == null || cantidad == null || cantidad <= 0) && btnOpcion == "Agregar")
                    {
                        ViewBag.Total = CalcularTotal(costoMaterial);
                        ViewBag.Agregados = Session["IdMaterialesAgregados"];
                        return RedirectToAction("Index");
                    }
                    else if (idM != null && cantidad == -1 && btnOpcion == "Quitar")
                    {
                        QuitarDeLista(idM);
                        ViewBag.Total = CalcularTotal(costoMaterial);
                        ViewBag.Agregados = Session["IdMaterialesAgregados"];
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Materiales = Session["Materiales"] as List<Material>;

                        if (Materiales == null)
                        {
                            Materiales = new List<Material>();
                        }

                        var materialSeleccionado = db.Materiales.Find(idM);
                        //materialSeleccionado.Costo = Convert.ToDouble(costoMaterial);
                        Materiales.Add(materialSeleccionado);
                        Session["Materiales"] = Materiales;

                        AgregarMaterialALista(idM, cantidad);
                        Session["TotalCompra"] = CalcularTotal(costoMaterial);


                        /*Parte para pasarle los datos a la vista*/ /*Hacerlo metodo*/ 
                        ViewBag.idProveedor = id;

                        var listaProvMate = db.Database.SqlQuery<ProveedorMaterials>("Select * from ProveedorMaterials where Proveedor_Id = " + id + "");

                        int[] MaterialesLista = new int[listaProvMate.Count()]; // Arreglo que guardará los Id
                        int val = 0;

                        foreach (var item in listaProvMate)
                        { // Guardar los ID de materiales del proveedor
                            MaterialesLista[val] = item.Material_Id;
                            val++;
                        }

                        ViewBag.Agregados = Session["IdMaterialesAgregados"];

                        var listaMate = db.Materiales.Where(x => MaterialesLista.Contains(x.Id));
                        return View("_MostrarProveedoresMateriales", listaMate.ToList());
                    }
                }
            }
        }

        public double CalcularTotal(int? costo)
        {
            List<int> ids = Session["IdMaterialesAgregados"] as List<int>;
            if (ids == null)
                return subTotal;
            subTotal = 0;
            Cantidades = Session["Cantidades"] as List<int>;
            for (int i = 0; i < ids.Count; i++)
            {
                var material = db.Materiales.Find(ids[i]);
                subTotal += (Convert.ToInt32(costo) * Cantidades[i]);
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
            var idProveedor = Convert.ToInt32(Session["IdProveedor"].ToString());

            db.Database.ExecuteSqlCommand("Insert into Compras " +
                                                "(Recibida, FechaSolicitud, FechaRecepción, Proveedor_Id)" +
                                                "values (@Recibida, @FechaSolicitud, @FechaRecepción, @Proveedor_Id)",
                                                new SqlParameter("@Recibida", false),
                                                new SqlParameter("@FechaSolicitud", DateTime.Now),
                                                new SqlParameter("@FechaRecepción", DateTime.Now),
                                                new SqlParameter("@Proveedor_Id", idProveedor));
            db.SaveChanges();

            var idCompra = db.Compras.Max(x => x.Id); // Obtener el ID del último dato insertado

            var Materiales = Session["Materiales"] as List<Material>;
            var Cantidades = Session["Cantidades"] as List<int>;

            for (int i = 0; i < Materiales.Count; i++)
            {
                db.Database.ExecuteSqlCommand("Insert into DetalleCompras " +
                                                "values " +
                                                "(@cantidad, @costo, @MaterialId, @CompraId) ",
                                                                    new SqlParameter("@cantidad", Cantidades[i]),
                                                                    new SqlParameter("@costo", Materiales[i].Costo),
                                                                    new SqlParameter("@MaterialId", Materiales[i].Id),
                                                                    new SqlParameter("@CompraId", idCompra));
                db.Database.ExecuteSqlCommand("UPDATE Materials SET " +
                    "Existencia = " + (Materiales[i].Existencia + (Cantidades[i] * Convert.ToInt32(Materiales[i].Contenido))) + "" +
                    "where Id = " + Materiales[i].Id + "");
                db.SaveChanges();
            }
            //db.SaveChanges();

            Session["Cantidades"] = null;
            Session["Materiales"] = null;
            Session["IdMaterialesAgregados"] = null;

            return RedirectToAction("Index", "Compras"); 
        }
        #endregion

    }
}