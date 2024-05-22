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
    public class ConfigVacationDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_CONFIG_VACATIONS = @"SELECT C.[idContrato], C.nomContrato as contrato,
[dVacPeriodo], [dVacHabiles], [dVacAdicional], [aPartir], [dMaxAcumulable]
,[bPeriodoCarga], [bDiaCarga], [eTiempoCumplido], [fDescuentoV], [fDescuentoCV]
,[bAtrasosCV], [bSalidasCV], [bAusenciasCV], [bExcesoLunchCV],[bDescontarDH]
FROM [dbo].[Vacations.Parameters] VP RIGHT join CONTRATO C on VP.idContrato = C.idContrato";


        public ConfigVacationDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<ConfigVacationModel>> GetConfigsVacsAsync()
        {            
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<ConfigVacationModel> result = await db.QueryAsync<ConfigVacationModel>(SELECT_CONFIG_VACATIONS, null, commandType: CommandType.Text);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task InsertaConfigVacationAsync(ConfigVacationModel configVacation)
        {
            DynamicParameters parametros = ParamConfigVacation(configVacation);
            string sql = @"INSERT INTO [dbo].[Vacations.Parameters]
([idContrato], [dVacPeriodo], [dVacHabiles], [dVacAdicional], [aPartir]
,[dMaxAcumulable], [bPeriodoCarga], [bDiaCarga], [eTiempoCumplido], [fDescuentoV], [fDescuentoCV]
,[bAtrasosCV], [bSalidasCV], [bAusenciasCV], [bExcesoLunchCV], [bDescontarDH])
VALUES
(@idContrato, @dVacPeriodo, @dVacHabiles, @dVacAdicional, @aPartir,
@dMaxAcumulable, @bPeriodoCarga, @bDiaCarga, @eTiempoCumplido, @fDescuentoV, @fDescuentoCV, 
@bAtrasosCV, @bSalidasCV, @bAusenciasCV, @bExcesoLunchCV, @bDescontarDH)";

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


        private DynamicParameters ParamConfigVacation(ConfigVacationModel configVacation)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("idContrato", configVacation.idContrato, DbType.Int16);
            parametros.Add("dVacPeriodo", configVacation.dVacPeriodo, DbType.Double);
            parametros.Add("dVacHabiles", configVacation.dVacHabiles, DbType.Double);
            parametros.Add("dVacAdicional", configVacation.dVacAdicional, DbType.Double);
            parametros.Add("aPartir", configVacation.aPartir, DbType.Int16);
            parametros.Add("dMaxAcumulable", configVacation.dMaxAcumulable, DbType.Int16);
            parametros.Add("bPeriodoCarga", configVacation.bPeriodoCarga, DbType.Int16);
            parametros.Add("bDiaCarga", configVacation.bDiaCarga, DbType.Int16);
            parametros.Add("eTiempoCumplido", configVacation.eTiempoCumplido, DbType.Int16);
            parametros.Add("fDescuentoV", configVacation.fDescuentoV, DbType.Double);
            parametros.Add("fDescuentoCV", configVacation.fDescuentoCV, DbType.Double);
            parametros.Add("bAtrasosCV", configVacation.bAtrasosCV, DbType.Boolean);
            parametros.Add("bSalidasCV", configVacation.bSalidasCV, DbType.Boolean);
            parametros.Add("bAusenciasCV", configVacation.bAusenciasCV, DbType.Boolean);
            parametros.Add("bExcesoLunchCV", configVacation.bExcesoLunchCV, DbType.Boolean);
            parametros.Add("bDescontarDH", configVacation.bDescontarDH, DbType.Boolean);
            return parametros;
        }

        public async Task UpdateConfigVacationAsync(ConfigVacationModel configVacation)
        {
            DynamicParameters parametros = ParamConfigVacation(configVacation);
            string sql = @"UPDATE [dbo].[Vacations.Parameters]
   SET [dVacPeriodo] = @dVacPeriodo
      ,[dVacHabiles] = @dVacHabiles
      ,[dVacAdicional] = @dVacAdicional
      ,[aPartir] = @aPartir
      ,[dMaxAcumulable] = @dMaxAcumulable
      ,[bPeriodoCarga] = @bPeriodoCarga
      ,[bDiaCarga] = @bDiaCarga
      ,[eTiempoCumplido] = @eTiempoCumplido
      ,[fDescuentoV] = @fDescuentoV
      ,[fDescuentoCV] = @fDescuentoCV
      ,[bAtrasosCV] = @bAtrasosCV
      ,[bSalidasCV] = @bSalidasCV
      ,[bAusenciasCV] = @bAusenciasCV
      ,[bExcesoLunchCV] = @bExcesoLunchCV
      ,[bDescontarDH] = @bDescontarDH
 WHERE [idContrato] = @idContrato;";

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

        public async Task<ConfigVacationModel> GetConfigVacAsync(string userId)
        {
            string sql = SELECT_CONFIG_VACATIONS + @" WHERE C.idContrato in
(
SELECT idContrato FROM USERINFO where USERID = @UserId
)";
            var parametros = new DynamicParameters();
            parametros.Add("UserId", userId, DbType.String);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    ConfigVacationModel result = await db.QueryFirstAsync<ConfigVacationModel>(sql, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public ConfigVacationModel GetConfigVac(string userId)
        {
            string sql = SELECT_CONFIG_VACATIONS + @" WHERE C.idContrato in
(
SELECT idContrato FROM USERINFO where USERID = @UserId
)";
            var parametros = new DynamicParameters();
            parametros.Add("UserId", userId, DbType.String);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    ConfigVacationModel result = db.QueryFirst<ConfigVacationModel>(sql, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }


        public async Task LogUpdateConfigVacation(SystemLogModel log)
        {
            log.LogDescr = "Modifica Configuración Vacación";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {

                db.Open();                
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
