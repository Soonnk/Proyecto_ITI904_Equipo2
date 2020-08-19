using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_ITI904_Equipo2.Models;
using Proyecto_ITI904_Equipo2.Models.Recetas;
using Proyecto_ITI904_Equipo2.Models.Ventas;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    [Authorize(Roles = "Empleado,Admin")]
    public class RecetasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Recetas
        public async Task<ActionResult> Index()
        {
            return View(await db.Recetas.ToListAsync());
        }

        // GET: Recetas/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Receta receta = await db.Recetas.FindAsync(id);
            if (receta == null)
            {
                return HttpNotFound();
            }
            return View(receta);
        }

        // GET: Recetas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Recetas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "Id,Nombre,Instrucciones,TiempoPreparacionAlmacenado")] Receta receta)
        public async Task<ActionResult> Create(Receta receta)
        {
            foreach (var item in receta.Ingredientes)
            {
                item.Cantidad /= 1000;
            }

            var file = Request.Files[0];

            byte[] bytes = new byte[file.ContentLength];
            var i = file.InputStream.Read(bytes, 0, file.ContentLength);

            receta.Imagen = Convert.ToBase64String(bytes);

            if (ModelState.IsValid)
            {
                db.Recetas.Add(receta);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(receta);
        }

        // GET: Recetas/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Receta receta = await db.Recetas.FindAsync(id);
            if (receta == null)
            {
                return HttpNotFound();
            }
            return View(receta);
        }

        // POST: Recetas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,Nombre,Instrucciones,TiempoPreparacionAlmacenado")] Receta receta)
        public async Task<ActionResult> Edit(Receta receta)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in receta.Ingredientes)
                {
                    item.Cantidad /= 1000;
                }

                receta.Preparacion = db.TipoPreparacions.Find(receta.Preparacion_Id);

                db.Entry(receta).State = EntityState.Modified;

                foreach (var ingrediente in receta.Ingredientes.ToList())
                {
                    IngredienteDeReceta i; // Declaramos un ingrediente que nos ayudará en caso de tener que buscarlo
                    switch (ingrediente.Estatus)
                    {
                        case 'E':
                            /*
                             * Si el ingrediente esta marcado como editado lo buscamos en la base de datos e indicamos que fue editado
                             */
                            i = await db.IngredienteDeRecetas.FindAsync(ingrediente.Id);
                            db.Entry(i).State = EntityState.Modified;
                            break;
                        case 'D':
                            /*
                             * Si el ingrediente esta marcado como borrado lo buscamos en la base de datos e indicamos que fue borrado
                             */
                            if (ingrediente.Id == 0) break; // si el objeto fue borrado pero no tiene id nunca estuvo en la base de datos por lo que lo ignoramos
                            i = await db.IngredienteDeRecetas.FindAsync(ingrediente.Id);
                            db.Entry(i).State = EntityState.Deleted;
                            break;
                        case 'A':
                        default:
                            /*
                             * Si el producto fue agregado lo añadimos al schema. Si no se sabe cual es su estatus directamente asumimos
                             * que es nuevo
                             */
                            db.IngredienteDeRecetas.Add(ingrediente);
                            break;
                    }
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(receta);
        }

        // GET: Recetas/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Receta receta = await db.Recetas.FindAsync(id);
            if (receta == null)
            {
                return HttpNotFound();
            }
            return View(receta);
        }

        // POST: Recetas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Receta receta = await db.Recetas.FindAsync(id);
            db.Recetas.Remove(receta);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public ActionResult AddToCart(int id, int? cantidad = 1)
        {
            List<ProductoPreparado> productos = Session["ProductosPreparados"] as List<ProductoPreparado> ?? new List<ProductoPreparado>();

            ProductoPreparado producto = new ProductoPreparado() {
                RecetaBase_Id = id
            };

            Receta receta = db.Recetas.Find(id);
            foreach (IngredienteDeReceta i in receta.Ingredientes)
            {
                IngredienteDeProductoVendido newIng = new IngredienteDeProductoVendido
                {
                    Material_Id = i.Material_Id,
                    Cantidad = i.Cantidad,
                    Precio = i.Precio,
                    Costo = i.Costo
                };


                producto.Ingredientes.Add(newIng);
            }
            producto.RecetaBase = receta;

            db.Entry(receta).State = EntityState.Unchanged;

            producto.Cantidad = cantidad.Value;
            producto.Precio = receta.Precio;
            producto.Costo = receta.Costo;

            productos.Add(producto);

            Session["ProductosPreparados"] = productos;
            return RedirectToAction("Index", "Venta");
        }

        public async Task<ActionResult> LoadIngrediente(string name, int index, double? cantidad)
        {
            IngredienteDeReceta ing = new IngredienteDeReceta();
            ing.Id = 0;
            ing.Material = await db.Materiales.FirstOrDefaultAsync<Models.Inventario.Material>(x => x.Nombre == name);
            ing.Cantidad = cantidad ?? 1.0;

            ViewBag.Index = index;

            return PartialView("_IngredienteRecetaTR", ing);
        }

        public ActionResult ListMaterialsAsJSON()
        {
            return Json(db.Materiales.Select(x => x.Descripcion), JsonRequestBehavior.AllowGet);
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
