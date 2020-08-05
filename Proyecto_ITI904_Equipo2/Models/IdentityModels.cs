using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Proyecto_ITI904_Equipo2.Models
{
    // Para agregar datos de perfil del usuario, agregue más propiedades a su clase ApplicationUser. Visite https://go.microsoft.com/fwlink/?LinkID=317594 para obtener más información.
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        public ICollection<Recetas.Receta> RecetasFavoritas { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Tenga en cuenta que el valor de authenticationType debe coincidir con el definido en CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Agregar aquí notificaciones personalizadas de usuario
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Ventas.Venta> Ventas { get; set; }
        public DbSet<Compras.Compra> Compras { get; set; }
        public DbSet<Compras.Proveedor> Proveedores { get; set; }
        public DbSet<Inventario.Material> Materiales { get; set; }
        public DbSet<Recetas.Receta> Recetas { get; set; }
        public DbSet<Ventas.DetalleVenta> DetalleVenta { get; set; }
        public DbSet<Recetas.IngredienteDeReceta> IngredienteDeRecetas { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /*
             * MAPEO USUARIOS_RECETAS
             * Es necesario mapear manualmente esta relación debido a que de no hacerlo
             * nos agrega un campo UserId a la receta (renacion 1:n) y para hacerlo procesar
             * correctamente en modo automatico necesitariamos agregar a las recetas una lista
             * de usuarios (inecesario y es un enfoque tonto)
             */
            modelBuilder.Entity<ApplicationUser>()
                .HasMany<Recetas.Receta>(e => e.RecetasFavoritas)
                .WithMany()
                .Map(ru =>{
                    ru.MapLeftKey("UserId");
                    ru.MapRightKey("RecetaId");
                    ru.ToTable("RecetasUsuarios");
                });
        }

        public System.Data.Entity.DbSet<Proyecto_ITI904_Equipo2.Models.Recetas.TipoPreparacion> TipoPreparacions { get; set; }

        public System.Data.Entity.DbSet<Proyecto_ITI904_Equipo2.Models.Ventas.Producto> Productoes { get; set; }
    }
}