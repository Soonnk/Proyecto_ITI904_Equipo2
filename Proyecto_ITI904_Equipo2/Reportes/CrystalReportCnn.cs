using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_ITI904_Equipo2.Views.Compras.ReportesCompras
{
    public class CrystalReportCnn
    {
        public static CrystalDecisions.Shared.ConnectionInfo GetConnectionInfo()
        {
            var SConn = new System.Data.SqlClient.SqlConnectionStringBuilder(
                System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            CrystalDecisions.Shared.ConnectionInfo connectionInfo = new CrystalDecisions.Shared.ConnectionInfo();
            connectionInfo.ServerName = SConn.DataSource;
            connectionInfo.DatabaseName = SConn.InitialCatalog;
            connectionInfo.IntegratedSecurity = true;
            connectionInfo.UserID = SConn.UserID;
            connectionInfo.Password = SConn.Password;

            return connectionInfo;
        }
    }
}