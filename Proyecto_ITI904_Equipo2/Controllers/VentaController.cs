using Microsoft.AspNet.Identity;
using Proyecto_ITI904_Equipo2.Models;
using Proyecto_ITI904_Equipo2.Models.Inventario;
using Proyecto_ITI904_Equipo2.Models.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

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

            return View(db.Materiales.ToList().Where(x=> x.DisponibleAPublico==true));
        }
        [HttpPost]
        public ActionResult Index(int? id, int? cantidad)
        {
            if (id == null || cantidad == null || cantidad <= 0)
            {
                return View(db.Materiales.ToList().Where(x => x.DisponibleAPublico == true));
            }
            var materialSeleccionado = db.Materiales.Find(id);
            Materiales = Session["Materiales"] as List<Material>;
            if (Materiales == null)
            {
                Materiales = new List<Material>(); 
            }
            Materiales.Add(materialSeleccionado);
            Session["Materiales"] = Materiales;
            if (materialSeleccionado?.Existencia< cantidad)
            {
                return View(db.Materiales.ToList().Where(x => x.DisponibleAPublico == true));
            }
            AgregarMaterialALista(id, cantidad);
            ViewBag.Total = CalcularTotal();
            ViewBag.Agregados = Session["MaterialesAgregados"];
            return View(db.Materiales.ToList().Where(x => x.DisponibleAPublico == true));
        }
        // POST: Ventas/Create
        [HttpPost]
        public ActionResult RealizarVenta(DateTime FechaSolicitada)
        {
            try
            {
                var NombreUsuario = User.Identity.Name;
                var Id = User.Identity.GetUserId();
                using (var context = new ApplicationDbContext())
                {
                    var usuario = context.Users.Where(x => x.Id == Id).FirstOrDefault();

                    if (FechaSolicitada == null)
                    {
                        return RedirectToAction("Index");
                    }
                    List<DetalleVenta> ListaProductos = new List<DetalleVenta>();
                    Cantidades = Session["Cantidades"] as List<int>;
                    Materiales = Session["Materiales"] as List<Material>;
                    for (int i = 0; i < Materiales?.Count; i++)
                    {
                        Producto pro = new Producto();
                        pro.Cantidad = Cantidades[i];
                        pro.Precio = Materiales[i].Precio;
                        pro.Costo = Materiales[i].Costo;
                        pro.Material = Materiales[i];
                        ListaProductos.Add(pro);
                    }
                    
                    Venta NuevaVenta = new Venta
                    {
                        Cliente = usuario,
                        EmpleadoResponsable = null,
                        EstadoPedido = EstadosDePedido.Pedido,
                        FechaPedido = DateTime.Now,
                        FechaSolicitada = FechaSolicitada,
                        Productos = ListaProductos

                    };
                    db.Ventas.Add(NuevaVenta);
                    Session["Cantidades"] = null;
                    Session["Materiales"] = null;
                    Session["MaterialesAgregados"] = null;
                    return RedirectToAction("Index","Home");
                }
                
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        } // ==================== LOGICA DE NEGOCIO =================
        public double CalcularTotal()
        {
            List<int> ids = Session["MaterialesAgregados"] as List<int>;
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
                if (Session["MaterialesAgregados"] == null)
                {
                    ListaMateriales = new List<int>();
                    Cantidades = new List<int>();
                }
                else
                {
                    ListaMateriales = Session["MaterialesAgregados"] as List<int>;
                    Cantidades = Session["Cantidades"] as List<int>;
                }
                ListaMateriales.Add(material.Id);
                Cantidades.Add(Convert.ToInt32(cantidad));
                Session["MaterialesAgregados"] = ListaMateriales;
                Session["Cantidades"] = Cantidades;

            }
        }
    }
}
