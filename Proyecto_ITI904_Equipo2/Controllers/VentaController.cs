using Microsoft.AspNet.Identity;
using Proyecto_ITI904_Equipo2.Models;
using Proyecto_ITI904_Equipo2.Models.Inventario;
using Proyecto_ITI904_Equipo2.Models.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    public class VentaController : Controller
    {
        public List<Material> Materiales;
        public List<int> ListaMateriales;
        public List<int> Cantidades;
        public DateTime FechaSolicitada;
        public double total = 0;
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Ventas
        public ActionResult Index()
        {
            ListaMateriales = Session["IdMaterialesAgregados"] as List<int>;
            Cantidades = Session["Cantidades"] as List<int>;
            Materiales = Session["Materiales"] as List<Material>;
            if (ListaMateriales != null && Cantidades !=  null && Materiales != null)
            {
                ViewBag.Total = CalcularTotal();
                ViewBag.Agregados = Session["IdMaterialesAgregados"];
                return View(db.Materiales.ToList().Where(x => x.DisponibleAPublico == true));
            }
            return View(db.Materiales.ToList().Where(x=> x.DisponibleAPublico==true));
        }
        [HttpPost]
        public ActionResult Index(int? id, int? cantidad, string btnOpcion)
        {
            if ((id == null || cantidad == null || cantidad <= 0) && btnOpcion == "Añadir" )
            {
                ViewBag.Total = CalcularTotal();
                ViewBag.Agregados = Session["IdMaterialesAgregados"];
                return View(db.Materiales.ToList().Where(x => x.DisponibleAPublico == true));
            }
            else if (id != null && cantidad == -1 && btnOpcion == "Quitar")
            {
                QuitarDeLista(id);
                ViewBag.Total = CalcularTotal();
                ViewBag.Agregados = Session["IdMaterialesAgregados"];
                return View(db.Materiales.ToList().Where(x => x.DisponibleAPublico == true));
            }
            else
            {
                var materialSeleccionado = db.Materiales.Find(id);
                if (materialSeleccionado?.Existencia < cantidad)
                {
                    ViewBag.Total = CalcularTotal();
                    ViewBag.Agregados = Session["IdMaterialesAgregados"];
                    return View(db.Materiales.ToList().Where(x => x.DisponibleAPublico == true));
                }
                Materiales = Session["Materiales"] as List<Material>;
                if (Materiales == null)
                {
                    Materiales = new List<Material>();
                }
                Materiales.Add(materialSeleccionado);
                Session["Materiales"] = Materiales;
                
                AgregarMaterialALista(id, cantidad);
                ViewBag.Total = CalcularTotal();
                ViewBag.Agregados = Session["IdMaterialesAgregados"];
                return View(db.Materiales.ToList().Where(x => x.DisponibleAPublico == true));
            }
        }
        // POST: Ventas/Create
        [HttpPost]
        public async Task<ActionResult> RealizarVenta(DateTime? FechaSolicitada)
        {
            try
            {
                if (FechaSolicitada == null)
                {
                    return RedirectToAction("Index","Venta");
                }
                var NombreUsuario = User.Identity.Name;
                var Id = User.Identity.GetUserId();
                //using (var context = new ApplicationDbContext())
                //{

                //}

                var usuario = db.Users.Where(x => x.Id == Id).FirstOrDefault();

                if (FechaSolicitada == null)
                {
                    return RedirectToAction("Index");
                }
                List<DetalleVenta> ListaProductos = new List<DetalleVenta>();
                Cantidades = Session["Cantidades"] as List<int>;
                Materiales = Session["Materiales"] as List<Material>;
                

                Venta NuevaVenta = new Venta
                {
                    Cliente_Id = usuario.Id,
                    EmpleadoResponsable = null,
                    EstadoPedido = EstadosDePedido.Pedido,
                    FechaPedido = DateTime.Now,
                    FechaSolicitada = Convert.ToDateTime(FechaSolicitada),
                    FechaEntrega = DateTime.Now,
                };
                db.Ventas.Add(NuevaVenta);
                await db.SaveChangesAsync();
                var UltimaVenta = db.Ventas.Max(x => x.Id);
                for (int i = 0; i < Materiales?.Count; i++)
                {
                    string query = $@"INSERT INTO DetalleVentas 
                                      (Cantidad, Precio, Costo,  Venta_Id) 
                                        OUTPUT INSERTED.Id 
                                        VALUES
                                      ({Cantidades[i]}, {Materiales[i].Precio},  {Materiales[i].Costo}, {UltimaVenta})";
                    db.Database.ExecuteSqlCommand(query);
                    query = $@"UPDATE Materials SET Existencia = {Materiales[i].Existencia - Cantidades[i]} where Id = {Materiales[i].Id}";
                    db.Database.ExecuteSqlCommand(query);
                }
                await db.SaveChangesAsync();
                var e = db.Database.SqlQuery<int>("SELECT MAX(Id) FROM DetalleVentas").FirstOrDefault();
                for (int i = 0; i < Materiales?.Count; i++)
                {
                    string query = $@"INSERT INTO Productos 
                                (Id, Material_Id) VALUES
                                ({e}, {Materiales[i].Id})";
                    db.Database.ExecuteSqlCommand(query);
                }
                await db.SaveChangesAsync();
                Session["Cantidades"] = null;
                Session["Materiales"] = null;
                Session["IdMaterialesAgregados"] = null;
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        } // ==================== LOGICA DE NEGOCIO =================

        public double CalcularTotal()
        {
            List<int> ids = Session["IdMaterialesAgregados"] as List<int>;
            if (ids == null)
                return total;
            total = 0;
            Cantidades = Session["Cantidades"] as List<int>;
            for (int i = 0; i < ids.Count; i++)
            {
                var material = db.Materiales.Find(ids[i]);
                total += (material.Precio * Cantidades[i]);
            }
            return total;
        }
        public void AgregarMaterialALista(int? id, int? cantidad)
        {
            Material material = db.Materiales.Find(id);
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
        public void QuitarDeLista(int? id)
        {
            Material material = db.Materiales.Find(id);
            if (material != null)
            {
                ListaMateriales = Session["IdMaterialesAgregados"] as List<int>;
                Cantidades = Session["Cantidades"] as List<int>;
                var index = ListaMateriales.IndexOf(Convert.ToInt32(id));

                ListaMateriales.RemoveAt(index);
                Cantidades.RemoveAt(index);
                ListaMateriales.RemoveAll(cadena => string.IsNullOrEmpty(Convert.ToString(cadena)));
                Cantidades.RemoveAll(cadena => string.IsNullOrEmpty(Convert.ToString(cadena)));
            }
        }
    }
}
