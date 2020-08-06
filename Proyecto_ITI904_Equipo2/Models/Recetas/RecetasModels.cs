using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Proyecto_ITI904_Equipo2.Models.Recetas
{
    /// <summary>
    /// La receta representa concretamente la forma en que se prepara
    /// el café y los ingredientes que debe tener.
    /// </summary>
    /// <remarks>
    /// Al final será alterable, pero le damos el uso de molde para
    /// predefinir las preparaciones y que el cliente como mucho deba
    /// agregar o quitar ingredientes.
    /// </remarks>
    public class Receta 
    {
        public int Id { get; set; }
        public int Preparacion_Id { get; set; }
        [ForeignKey("Preparacion_Id")]
        public virtual TipoPreparacion Preparacion { get; set; }
        public string Nombre { get; set; }

        public string Instrucciones { get; set; }

        /// <summary>
        /// Tiempo de preparación en Ticks (No mostrar al usuario y preferiblemente no trabajar con este campo)
        /// Usar en su lugar <see cref="Receta.TiempoPreparacion"/>
        /// </summary>
        public long TiempoPreparacionAlmacenado { get; set; }

        /// <summary>
        /// Tiempo estimado para la preparación de la receta
        /// </summary>
        /// <remarks>
        /// Es necesario almacenar esto para ayudarnos a generar la cola de pedidos a realizar.
        /// La forma de administrarlo (el uso de <see cref="Receta.TiempoPreparacionAlmacenado"/> y
        /// <seealso cref="Receta.TiempoPreparacion"/>) se debe a que el tipo TiemSpan no es especialmente
        /// compatible con EntityFramework y terminará mapeado a <see cref="System.Data.SqlDbType.Time"/>
        /// entonces, usaremos el tipo long para interactuar con la base de datos pero al codificar usaremos
        /// el tipo <see cref="TimeSpan"/> para operar fechas de forma más estándar
        /// </remarks>
        [NotMapped]
        [Display(Name = "Tiempo de Preparación")]
        [RegularExpression("[0-9][0-9]:[0-9][0-9]:[0-9][0-9]", ErrorMessage = "El tiempo debe estar en formato HH:MM:SS")]
        public TimeSpan TiempoPreparacion { 
            get => new TimeSpan(TiempoPreparacionAlmacenado);
            set => this.TiempoPreparacionAlmacenado = value.Ticks;
        }

        public string Imagen { get; set; }

        public virtual ICollection<IngredienteDeReceta> Ingredientes { get; set; }

    }

    /// <summary>
    /// La clase Ingrediente es abstracta, solo nos permite definir que
    /// cosas son parte del concepto de ingrediente
    /// </summary>
    /// <remarks>
    /// Se volvio necesario inhabilitar el uso de esta debido a que podía causar
    /// que EntityFramework generara una tabla unificada de ingredientes y los
    /// derivara.
    /// Lamentablemente ese enfoque volvería muy ineficiente la consulta de
    /// ingredientes de recetas al saturar la formación de la tabla,
    /// la unica tabla que querriamos saturar sería la de IngredientesProductoVendido
    /// que es algo así como un detalle del detalle.
    /// </remarks>
    [Obsolete("Eliminado para prevenir generacion de tabla TPT")]
    public abstract class Ingrediente
    {
        public Inventario.Material Material { get; set; }
        public double Cantidad { get; set; }
        public abstract double Costo { get; set; }
        public abstract double Precio { get; set; }
    }

    /// <summary>
    /// Representa un ingrediente de un preparado ya vendido
    /// </summary>
    /// <remarks>
    /// El producto vendido debe conservar su historial por ello el
    /// ingrediente proporciona su propio costo y precio, a diferencia de
    /// <see cref="IngredienteDeReceta"/> que los extrae de su material
    /// </remarks>
    public class IngredienteDeProductoVendido
    {
        public long Id { get; set; }
        public Inventario.Material Material { get; set; }
        public double Cantidad { get; set; }
        public double Costo { get; set; }
        public double Precio { get; set; }
    }

    /// <summary>
    /// Representa el ingrediente de una receta
    /// </summary>
    /// <remarks>
    /// El ingrediente de receta no devuelve sus propios precios y costos ya
    /// que estos pueden variar segun el precio de los materiales en un momento
    /// dado, es por ello que estos ingredientes solo consultan de su material
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Si se intenta alterar el precio o costo de un material mediante la receta,
    /// los cambios de ese tipo se deberían realizar mediante acceso al material
    /// o en el debido caso propiamente desde el ingrediente en la venta.
    /// </exception>
    public class IngredienteDeReceta
    {
        public IngredienteDeReceta()
        {
            this.Estatus = 'E';
        }

        public long Id { get; set; }

        public int Material_Id { get; set; }

        [ForeignKey("Material_Id")]
        public virtual Inventario.Material Material { get; set; }
        public double Cantidad { get; set; }

        [NotMapped]
        public double Costo {
            get => this.Material?.Costo ?? 0;
            set => throw new InvalidOperationException("Los ingredientes de recetas no pueden alterar el Costo de su material");
        }
        [NotMapped]
        public double Precio { 
            get => this.Material?.Precio ?? 0; 
            set => throw new InvalidOperationException("Los ingredientes de recetas no pueden alterar el Precio de su material");
        }
        /// <summary>
        /// Permite saber si la el ingrediente será eliminado, o agregado a la receta
        /// </summary>
        [NotMapped]
        public char Estatus { get; set; }
    }

    /// <summary>
    /// Esta clase controla los tipos de preparaciones (Espresso, Filtrado, etc.)
    /// </summary>
    /// <remarks>
    /// Agregamos esta clase por 2 motivos:
    /// <list type="bullet">
    ///     <item>
    ///         Debe haber opcion a agregar nuevos tipos en caso que
    ///         surjan mas tipos de preparaciones
    ///     </item>
    ///     <item>
    ///         Es necesario brindar a los clientes información sobre como
    ///         la preparacion de su cafe con campos como la descripción
    ///     </item>
    /// </list>
    /// </remarks>
    public class TipoPreparacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripción { get; set; }
        public Tamanio TamanioMinimo { get; set; }

        public ICollection<TipoPreparacionSabor> Recomendaciones { get; set; }
    }

    /// <summary>
    /// Representa la relación entre un Tipo de preparación y el
    /// sabor que esta genera
    /// </summary>
    /// <remarks>
    /// La razon para generar esta opción es que cada tipo de
    /// preparación genera diferente concentración de café debido
    /// a la forma en que el agua pasa por el café. Por lo tanto,
    /// por ejemplo, 6 gr de café en preparación Espresso generan un
    /// sabor mas fuerte que 12 gr en filtrado
    /// </remarks>
    public class TipoPreparacionSabor
    {
        public int Id { get; set; }
        public TipoPreparacion Preparacion {get;set;}
        public Sabores Sabor { get; set; }
        public double CantidadCafe { get; set; }
    }

    public enum Sabores : Int16
    {
        Agrio,
        Afrutado,
        Balanceado,
        Fuerte,
        Personalizado
    }

    /// <summary>
    /// Representan a los tamaños de café que serán vendidos
    /// </summary>
    /// <remarks>
    /// El tener el tamaños registrados nos permite controlar el
    /// tamaño minimo de los cafés segun su tipo de preparación,
    /// esto debido a que por ejemplo la unica preparación que puede
    /// ser pequeña es la Espresso, el resto suelen generar sabores
    /// insipidos si no se usa suficiente agua para extraer sabores
    /// </remarks>
    public class Tamanio
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public double CantidadAgua { get; set; }
    }
}