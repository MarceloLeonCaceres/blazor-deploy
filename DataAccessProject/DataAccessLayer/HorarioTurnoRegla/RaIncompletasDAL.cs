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
    public class RaIncompletasDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_RA_INCOMPLETAS = @"SELECT [id_RegAsistencia], [Des_RegAsistencia], [JMaximaT], [JminimaT],
            [Intervalo], [NoTIngreso], [NoTSalida], [MultaIngreso], [MultaSalida]
			, (SELECT Descripcion from BpParam where Parametro = 'ra_noRegEnt' AND id = NoTIngreso) as SinIngreso
			, (SELECT Descripcion from BpParam where Parametro = 'ra_noRegSal' AND id = NoTSalida) as SinSalida
            FROM  [RegAsistencia] ";

        /*
            ra_noRegEnt	    0	Llega Puntual
            ra_noRegEnt	    1	Llega Tarde
            ra_noRegEnt	    2	No vino a Trabajar

            ra_noRegSal	    0	Sale Puntual
            ra_noRegSal	    1	Sale Temprano
            ra_noRegSal	    2	No vino a Trabajar
         * */
        public RaIncompletasDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<RaIncompletasModel>> GetRaIncompletasAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                IEnumerable<RaIncompletasModel> result = await db.QueryAsync<RaIncompletasModel>(SELECT_RA_INCOMPLETAS);
                return result.ToList();
            }
        }

        public async Task<int> GetRaIncompletasCountAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                int result = await db.ExecuteScalarAsync<int>("select count(*) from RegAsistencia");
                return result;
            }
        }

        public async Task AddRaIncompletasAsync(RaIncompletasModel regla)
        {
            if (regla.NoTIngreso != 1) regla.MultaIngreso = null;
            if (regla.NoTSalida != 1) regla.MultaSalida = null;
            DynamicParameters parametros = new DynamicParameters();
            string sql = @"Insert into [RegAsistencia] 
                ([Des_RegAsistencia],[JMaximaT],[JminimaT],[Intervalo],[NoTIngreso],[NoTSalida],[MultaIngreso],[MultaSalida])
                values (@Des_RegAsistencia,@JMaximaT,@JminimaT,@Intervalo,@NoTIngreso,@NoTSalida,@MultaIngreso,@MultaSalida);";
            parametros.Add("Des_RegAsistencia", regla.Des_RegAsistencia, DbType.String);
            parametros.Add("JMaximaT", regla.JMaximaT, DbType.Int32);
            parametros.Add("JminimaT", regla.JminimaT, DbType.Int32);
            parametros.Add("Intervalo", regla.Intervalo, DbType.Int32);
            parametros.Add("NoTIngreso", regla.NoTIngreso, DbType.Int32);
            parametros.Add("NoTSalida", regla.NoTSalida, DbType.Int32);
            parametros.Add("MultaIngreso", regla.MultaIngreso, DbType.Int32);
            parametros.Add("MultaSalida", regla.MultaSalida, DbType.Int32);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.ExecuteAsync(sql, regla);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task UpdateRaIncompletasAsync(RaIncompletasModel regla)
        {
            DynamicParameters parametros = new DynamicParameters();
            string sql = @"Update [RegAsistencia] SET [Des_RegAsistencia] = @Des_RegAsistencia
                ,[JMaximaT] = @JMaximaT
                ,[JminimaT] = @JminimaT
                ,[Intervalo] = @Intervalo
                ,[NoTIngreso] = @NoTIngreso
                ,[NoTSalida] = @NoTSalida
                ,[MultaIngreso] = @MultaIngreso
                ,[MultaSalida] = @MultaSalida
                where id_RegAsistencia = @id_RegAsistencia";
            parametros.Add("Des_RegAsistencia", regla.Des_RegAsistencia, DbType.String);
            parametros.Add("JMaximaT", regla.JMaximaT, DbType.Int32);
            parametros.Add("JminimaT", regla.JminimaT, DbType.Int32);
            parametros.Add("Intervalo", regla.Intervalo, DbType.Int32);
            parametros.Add("NoTIngreso", regla.NoTIngreso, DbType.Int32);
            parametros.Add("NoTSalida", regla.NoTSalida, DbType.Int32);
            if (regla.NoTIngreso == 2) regla.MultaIngreso = null;
            parametros.Add("MultaIngreso", regla.MultaIngreso, DbType.Int32);
            if (regla.NoTSalida == 2) regla.MultaSalida = null;
            parametros.Add("MultaSalida", regla.MultaSalida, DbType.Int32);
            try
            {
                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
                {
                    db.Open();
                    await db.ExecuteAsync(sql, regla);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }

        public async Task RemoveRaIncompletasAsync(int idRegla)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "Delete from [RegAsistencia] Where id_RegAsistencia=@BugId;\n";
                await db.ExecuteAsync(sql, new { BugId = idRegla });
            }
        }

        public async Task LogInsertRaIncompletas(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega regla marcaciones incompletas";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"SELECT Des_RegAsistencia from [RegAsistencia] 
                                    where id_RegAsistencia = (select IDENT_CURRENT('RegAsistencia')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateRaIncompletas(SystemLogModel log)
        {
            log.LogDescr = "Modifica regla marcaciones incompletas";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"SELECT [Des_RegAsistencia] from [RegAsistencia] 
                                    WHERE id_RegAsistencia = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteRaIncompletas(SystemLogModel log)
        {
            log.LogDescr = "Borra regla marcaciones incompletas";
            InsertLog insertLog = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
