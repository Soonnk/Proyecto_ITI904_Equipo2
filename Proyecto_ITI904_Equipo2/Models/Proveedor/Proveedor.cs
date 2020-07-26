using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_ITI904_Equipo2.Models.Proveedor
{
    [Table("VideoJuegos")]
    public class Proveedor
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.None)] // Si bien es un campo primario no hacerlo autoincrementable
        [Key] // Llave primaria
        [Column("IdProveedor")]
        public int IdProveedor { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("Telefono")]
        public string Telefono { get; set; }
        [Column("Direccion")]
        public string Direccion { get; set; }
        [Column("ImagenUrl")] // URL o la codificación de la imagen
        public string ImagenUrl { get; set; }
        [Column("Estatus")] // Si el proveedor ya no está disponible, quebró la compañía o simplemente ya no nos surtimos con él
        public bool Estatus { get; set; }

    }
}