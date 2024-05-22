using Dapper;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using DataAccessLibrary.Models.Reports;
using DataAccessLibrary.QueriesStrategy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.ReportsDAL
{
    public class RpMarcacionesDAL
    {
        public IConfiguration Configuration;

        private ITipoDispositivos _tipoDispositivos;

        public RpMarcacionesDAL(IConfiguration configuration)
        {
            Configuration = configuration;
            _tipoDispositivos = _factoryDispositivoQueries.Factory(Configuration.GetSection("TipoDispositivos").Value.ToLower());
        }

        private FactoryDispositivoQueries _factoryDispositivoQueries { get; set; }
         = new FactoryDispositivoQueries();
        public async Task<List<RpMarcacionesModel>> GetMarcacionesAsync(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            // Antes
            // string sql = SELECT_MARCACIONES(csLstUsers, fDesde, fHasta);

            //Después
            string sql = _tipoDispositivos.SELECT_MARCACIONES(csLstUsers, fDesde, fHasta);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<RpMarcacionesModel> result = await db.QueryAsync<RpMarcacionesModel>(sql, null, commandType: CommandType.Text);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }
        public async Task<List<RpMarcacionesMRLModel>> GetMarcacionesMRLAsync(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            // Antes
            // string sql = SELECT_MARCACIONES(csLstUsers, fDesde, fHasta);

            //Después
            string sql = _tipoDispositivos.SELECT_MARCACIONES_MRL(csLstUsers, fDesde, fHasta);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<RpMarcacionesMRLModel> result = await db.QueryAsync<RpMarcacionesMRLModel>(sql, null, commandType: CommandType.Text);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<RpMarcacionesModel> GetMarcaciones(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            string sql = _tipoDispositivos.SELECT_MARCACIONES(csLstUsers, fDesde, fHasta);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<RpMarcacionesModel> result = db.Query<RpMarcacionesModel>(sql, null, commandType: CommandType.Text);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<bool> DeleteMarcacionManual(string userId, DateTime fhMarcacion)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"SET DateFormat dmy;
                    DELETE FROM CheckInOut WHERE Userid = " + userId + " AND CHECKTIME = '" + fhMarcacion.ToString("dd/MM/yyyy HH:mm") + "';\n" +
                    "DELETE FROM CheckExact WHERE Userid = " + userId + " AND CHECKTIME = '" + fhMarcacion.ToString("dd/MM/yyyy HH:mm") + "';";
                db.Open();
                try
                {
                    int result = await db.ExecuteAsync(sql, null, commandType: CommandType.Text);
                    return result == 2;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task LogInsertMarcacionManual(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega Marcación Manual";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select HolidayName from HOLIDAYS 
                                    where HolidayId = (select IDENT_CURRENT('HOLIDAYS')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateMarcacionManual(SystemLogModel log)
        {
            log.LogDescr = "Modifica Marcación Manual";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select HolidayName from HOLIDAYS 
                                    where HolidayId = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteMarcacionManual(SystemLogModel log)
        {
            log.LogDescr = "Borra Marcación Manual";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
