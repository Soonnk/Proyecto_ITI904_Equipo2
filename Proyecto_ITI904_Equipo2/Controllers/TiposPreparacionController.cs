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

namespace Proyecto_ITI904_Equipo2.Controllers
{
    public class TiposPreparacionController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TiposPreparacion
        public async Task<ActionResult> Index()
        {
            return View(await db.TipoPreparacions.ToListAsync());
        }

        // GET: TiposPreparacion/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoPreparacion tipoPreparacion = await db.TipoPreparacions.FindAsync(id);
            if (tipoPreparacion == null)
            {
                return HttpNotFound();
            }
            return View(tipoPreparacion);
        }

        // GET: TiposPreparacion/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TiposPreparacion/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Nombre,Descripción")] TipoPreparacion tipoPreparacion)
        {
            if (ModelState.IsValid)
            {
                db.TipoPreparacions.Add(tipoPreparacion);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(tipoPreparacion);
        }

        // GET: TiposPreparacion/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoPreparacion tipoPreparacion = await db.TipoPreparacions.FindAsync(id);
            if (tipoPreparacion == null)
            {
                return HttpNotFound();
            }
            return View(tipoPreparacion);
        }

        // POST: TiposPreparacion/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Nombre,Descripción")] TipoPreparacion tipoPreparacion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoPreparacion).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tipoPreparacion);
        }

        // GET: TiposPreparacion/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoPreparacion tipoPreparacion = await db.TipoPreparacions.FindAsync(id);
            if (tipoPreparacion == null)
            {
                return HttpNotFound();
            }
            return View(tipoPreparacion);
        }

        // POST: TiposPreparacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TipoPreparacion tipoPreparacion = await db.TipoPreparacions.FindAsync(id);
            db.TipoPreparacions.Remove(tipoPreparacion);
            await db.SaveChangesAsync();
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
    }
}
