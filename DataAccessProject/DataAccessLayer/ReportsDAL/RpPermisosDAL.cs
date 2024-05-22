using Dapper;
using DataAccessLibrary.Models;
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
    public class RpPermisosDAL
    {
        public IConfiguration Configuration;

        private const string iCharsHora = "5";

        public const string SELECT_PERMISOS = @"SET DateFormat dmy;
            SELECT U.Name as Empleado, U.Badgenumber as NumCA, U.SSN as Cedula, DEPARTMENTS.DEPTNAME as Departamento, 
            LEAVECLASS.LEAVENAME as Permiso, 
            case DATEPART(DW, SPE.STARTSPECDAY) when 1 then 'Lunes' when 2 then 'Martes' when 3 then 'Miércoles' when 4 then 'Jueves'   when 5 then 'Viernes' when 6 then 'Sábado' when 7 then 'Domingo' end as Dia, 
            CAST(SPE.STARTSPECDAY as DATE) as Fecha, 
            CONVERT(CHAR(" + iCharsHora + "), SPE.STARTSPECDAY, 108) as Inicio, " +
            "CONVERT(CHAR(" + iCharsHora + "), SPE.ENDSPECDAY, 108) as Fin, " +
            "CONVERT(CHAR(" + iCharsHora + @"), SPE.ENDSPECDAY - SPE.STARTSPECDAY, 108) as Tiempo, 
            SPE.YUANYING as Motivo, 
            LEAVECLASS.LeaveName as TipoPermiso, U.userid, SPE.IdSolicitud  
            FROM DEPARTMENTS INNER JOIN (USERINFO U INNER JOIN (LEAVECLASS RIGHT JOIN USER_SPEDAY SPE ON 
            LEAVECLASS.LEAVEID = SPE.DATEID) ON U.USERID = SPE.USERID) ON DEPARTMENTS.DEPTID = U.DEFAULTDEPTID " + "\n";

        public RpPermisosDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<RpPermisosModel>> GetPermisosAsync(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_PERMISOS + " WHERE U.Userid in (" + csLstUsers + ") \n" +
                    "AND SPE.STARTSPECDAY >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND SPE.ENDSPECDAY < '" + fHasta.ToString("dd/MM/yyyy") + "' \n" +
                    "ORDER BY U.Name, U.Badgenumber, SPE.STARTSPECDAY;";
                db.Open();
                try
                {
                    IEnumerable<RpPermisosModel> result = await db.QueryAsync<RpPermisosModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<RpPermisosModel> GetPermisos(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_PERMISOS + " WHERE U.Userid in (" + csLstUsers + ") " +
                    "AND SPE.STARTSPECDAY >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND SPE.ENDSPECDAY < '" + fHasta.ToString("dd/MM/yyyy") + "' \n" +
                    "ORDER BY U.Name, U.Badgenumber, SPE.STARTSPECDAY;";
                db.Open();
                try
                {
                    IEnumerable<RpPermisosModel> result = db.Query<RpPermisosModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }
        
        public async Task<bool> DeletePermisoAsync(string userId, DateTime fDesde)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = "SET DateFormat dmy; \n" +
                    "DELETE USER_SPEDAY WHERE Userid in (" + userId + ") \n" +
                    "AND STARTSPECDAY = '" + fDesde.ToString("dd/MM/yyyy HH:mm") + "';";
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql);
                    return true;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }


        public async Task LogInsertPermiso(SystemLogModel log)
        {
            log.LogDescr = "Agrega Permiso";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();

                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdatePermiso(SystemLogModel log)
        {
            log.LogDescr = "Modifica Permiso";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeletePermiso(SystemLogModel log)
        {
            log.LogDescr = "Borra Permiso";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
