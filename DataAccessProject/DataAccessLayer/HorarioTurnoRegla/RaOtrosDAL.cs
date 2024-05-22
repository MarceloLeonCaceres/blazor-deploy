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
    public class RaOtrosDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_RA_Otros = @"SELECT [Id_RegOtros], [Des_RegOtros], 
            case AtrasoGracia when 1 then 'true' else 'false' end as AtrasoGracia, 
            case CobraMulta when 1 then 'true' else 'false' end as CobraMulta, 
             Descripcion as EnFeriado
        from RegOtros inner join BpParam on Parametro = 'ra_holiday' AND id = holiday";

        // ra_holiday   0   Horario Normal
        // ra_holiday   1   Horario Extra 100%
        // ra_holiday  -1   Día Libre

        public RaOtrosDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<RaOtrosModel>> GetRaOtrosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                IEnumerable<RaOtrosModel> result = await db.QueryAsync<RaOtrosModel>(SELECT_RA_Otros);
                return result.ToList();
            }
        }

        public async Task<int> GetRaOtrosCountAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                int result = await db.ExecuteScalarAsync<int>("select count(*) from RegOtros");
                return result;
            }
        }

        public async Task AddRaOtrosAsync(RaOtrosModel regla)
        {
            DynamicParameters parametros = SetParametros(regla);

            string sql = @"INSERT INTO [RegOtros] 
                ([Des_RegOtros],[atrasoGracia],[cobraMulta],[Holiday])                
                values (@Des_RegOtros,@atrasoGracia,@cobraMulta,@Feriados);";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();

                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        DynamicParameters SetParametros(RaOtrosModel regla)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Des_RegOtros", regla.Des_RegOtros, DbType.String);
            parametros.Add("atrasoGracia", regla.AtrasoGracia, DbType.Boolean);
            parametros.Add("cobraMulta", regla.CobraMulta, DbType.Boolean);
            parametros.Add("Feriados", regla.HOLIDAY, DbType.Int16);

            return parametros;
        }

        public async Task UpdateRaOtrosAsync(RaOtrosModel regla)
        {
            DynamicParameters parametros = SetParametros(regla);
            parametros.Add("Id_RegOtros", regla.id_RegOtros, DbType.Int16);

            string sql = @"UPDATE [dbo].[RegOtros]
               SET [Des_RegOtros] = @Des_RegOtros      
                  ,[AtrasoGracia] = @atrasoGracia
                  ,[CobraMulta] = @cobraMulta
                  ,[HOLIDAY] = @Feriados                  
                where Id_RegOtros = @Id_RegOtros";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task RemoveRaOtrosAsync(int idRegla)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "Delete from [RegOtros] Where id_RegOtros=@BugId;\n";
                await db.ExecuteAsync(sql, new { BugId = idRegla });
            }
        }

        public async Task LogInsertRaOtros(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega regla En Feriados";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select Des_RegOtros from [RegOtros] 
                                    where id_RegOtros = (select IDENT_CURRENT('RegOtros')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateRaOtros(SystemLogModel log)
        {
            log.LogDescr = "Modifica regla En Feriados";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select [Des_RegOtros] from [RegOtros] 
                                    where id_RegOtros = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteRaOtros(SystemLogModel log)
        {
            log.LogDescr = "Borra regla En Feriados";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}




