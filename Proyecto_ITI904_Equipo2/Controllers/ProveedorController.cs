using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    public class ProveedorController : Controller
    { 
        //ProyectoDbContext dbModel; // Nombre hipotético para el DBContext del proyecto

        // GET: Proveedor
        public ActionResult Index(int? IdProveedor)
        {
            //this.dbModel = new VideoJuegoContext(); // Declarar DBContext cada que se inicie la página

            if (IdProveedor == null)
            {
                return View(); // Esto es solo para que no de lata porque no tiene nada que regresar a la vista
                //return View(this.dbModel.Proveedor.ToList()); // Regresará los datos de Proveedor que encuentre de la BD a la vista
            }
            else
            { // Esto es en caso de que busquemos el proveedor se actualice la página, si no se actualiza no será necesario
                return View();
                //return View(BuscarProveedor(IdProveedor)); // Regresará el proveedor específico que busca
            }
        }



        //public ActionResult AgregarProveedor(Proveedor proveedor) // Insertar y modificar los proveedores
        //{
        //    // https://stackoverflow.com/questions/31342809/how-to-execute-insert-query-using-entity-framework Donde saqué el código

        //    try 
        //    {
        //        // ¿Debo poner using y declarar un nuevo dbModel?
        //        if (proveedor.IdProveedor != null) // --------------------- Inserta datos
        //        {
        //            dbModel.Database.ExecuteSqlCommand("Insert into Proveedor values " +
        //                                                        "Nombre =    @nom, " +
        //                                                        "Telefono =  @tel," +
        //                                                        "Direccion = @dir," +
        //                                                        "ImagenUrl = @ImgUrl," +
        //                                                        "Estatus =   true",
        //                                                            new SqlParameter("@nom", proveedor.Nombre),
        //                                                            new SqlParameter("@tel", proveedor.Telefono),
        //                                                            new SqlParameter("@dir", proveedor.Direccion),
        //                                                            new SqlParameter("@ImgUrl", proveedor.ImagenUrl),
        //                                                            new SqlParameter("@status", proveedor.Estatus));
        //            dbModel.SaveChanges(); //¿Es necesario?
        //            return View();
        //        }
        //        else // ------------------- Modifica datos
        //        {
        //            dbModel.Database.ExecuteSqlCommand("Insert into Proveedor values " +
        //                                                        "IdProveedor = @idP, " +
        //                                                        "Nombre =      @nom, " +
        //                                                        "Telefono =    @tel," +
        //                                                        "Direccion =   @dir," +
        //                                                        "ImagenUrl =   @ImgUrl",
        //                                                            new SqlParameter("@idP", proveedor.IdProveedor),
        //                                                            new SqlParameter("@nom", proveedor.Nombre),
        //                                                            new SqlParameter("@tel", proveedor.Telefono),
        //                                                            new SqlParameter("@dir", proveedor.Direccion),
        //                                                            new SqlParameter("@ImgUrl", proveedor.ImagenUrl));
        //            dbModel.SaveChanges(); //¿Es necesario?
        //            return View();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return View();
        //    }

        //}



        //public ActionResult DarBajaProveedor(int IdProveedor)
        //{
        //    try
        //    {
        //        dbModel.Database.ExecuteSqlCommand("Update Proveedor set Estatus = false where " +
        //                                                        "IdProveedor = @IdP",
        //                                                            new SqlParameter("@IdP", IdProveedor));
        //        dbModel.SaveChanges(); //¿Es necesario?
        //        return View();
        //        //return View("Index"); Según el internet es una forma de regresar a la página index del controller, ¿Es cierto?
        //    }
        //    catch (Exception)
        //    {
        //        return View();
        //    }
        //}



        // ---------------- Métodos que no actualizan la página ---------------- //

        //    public List<Proveedor> BuscarProveedor(int IdProveedor)
        //{
        //    var proveedorEspecifico = dbModel.Database.SqlQuery<Proveedor>(@"Select * from Proveedor where IdProveedor = @id", 
        //        new SqlParameter("@id", IdProveedor)).FirstOrDefault();

        //    return proveedorEspecifico;
        //}

    }
}