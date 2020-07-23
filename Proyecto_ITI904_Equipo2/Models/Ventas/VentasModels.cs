using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Proyecto_ITI904_Equipo2.Models.Ventas
{
    /// <summary>
    /// Representa la venta realizada a un cliente
    /// </summary>
    public class Venta
    {
        public int Id { get; set; }

        /// <summary>
        /// Cliente que realiza la compra
        /// </summary>
        /// <remarks>
        /// Se conserva solo un "tipo" de usuario con diferentes roles debido a
        /// que se no hay manera sencilla de mantener 2 tipos heredados de
        /// IdentityUser en el mismo DbContex, además, manejarlo así nos permitirá
        /// dar la opción a los empleados de comprar sin necesidad de otro registro
        /// </remarks>
        public virtual ApplicationUser Cliente { get; set; }

        public virtual ApplicationUser EmpleadoResponsable { get; set; }

        /// <summary>
        /// Fecha en que el pedido es levantado por el cliente
        /// </summary>
        public DateTime FechaPedido { get; set; }

        /// <summary>
        /// Fecha definida por el cliente para recoger su pedido
        /// </summary>
        public DateTime FechaSolicitada { get; set; }

        /// <summary>
        /// Fecha en la que el cliente recibe el pedido y queda concretada la transacción
        /// </summary>
        public DateTime FechaEntrega { get; set; }

        public EstadosDePedido EstadoPedido { get; set; }

        public virtual ICollection<DetalleVenta> Productos { get; set;}
    }

    public abstract class DetalleVenta
    {
        public int Id { get; set; }
        public virtual double Cantidad { get; set; }
        public virtual double Precio { get; set; }
        public virtual double Costo { get; set; }
    }

    [Table("Productos")]
    public class Producto : DetalleVenta
    {
        public virtual Inventario.Material Material { get; set; }
    }

    [Table("ProductosPreparados")]
    public class ProductoPreparado : DetalleVenta
    {
        public virtual Recetas.Receta RecetaBase { get; set; }

        public virtual Recetas.Sabores Sabor { get; set; }
        
        public virtual bool Alterado { get; set; }

        public virtual ICollection<Recetas.IngredienteDeProductoVendido> Ingredientes { get; set; }

    }

    public enum EstadosDePedido : Int16
    {
        /// <summary>
        /// El pedido fue solicitado por el cliente, está en la cola de pedidos
        /// </summary>
        Pedido,

        /// <summary>
        /// El pedido ya fue tomado por un empleado que se encargará de su preparación
        /// </summary>
        Preparando,

        /// <summary>
        /// El producto está esperando a ser recogido por el cliente
        /// </summary>
        EnEspera,

        /// <summary>
        /// El pedido fue entregado al cliente
        /// </summary>
        Entregado,

        /// <summary>
        /// El pedido fue Cancelado
        /// </summary>
        Cancelado
    }
}