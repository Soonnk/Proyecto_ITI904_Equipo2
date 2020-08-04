using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Proyecto_ITI904_Equipo2.Models;

namespace Proyecto_ITI904_Equipo2.Services
{
    public static class RecetasServices
    {
        public static HtmlString GetMaterialsAsJSON()
        {
            return new HtmlString(JsonConvert.SerializeObject(new ApplicationDbContext().Materiales.Select(x=>x.Nombre)));
        }
    }
}