using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.Vacaciones;
using System;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.Vacaciones
{
    public class CorteVacationDAL
    {

        public IConfiguration Configuration;

        public const string SELECT_CORTES = @"SELECT [Userid]
          ,[FechaCorte]
          ,[Motivo]
          ,[date]
          ,[DiasAgregados]
          ,[DiasHabiles]
          ,[Disponible]
        FROM [Vacations.Cortes]
        WHERE UserId = @UserId";

        public CorteVacationDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<CorteVacationModel>> GetCortesVacsAsync(int userid)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("UserId", userid, DbType.Int16);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<CorteVacationModel> result = await db.QueryAsync<CorteVacationModel>(SELECT_CORTES, param, commandType: CommandType.Text);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }


        public async Task InsertaCorteAsync(CorteVacationModel corteVacation)
        {
            DynamicParameters parametros = ParamCorteV(corteVacation);
            string sql = @"INSERT INTO [dbo].[Vacations.Cortes]
            ([Userid], [FechaCorte], [Motivo], [date], [DiasAgregados], [DiasHabiles], [Disponible]) 
            VALUES
                (@Userid, @FechaCorte, @Motivo, @Date, @DiasAgregados, @DiasHabiles, @Disponible)";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task DeleteCortesVacsAsync(int userid, DateTime fecha)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("@UserId", userid, DbType.Int16);
            parametros.Add("@Fecha", fecha, DbType.DateTime);
            string sql = @"DELETE FROM [dbo].[Vacations.Cortes]
            WHERE [Userid] = @UserId AND [FechaCorte] = @Fecha";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        private DynamicParameters ParamCorteV(CorteVacationModel corteVacation)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("UserId", corteVacation.UserId, DbType.Int16);
            parametros.Add("FechaCorte", corteVacation.FechaCorte, DbType.DateTime);
            parametros.Add("date", corteVacation.date, DbType.DateTime);
            parametros.Add("Motivo", corteVacation.Motivo, DbType.String);
            parametros.Add("DiasAgregados", corteVacation.DiasAgregados, DbType.Double);
            parametros.Add("DiasHabiles", corteVacation.DiasHabiles, DbType.Double);
            parametros.Add("Disponible", corteVacation.Disponible, DbType.Double);
            return parametros;
        }

        public async Task LogInsertCorte(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega corte";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"SELECT [Name] FROM [USERINFO] 
                                    WHERE UserId = " + log.LogTag.ToString();
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateCorte(SystemLogModel log)
        {
            log.LogDescr = "Modifica corte";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"SELECT [Name] FROM [USERINFO] 
                                    WHERE UserId = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteCorte(SystemLogModel log)
        {
            log.LogDescr = "Borra corte";            
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"SELECT [Name] FROM [USERINFO] 
                                    WHERE UserId = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }


    }
}
