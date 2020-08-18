using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/*
 * Acordemos lineamientos para entendernos son necesidad de hacer el
 * sistema mucho más complejo de lo que ya es, evitaremos manejar
 * cantidades marcadas en el inventario y no pondremos productos por
 * pieza, nosotros vendemos café y liquidos o especias que ponerles,
 * por ende:
 * -Los precios son solo pesos mexicanos
 * -Las cantidades almacenadas son kilos y litros segun convenga
 * -Los precios son pesos/kilos, para mostrarlos al cliente y demas
 *  haremos los calculos (solo para mostrar, al sistema entran puros kilos)
 */

namespace Proyecto_ITI904_Equipo2.Models.Inventario
{

    /// <summary>
    /// Clase que representa los objetos en el inventario, podemos tener
    /// kilos de café para vender y por supuesto tambien vender un kilo
    /// de café a un cliente
    /// </summary>
    public class Material
    {
        public int Id { get; set; }

        /// <summary>
        /// Determina el nombre del producto (corto y mostrado en listas)
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción profunda de que es lo que consiste el producto
        /// </summary>
        /// <remarks>
        /// En este campo colocaremos las descripciones para los productos que se
        /// venden directamente al cliente, por ejemplo, para un kilo de café
        /// describir de donde viene, que nivel de tostado, etc.
        /// </remarks>
        public string Descripcion { get; set; }

        public double Contenido { get; set; }

        public double Precio { get; set; }
        public double Costo { get; set; }

        /// <summary>
        /// Indica si el producto estará disponible para venta al publico
        /// o solo es interno para la preparación de los productos
        /// </summary>
        public bool DisponibleAPublico { get; set; }

        public double Existencia { get; set; }

        /// <summary>
        /// Unidad para almcenamiento, generalmente seran Kg o L
        /// </summary>
        public string UnidadInventario { get; set; }

        /// <summary>
        /// Unidad mostrada al cliente, generamente seran g o ml
        /// </summary>
        public string UnidadVenta { get; set; }

        /// <summary>
        /// Columna que indica si el material está asignado a un proveedor (True) o no (False)
        /// Se usa para evitar mostrar aquellos datos que ya tienen proveedor cuando se estén asignando
        /// nuevos materiales a uno que no tiene o que tiene varios.
        /// </summary>
        public bool EnUso { get; set; }

        public bool Estatus { get; set; }

        public virtual ICollection<Compras.Proveedor> Proveedores { get; set; }
    }

    public class ProveedorMaterials
    {
        public int Proveedor_Id { get; set; }
        public int Material_Id { get; set; }
    }
}