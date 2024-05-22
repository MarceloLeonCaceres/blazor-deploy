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
    public class RaSobreTiempoDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_RA_AntesEntDespuesSal = @"SELECT id_RegSobretiempo, [Des_RegSobretiempo], 
            [Tarde], [Temprano], [MinTarde], [MinTemprano]
			, Antes.Descripcion as LlegaTemprano, Despues.Descripcion as SaleTarde
            FROM  RegSobretiempo left join  BpParam Antes on ( Temprano = Antes.id AND Antes.Parametro = 'ra_AntesEnt' )
			left join  BpParam Despues on ( Tarde = Despues.id AND Despues.Parametro = 'ra_DespuesSal' )";

        /*
            'ra_AntesEnt', 1,'Hora Extra'
            'ra_AntesEnt', 4,'Tiempo Trabajado'
            'ra_AntesEnt', 5,'Entrada Puntual'

            'ra_DespuesSal', 6,'Hora Extra'
            'ra_DespuesSal', 9,'Tiempo Trabajado'
            'ra_DespuesSal', 10,'Salida Puntual'
         * */
        public RaSobreTiempoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<RaSobreTiempoModel>> GetRaAntesEntDespuesSalAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                IEnumerable<RaSobreTiempoModel> result = await db.QueryAsync<RaSobreTiempoModel>(SELECT_RA_AntesEntDespuesSal);
                return result.ToList();
            }
        }

        public async Task<int> GetRaAntesEntDespuesSalCountAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                int result = await db.ExecuteScalarAsync<int>("Select count(*) from RegSobretiempo");
                return result;
            }
        }

        public async Task AddRaAntesEntDespuesSalAsync(RaSobreTiempoModel regla)
        {
            if (regla.Temprano == 5) regla.MinTemprano = null;
            if (regla.Tarde == 10) regla.MinTarde = null;
            DynamicParameters parametros = new DynamicParameters();
            string sql = @"INSERT INTO [dbo].[RegSobretiempo] 
                ([Des_RegSobretiempo], [Tarde], [Temprano], [MinTarde], [MinTemprano])
                values (@Des_RegSobretiempo, @Tarde, @Temprano, @MinTarde, @MinTemprano);";
            parametros.Add("Des_RegSobretiempo", regla.Des_RegSobretiempo, DbType.String);
            parametros.Add("Tarde", regla.Tarde, DbType.Int32);
            parametros.Add("Temprano", regla.Temprano, DbType.Int32);
            parametros.Add("MinTarde", regla.MinTarde, DbType.Int32);
            parametros.Add("MinTemprano", regla.MinTemprano, DbType.Int32);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();

                await db.ExecuteAsync(sql, regla);
            }
        }

        public async Task UpdateRaAntesEntDespuesSalAsync(RaSobreTiempoModel regla)
        {
            DynamicParameters parametros = new DynamicParameters();
            string sql = @"Update [RegSobretiempo] SET [Des_RegSobretiempo] = @Des_RegSobretiempo
                ,[Temprano] = @Temprano
                ,[Tarde] = @Tarde
                ,[MinTemprano] = @MinTemprano
                ,[MinTarde] = @MinTarde
                where id_RegSobretiempo = @id_RegSobretiempo";
            parametros.Add("Des_RegSobretiempo", regla.Des_RegSobretiempo, DbType.String);
            parametros.Add("Tarde", regla.Tarde, DbType.Int32);
            parametros.Add("Temprano", regla.Temprano, DbType.Int32);
            parametros.Add("MinTarde", regla.MinTarde, DbType.Int32);
            parametros.Add("MinTemprano", regla.MinTemprano, DbType.Int32);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, regla);
            }
        }

        public async Task RemoveRaAntesEntDespuesSalAsync(int idRegla)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "Delete from [RegSobretiempo] Where id_RegSobretiempo = @BugId;\n";
                await db.ExecuteAsync(sql, new { BugId = idRegla });
            }
        }

        public async Task LogInsertRaAntesEntDespuesSal(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega regla Sobretiempo";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select Des_RegSobretiempo from [RegSobretiempo] 
                                    where id_RegSobretiempo = (select IDENT_CURRENT('RegSobretiempo')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateRaAntesEntDespuesSal(SystemLogModel log)
        {
            log.LogDescr = "Modifica regla Sobretiempo";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select [Des_RegSobretiempo] from [RegSobretiempo] 
                                    where id_RegSobretiempo = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteRaAntesEntDespuesSal(SystemLogModel log)
        {
            log.LogDescr = "Borra regla Sobretiempo";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
