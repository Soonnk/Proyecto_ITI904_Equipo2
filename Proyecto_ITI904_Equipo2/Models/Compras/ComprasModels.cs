﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_ITI904_Equipo2.Models.Compras
{
    public class Compra
    {
        public int Id { get; set; }
        /// <summary>
        /// Persona que se encargó de solicitar la Compra
        /// </summary>
        public virtual ApplicationUser Encargado { get; set; }
        public virtual Proveedor Proveedor { get; set; }
        public virtual ICollection<DetalleCompra> Materiales { get; set; }
        /// <summary>
        /// Nos indicará si el material de la compra ya fue surtido o si aun se espera
        /// </summary>
        public bool Recibida { get; set; }

        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaRecepción { get; set; }
    }

    public class DetalleCompra
    {
        public int Id { get; set; }
        public Inventario.Material Material { get; set; }
        public double Cantidad { get; set; }
        public double Costo { get; set; }

    }

    /// <summary>
    /// Representa a un proveedor que nos venderá materiales
    /// </summary>
    public class Proveedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string RFC { get; set; }
        public string Telefono { get; set; }
        /// <summary>
        /// Lista de materiales que el proveedor puede ofrecer
        /// </summary>
        public virtual ICollection<Inventario.Material> Materiales { get; set; }
        public string Nota { get; set; }
    }
}