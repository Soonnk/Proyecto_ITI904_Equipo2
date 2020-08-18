using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Proyecto_ITI904_Equipo2.Models;
using Proyecto_ITI904_Equipo2.Models.Compras;
using Proyecto_ITI904_Equipo2.Models.Inventario;
using Proyecto_ITI904_Equipo2.Views.Compras.ReportesCompras;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    [Authorize(Roles = "Empleado,Admin")]
    public class ComprasController : Controller
    {
        public List<Material> Materiales;
        public List<int> ListaMateriales;
        public List<int> Cantidades;
        public List<double> Costos;
        public double subTotal = 0;
        public double totalCompra = 0;
        private ApplicationDbContext db = new ApplicationDbContext();


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
        public ActionResult Edit([Bind(Include = "Id,Recibida,FechaSolicitud,FechaRecepción,Proveedor_Id,Encargado_Id")] Compra compra, DateTime? FechaSolicitada)
        {
            if (ModelState.IsValid)
            {
                if (compra.Recibida == true)
                {
                    var vistaDetalleCompras = db.Database.SqlQuery<DetalleCompra>("Select * from DetalleCompras where Compra_Id = " + compra.Id);
                    var modeloDetalleCompras = vistaDetalleCompras.ToList();
                    List<Material> modeloMateriales = new List<Material>();
                    for (int i = 0; i < modeloDetalleCompras.Count; i++) {
                        var vistaMateriales = db.Database.SqlQuery<Material>("Select * from Materials where Id = " + modeloDetalleCompras[i].Material_Id);
                        modeloMateriales.Add(vistaMateriales.ToList()[0]);
                    }

                    for (int i = 0; i < modeloMateriales.Count; i++)
                    {
                        db.Database.ExecuteSqlCommand("UPDATE Materials SET " +
                        "Existencia = " + (modeloMateriales[i].Existencia + (modeloDetalleCompras[i].Cantidad * Convert.ToDouble(modeloMateriales[i].Contenido))) + "" +
                        "where Id = " + modeloMateriales[i].Id + "");
                        db.SaveChanges();
                    }

                    compra.FechaRecepción = Convert.ToDateTime(FechaSolicitada);
                    compra.Estatus = true;
                    db.Entry(compra).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("MostrarCompras");
                }
                else
                {
                    db.Entry(compra).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("MostrarCompras");
                }   
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

            if (ModelState.IsValid)
            {
                compra.Estatus = false;;
                db.Entry(compra).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MostrarCompras", new { mostrar = 0});
            }
            return View(compra);
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

        public ActionResult MostrarCompras(int? mostrar)
        {
            var comprasRealizadas = db.Database.SqlQuery<Compra>("Select * from Compras where Recibida = 1 and Estatus = 1");
            var comprasNoRealizadas = db.Database.SqlQuery<Compra>("Select * from Compras where Recibida = 0 and Estatus = 1");

            if (mostrar == 1)
            {
                ViewBag.Ocultar = 0;
                return View("_MostrarCompras", comprasNoRealizadas.ToList());
            }
            else
            {
                ViewBag.Ocultar = 1;
                return View("_MostrarCompras", comprasRealizadas.ToList());
            }

            
        }

        public ActionResult MostrarCarritoCompras(int? id)
        {
            if (Session["IdMaterialesAgregados"] == null && Session["TotalCompra"] == null &&
                Session["MaterialesAgregados"] == null)
            {
                return View("Index");
            }
            else
            {
                ViewBag.idProveedor = id;

                var costos = Session["Costos"] as List<double>;
                ViewBag.Costos = costos;

                var cantidades = Session["Cantidades"] as List<int>;
                ViewBag.Cantidades = cantidades;

                ViewBag.TotalCompras = Session["TotalCompra"];

                var materialesId = Session["IdMaterialesAgregados"] as List<int>;
                var materialesSeleccionados = db.Materiales.Where(x => materialesId.Contains(x.Id));
                Session["MaterialesAgregados"] = materialesSeleccionados; // Guardamos la lista de los materiales para usarlos al momento de crear los detalles de compra
                return View("_MostrarCarritoCompras", materialesSeleccionados.ToList());
            }
        }

        public ActionResult MostrarDetCompra(int? id)
        {
            var listaDetCompra = db.Database.SqlQuery<DetalleCompra>("Select * from DetalleCompras where Compra_Id = " + id + "");
            var modeloListaDetCompra = listaDetCompra.ToList();
            double total = 0;
            List<double> importe = new List<double>();
            for (int i = 0; i < modeloListaDetCompra.Count; i++)
            {
                var listaMaterial = db.Database.SqlQuery<Material>("Select * from Materials where Id = " + modeloListaDetCompra[i].Material_Id);
                modeloListaDetCompra[i].Material = listaMaterial.ToList()[0];
                importe.Add(modeloListaDetCompra[i].Costo * modeloListaDetCompra[i].Cantidad);
                total += importe[i];
            }
            ViewBag.Importe = importe;
            ViewBag.Total = total;
            return PartialView("_MostrarDetCompra", modeloListaDetCompra.ToList());
        }
        #endregion


        #region Funcionalidad de Nueva Compra
        public ActionResult MostrarProveedoresMateriales(int? id, int? idM, int? cantidad, string btnOpcion, double? costoMaterial)
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


                        var listaProvMate = db.Database.SqlQuery<ProveedorMaterials>("Select * from ProveedorMaterials where Proveedor_Id = " + id + "");

                        int[] MaterialesLista = new int[listaProvMate.Count()]; // Arreglo que guardará los Id
                        int val = 0;

                        foreach (var item in listaProvMate)
                        { // Guardar los ID de materiales del proveedor
                            MaterialesLista[val] = item.Material_Id;
                            val++;
                        }
                        var listaMate = db.Materiales.Where(x => MaterialesLista.Contains(x.Id));
                        ViewBag.idProveedor = id;

                        return View("_MostrarProveedoresMateriales", listaMate.ToList());
                    }
                    else
                    {
                        Materiales = Session["Materiales"] as List<Material>;
                        Costos = Session["Costos"] as List<double>;
                        //Importe = Session["TotalCompra"] as List<double>;

                        if (Materiales == null || Costos == null)
                        {
                            Materiales = new List<Material>();
                            Costos = new List<double>();
                        }

                        var materialSeleccionado = db.Materiales.Find(idM);
                        //materialSeleccionado.Costo = Convert.ToDouble(costoMaterial);
                        Materiales.Add(materialSeleccionado);
                        Session["Materiales"] = Materiales;

                        AgregarMaterialALista(idM, cantidad);
                        
                        //Abro - Guardamos el costo que agregamos de cada material
                        Costos.Add(Convert.ToDouble(costoMaterial));
                        Session["Costos"] = Costos;
                        //Cierro - Guardamos el costo que agregamos de cada material

                        // Guardar el total y pasarlo al carrito
                        double total = CalcularTotal(costoMaterial);
                        //Importe.Add(total);
                        Session["TotalCompra"] = total;

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
        
        public double CalcularTotal(double? costo)
        {
            List<int> ids = Session["IdMaterialesAgregados"] as List<int>;
            if (ids == null)
                return subTotal;
            subTotal = 0;
            Cantidades = Session["Cantidades"] as List<int>;
            for (int i = 0; i < ids.Count; i++)
            {
                var material = db.Materiales.Find(ids[i]);
                subTotal += (Convert.ToDouble(costo) * Cantidades[i]); //CREAR SESSION Y GUARDAR LOS COSTOS 
            } // CON EL SESSION PASAR LOS COSTOS PERSONALIZADOS A LA VISTA DEL CARRITO
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
                Costos = Session["Costos"] as List<double>;
                //Importe = Session["TotalCompra"] as List<double>;
                var index = ListaMateriales.IndexOf(Convert.ToInt32(idM));

                ListaMateriales.RemoveAt(index);
                Cantidades.RemoveAt(index);
                Costos.RemoveAt(index);
                //Importe.RemoveAt(index);

                ListaMateriales.RemoveAll(cadena => string.IsNullOrEmpty(Convert.ToString(cadena)));
                Cantidades.RemoveAll(cadena => string.IsNullOrEmpty(Convert.ToString(cadena)));
                Costos.RemoveAll(cadena => string.IsNullOrEmpty(Convert.ToString(cadena)));
                //Importe.RemoveAll(cadena => string.IsNullOrEmpty(Convert.ToString(cadena)));
            }
        }



        public ActionResult AgregarNuevaCompra(int id)
        {

            var idEncargado = User.Identity.GetUserId();
            var idProveedor = id;
            //var idProveedor = Convert.ToInt32(Session["IdProveedor"].ToString());

            //db.Database.ExecuteSqlCommand("Insert into Compras " +
            //                                    "(Recibida, FechaSolicitud, FechaRecepción, Proveedor_Id, Encargado_Id, Estatus)" +
            //                                    "values (" + 0 + ", " + DateTime.Now + ", " + DateTime.Now + ", " + idProveedor + ", " + idEncargado + ", " + 1 + ")");

            db.Database.ExecuteSqlCommand("Insert into Compras " +
                                                "(Recibida, FechaSolicitud, FechaRecepción, Proveedor_Id, Encargado_Id, Estatus)" +
                                                "values (@Recibida, @FechaSolicitud, @FechaRecepción, @Proveedor_Id, @Encargado_Id, @Estatus)",
                                                new SqlParameter("@Recibida", false),
                                                new SqlParameter("@FechaSolicitud", DateTime.Now),
                                                new SqlParameter("@FechaRecepción", DateTime.Now),
                                                new SqlParameter("@Proveedor_Id", idProveedor),
                                                new SqlParameter("@Estatus", true),
                                                new SqlParameter("@Encargado_Id", idEncargado)                                      
                                                );
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
                db.SaveChanges();
            } // checar porque materiales no elimina los datos que uno le quita cuando le presiona a quitar
            //db.SaveChanges();

            Session["Cantidades"] = null;
            Session["Materiales"] = null;
            Session["IdMaterialesAgregados"] = null;

            return RedirectToAction("Details", "Compras", new { id = idCompra }); 
        }

        public ActionResult CancelarCompra()
        {
            Session["Costos"] = null;
            Session["Cantidades"] = null;
            Session["Materiales"] = null;
            Session["IdProveedor"] = null;
            Session["TotalCompra"] = null;
            Session["MaterialesAgregados"] = null;
            Session["IdMaterialesAgregados"] = null;

            subTotal = 0;
            totalCompra = 0;

            return View("Index");
        }

        public ActionResult DescargarOrdenCompra(int idCompra)
        {
            try
            {
                var rptH = new ReportClass();
                rptH.FileName = Server.MapPath("/Views/Compras/ReportesCompras/OrdenCompra.rpt");
                rptH.Load();

                rptH.SetParameterValue("IdCompra", idCompra);

                var connInfo = CrystalReportCnn.GetConnectionInfo();
                TableLogOnInfo logOnInfo = new TableLogOnInfo();
                Tables tables;
                tables = rptH.Database.Tables;
                foreach (Table table in tables)
                {
                    logOnInfo = table.LogOnInfo;
                    logOnInfo.ConnectionInfo = connInfo;
                    table.ApplyLogOnInfo(logOnInfo);
                }

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();

                Stream stream = rptH.ExportToStream(ExportFormatType.PortableDocFormat);
                rptH.Dispose();
                rptH.Close();

                return new FileStreamResult(stream, "application/pdf");
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

    }
}