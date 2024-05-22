using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.FeriadoLogTipoPermiso
{
    public class FeriadoDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_FERIADOS = @"SELECT [HOLIDAYID], [HOLIDAYNAME], [STARTTIME], C.*
            FROM [HOLIDAYS] F INNER JOIN CIUDAD C
            ON F.IdCiudad = C.idCiudad
            ORDER BY [STARTTIME] DESC";

        public FeriadoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<FeriadoModel>> GetFeriadosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<FeriadoModel> result = await db.QueryAsync<FeriadoModel>(SELECT_FERIADOS);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task AddFeriadoAsync(FeriadoModel holiday)
        {
            DynamicParameters parametros = new DynamicParameters();
            string sql = "Insert Into HOLIDAYS (HolidayName, StartTime, IdCiudad) " +
                    "Values (@HolidayName, @StartTime, @IdCiudad);";
            parametros.Add("HolidayName", holiday.HolidayName, DbType.String);
            parametros.Add("StartTime", holiday.StartTime, DbType.String);
            parametros.Add("IdCiudad", holiday.IdCiudad, DbType.Int32);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task UpdateFeriadoAsync(FeriadoModel holiday)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("HolidayName", holiday.HolidayName, DbType.String);
            parametros.Add("StartTime", holiday.StartTime, DbType.String);
            parametros.Add("IdCiudad", holiday.IdCiudad, DbType.Int32);
            parametros.Add("HolidayId", holiday.HolidayId, DbType.Int32);
            string sql = "Update HOLIDAYS Set HolidayName=@HolidayName," +
                    "StartTime=@StartTime, IdCiudad = @IdCiudad where HolidayId=@HolidayId";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task RemoveFeriadoAsync(int bugid)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "Delete from [HOLIDAYS] Where HolidayId=@BugId;\n";
                await db.ExecuteAsync(sql, new { BugId = bugid });
            }
        }


        public async Task LogInsertFeriado(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega feriado";
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

        public async Task LogUpdateFeriado(SystemLogModel log)
        {
            log.LogDescr = "Modifica feriado";
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

        public async Task LogDeleteFeriado(SystemLogModel log)
        {
            log.LogDescr = "Borra feriado";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
