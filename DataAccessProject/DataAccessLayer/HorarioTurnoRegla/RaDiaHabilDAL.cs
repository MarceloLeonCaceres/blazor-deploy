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
    public class RaDiaHabilDAL
    {

        public IConfiguration Configuration;

        public const string SELECT_RA_DiaHabil = @"SELECT [Id_RegDiaHabil], [Des_RegDiaHabil], 
            case dia0 when 1 then 'true' else 'false' end as dia0,
            case dia1 when 1 then 'true' else 'false' end as dia1,
            case dia2 when 1 then 'true' else 'false' end as dia2,
            case dia3 when 1 then 'true' else 'false' end as dia3,
            case dia4 when 1 then 'true' else 'false' end as dia4,
            case dia5 when 1 then 'true' else 'false' end as dia5,
            case dia6 when 1 then 'true' else 'false' end as dia6,
            case DiaHabilRotativo when 1 then 'true' else 'false' end as DiaHabilRotativo
        from RegDiaHabil";

        public RaDiaHabilDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<RaDiaHabilModel>> GetRaDiaHabilAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                IEnumerable<RaDiaHabilModel> result = await db.QueryAsync<RaDiaHabilModel>(SELECT_RA_DiaHabil);
                return result.ToList();
            }
        }

        public async Task<int> GetRaDiaHabilCountAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                int result = await db.ExecuteScalarAsync<int>("select count(*) from RegDiaHabil");
                return result;
            }
        }

        public async Task AddRaDiaHabilAsync(RaDiaHabilModel regla)
        {
            DynamicParameters parametros = new DynamicParameters();
            string sql = @"INSERT INTO [RegDiaHabil] 
                ([Des_RegDiaHabil],[dia0],[dia1],[dia2],[dia3],[dia4],[dia5],[dia6],[DiaHabilRotativo])                
                values (@Des_RegDiaHabil,@dia0,@dia1,@dia2,@dia3,@dia4,@dia5,@dia6,@DiaHabilRotativo);";
            parametros.Add("Des_RegDiaHabil", regla.Des_RegDiaHabil, DbType.String);
            parametros.Add("dia0", regla.dia0, DbType.Boolean);
            parametros.Add("dia1", regla.dia1, DbType.Boolean);
            parametros.Add("dia2", regla.dia2, DbType.Boolean);
            parametros.Add("dia3", regla.dia3, DbType.Boolean);
            parametros.Add("dia4", regla.dia4, DbType.Boolean);
            parametros.Add("dia5", regla.dia5, DbType.Boolean);
            parametros.Add("dia6", regla.dia6, DbType.Boolean);
            parametros.Add("DiaHabilRotativo", regla.DiaHabilRotativo, DbType.Boolean);


            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();

                await db.ExecuteAsync(sql, regla);
            }
        }

        public async Task UpdateRaDiaHabilAsync(RaDiaHabilModel regla)
        {
            DynamicParameters parametros = new DynamicParameters();
            string sql = @"UPDATE [dbo].[RegDiaHabil]
               SET [Des_RegDiaHabil] = @Des_RegDiaHabil      
                  ,[dia0] = @dia0
                  ,[dia1] = @dia1
                  ,[dia2] = @dia2
                  ,[dia3] = @dia3
                  ,[dia4] = @dia4
                  ,[dia5] = @dia5
                  ,[dia6] = @dia6
                  ,[DiaHabilRotativo] = @DiaHabilRotativo
                where Id_RegDiaHabil = @Id_RegDiaHabil";
            parametros.Add("Des_RegDiaHabil", regla.Des_RegDiaHabil, DbType.String);
            parametros.Add("dia0", regla.dia0, DbType.Boolean);
            parametros.Add("dia1", regla.dia1, DbType.Boolean);
            parametros.Add("dia2", regla.dia2, DbType.Boolean);
            parametros.Add("dia3", regla.dia3, DbType.Boolean);
            parametros.Add("dia4", regla.dia4, DbType.Boolean);
            parametros.Add("dia5", regla.dia5, DbType.Boolean);
            parametros.Add("dia6", regla.dia6, DbType.Boolean);
            parametros.Add("DiaHabilRotativo", regla.DiaHabilRotativo, DbType.Boolean);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, regla);
            }
        }

        public async Task RemoveRaDiaHabilAsync(int idRegla)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "Delete from [RegDiaHabil] Where id_RegDiaHabil=@BugId;\n";
                await db.ExecuteAsync(sql, new { BugId = idRegla });
            }
        }

        public async Task LogInsertRaDiaHabil(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega regla Día Hábil";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select Des_RegDiaHabil from [RegDiaHabil] 
                                    where id_RegDiaHabil = (select IDENT_CURRENT('RegDiaHabil')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateRaDiaHabil(SystemLogModel log)
        {
            log.LogDescr = "Modifica regla Día Hábil";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select [Des_RegDiaHabil] from [RegDiaHabil] 
                                    where id_RegDiaHabil = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteRaDiaHabil(SystemLogModel log)
        {
            log.LogDescr = "Borra regla Día Hábil";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
