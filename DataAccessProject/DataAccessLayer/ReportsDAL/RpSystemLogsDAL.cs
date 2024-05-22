using Dapper;
using DataAccessLibrary.Models.Reports;

using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.ReportsDAL
{
    public class RpSystemLogsDAL
    {
        public IConfiguration Configuration;

        public RpSystemLogsDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string SELECT_LOGs = "SET DateFormat dmy; \n" +
            "SELECT [ID],[Operator],[LogTime],[Alias],[LogTag],[LogDescr],[LogDetailed] \n" +
            "FROM [SystemLog] ";

        public async Task<List<SystemLogModel>> GetSystemLogsAsync(List<int> lstUsers, DateTime fDesde, DateTime fHasta)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_LOGs + " WHERE LogTime >= '" + fDesde.ToShortDateString() + "' AND LogTime < '" + fHasta.AddDays(1).ToShortDateString() + "' \n" +
                    "ORDER BY [LogTime] desc;";
                db.Open();
                try
                {
                    IEnumerable<SystemLogModel> result = await db.QueryAsync<SystemLogModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<SystemLogModel> GetSystemLogs(List<int> lstUsers, DateTime fDesde, DateTime fHasta)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_LOGs + " WHERE LogTime >= '" + fDesde.ToShortDateString() + "' AND LogTime < '" + fHasta.AddDays(1).ToShortDateString() + "' \n" +
                    "ORDER BY [LogTime] desc;";
                db.Open();
                try
                {
                    IEnumerable<SystemLogModel> result = db.Query<SystemLogModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
