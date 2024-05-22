using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.HorarioTurnoRegla;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.HorarioTurnoRegla
{
    public class RaHoraExtraDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_RA_HoraExtra = @"SELECT [Id_RegHoraExtra], [Des_RegHoraExtra], 
            case Overtime when 1 then 'true' else 'false' end as OVertime, 
            case RegisterOT when 1 then 'true' else 'false' end as RegisterOT, 
            case HorarioRotativo when 1 then 'true' else 'false' end as HorarioRotativo,
            case FueraDeHorario when 1 then 'true' else 'false' end as FueraDeHorario,
            case HEDiaLaborable when 1 then 'true' else 'false' end as HEDiaLaborable
        from RegHoraExtra";

        public RaHoraExtraDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<RaHoraExtraModel>> GetRaHoraExtraAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                IEnumerable<RaHoraExtraModel> result = await db.QueryAsync<RaHoraExtraModel>(SELECT_RA_HoraExtra);
                return result.ToList();
            }
        }

        public async Task<int> GetRaHoraExtraCountAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                int result = await db.ExecuteScalarAsync<int>("select count(*) from RegHoraExtra");
                return result;
            }
        }

        public async Task AddRaHoraExtraAsync(RaHoraExtraModel regla)
        {
            DynamicParameters parametros = SetParametros(regla);

            string sql = @"INSERT INTO [RegHoraExtra] 
                ([Des_RegHoraExtra],[Overtime],[RegisterOT],[HorarioRotativo],[FueraDeHorario],[HEDiaLaborable])                
                values (@Des_RegHoraExtra,@overtime,@registerOT,@HorarioRotativo,@FueraDeHorario,@HEDiaLaborable);";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();

                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        DynamicParameters SetParametros(RaHoraExtraModel regla)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Des_RegHoraExtra", regla.Des_RegHoraExtra, DbType.String);
            parametros.Add("overtime", regla.Overtime, DbType.Boolean);
            parametros.Add("registerOT", regla.RegisterOT, DbType.Boolean);
            parametros.Add("HorarioRotativo", regla.HorarioRotativo, DbType.Boolean);
            parametros.Add("FueraDeHorario", regla.FueraDeHorario, DbType.Boolean);
            parametros.Add("HEDiaLaborable", regla.HEDiaLaborable, DbType.Boolean);
            return parametros;
        }

        public async Task UpdateRaHoraExtraAsync(RaHoraExtraModel regla)
        {
            DynamicParameters parametros = SetParametros(regla);
            parametros.Add("Id_RegHoraExtra", regla.id_RegHoraExtra, DbType.Int16);

            string sql = @"UPDATE [dbo].[RegHoraExtra]
               SET [Des_RegHoraExtra] = @Des_RegHoraExtra      
                  ,[OverTime] = @overtime
                  ,[RegisterOT] = @registerOT
                  ,[HorarioRotativo] = @HorarioRotativo
                  ,[FueraDeHorario] = @FueraDeHorario
                  ,[HEDiaLaborable] = @HEDiaLaborable                  
                where Id_RegHoraExtra = @Id_RegHoraExtra";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task RemoveRaHoraExtraAsync(int idRegla)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "Delete from [RegHoraExtra] Where id_RegHoraExtra=@BugId;\n";
                await db.ExecuteAsync(sql, new { BugId = idRegla });
            }
        }

        public async Task LogInsertRaHoraExtra(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega regla Hora Extra";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select Des_RegHoraExtra from [RegHoraExtra] 
                                    where id_RegHoraExtra = (select IDENT_CURRENT('RegHoraExtra')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateRaHoraExtra(SystemLogModel log)
        {
            log.LogDescr = "Modifica regla Hora Extra";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select [Des_RegHoraExtra] from [RegHoraExtra] 
                                    where id_RegHoraExtra = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteRaHoraExtra(SystemLogModel log)
        {
            log.LogDescr = "Borra regla Hora Extra";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
