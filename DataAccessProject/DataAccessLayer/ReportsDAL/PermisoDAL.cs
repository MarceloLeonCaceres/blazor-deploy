using Dapper;
using DataAccessLibrary.Models.Auxiliares;
using DataAccessLibrary.Models.Reports;

using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.ReportsDAL
{
    public class PermisoDAL
    {
        public IConfiguration Configuration { get; set; }

        public PermisoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public int CuentaPermisosSobrepuestos(int userId, DateTime fDesde, DateTime fHasta)
        {
            var parametros = new DynamicParameters();
            parametros.Add("UserId", userId, DbType.Int32);
            parametros.Add("FDesde", fDesde, DbType.DateTime);
            parametros.Add("FHasta", fHasta, DbType.DateTime);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"SET DateFormat dmy;
                    SELECT COUNT(*) FROM USER_SPEDAY WHERE Userid = @UserId AND 
                    ( StartSpecDay between @FDesde and @FHasta OR 
                      EndSpecDay   between @FDesde and @FHasta OR
                      StartSpecDay <= @FDesde and EndSpecDay >= @FHasta );";
                db.Open();
                try
                {
                    int result = db.ExecuteScalar<int>(sql, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }
        
        public int CuentaPermisosSobrepuestos(string csvUsersId, DateTime fDesde, DateTime fHasta)
        {
            var parametros = new DynamicParameters();            
            parametros.Add("FDesde", fDesde, DbType.DateTime);
            parametros.Add("FHasta", fHasta, DbType.DateTime);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"SET DateFormat dmy;
                    SELECT COUNT(*) FROM USER_SPEDAY WHERE Userid in ( " + csvUsersId + @") AND 
                    ( StartSpecDay between @FDesde and @FHasta OR 
                      EndSpecDay   between @FDesde and @FHasta OR
                      StartSpecDay <= @FDesde and EndSpecDay >= @FHasta );";
                db.Open();
                try
                {
                    int result = db.ExecuteScalar<int>(sql, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public int InsertaPermisos(PermisoModel permiso)
        {
            PermisoComplejo permisoComplejo = new PermisoComplejo(permiso);

            var parametros = new DynamicParameters();
            parametros.Add("UserId", permisoComplejo.UserId, DbType.Int32);
            parametros.Add("Tipo", permisoComplejo.IdTipo, DbType.Int16);
            parametros.Add("Motivo", permisoComplejo.Motivo, DbType.String);
            parametros.Add("IdSolicitud", permisoComplejo.IdSolicitud, DbType.Int32);
            parametros.Add("Fecha", DateTime.Now, DbType.DateTime);
            int result = 0;

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"SET DateFormat dmy;
                    INSERT INTO USER_SPEDAY 
                    (USERID, DATEID, YUANYING, [DATE], idSolicitud, STARTSPECDAY, ENDSPECDAY)
                    VALUES (@UserId, @Tipo, @Motivo, @Fecha, @IdSolicitud, '";
                    
                db.Open();
                try
                {
                    foreach(RangoHorario rango in permisoComplejo.LstRangoHorarios)
                    {
                        string sFechasHoras = rango.Ini.ToString("dd/MM/yyyy HH:mm:ss") + "', '" + rango.Fin.ToString("dd/MM/yyyy HH:mm:ss") + "');";
                        result = db.Execute(sql + sFechasHoras, parametros, commandType: CommandType.Text);
                    }
                    
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }


        public int InsertaPermisosMasivos(List<PermisoModel> lstPermisos)
        {
            int contador = 0;
            foreach(PermisoModel permiso in lstPermisos)
            {
                contador += InsertaPermisos(permiso);
            }

            return contador;
        }


    }
}
            
