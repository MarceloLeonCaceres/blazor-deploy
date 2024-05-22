using Dapper;
using DataAccessLibrary.Models;
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
    public class TurnoDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_TURNOS = @"SELECT [NUM_RUNID] AS Id
          ,[NAME] as Nombre
          ,[STARTDATE] as FechaInicio
          ,[ENDDATE] as FechaFin
          ,[CYCLE] as Ciclo
          ,[UNITS] as Unidades
          ,[HORAS]
          ,CASE UNITS WHEN 1 then 'Semana' when 0 then 'Dia' else 'Mes' end AS SUnidades
            FROM [NUM_RUN]";


        public TurnoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public List<TurnoModel> GetTurnos()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<TurnoModel> result = db.Query<TurnoModel>(SELECT_TURNOS);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<List<TurnoModel>> GetTurnosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<TurnoModel> result = await db.QueryAsync<TurnoModel>(SELECT_TURNOS);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task CreaTurnoAsync(TurnoModel Turno)
        {

            DynamicParameters parametros = Parametros(Turno);

            string sql = @"INSERT INTO [dbo].[NUM_RUN] 
                ( [NAME], [STARTDATE], [ENDDATE],[CYCLE], [UNITS], [HORAS] ) " +
                @"Values 
                (@Nombre, @FechaInicio, @FechaFin, @Ciclo, @Unidades, @Horas );";
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

        private DynamicParameters Parametros(TurnoModel Turno)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Nombre", Turno.Nombre, DbType.String);
            parametros.Add("FechaInicio", Turno.FechaInicio, DbType.DateTime);
            parametros.Add("FechaFin", Turno.FechaFin, DbType.DateTime);
            parametros.Add("Ciclo", Turno.Ciclo, DbType.Int16);
            parametros.Add("Unidades", Turno.Unidades, DbType.Int16);
            parametros.Add("Horas", Turno.Horas, DbType.Int16);
            return parametros;
        }

        public async Task UpdateTurnoAsync(TurnoModel Turno)
        {
            DynamicParameters parametros = Parametros(Turno);
            parametros.Add("Id", Turno.Id, DbType.Int16);
            string sql = "Update NUM_RUN SET " +
                @"[NAME]       =  @Nombre
                  ,[STARTDATE] =  @FechaInicio
                  ,[ENDDATE]   =  @FechaFin
                  ,[CYCLE]     = @Ciclo
                  ,[UNITS]     = @Unidades
                  ,[HORAS]     = @Horas " +
                    "where [NUM_RUNID] = @Id";

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

        public async Task RemoveTurnoAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = @"Delete from [NUM_RUN] Where NUM_RUNId = @BugId;
                Delete from [NUM_RUN_DEIL] Where NUM_RUNID = @BugId;
                Delete from USER_OF_RUN where NUM_OF_RUN_ID = @BugId;";
                await db.ExecuteAsync(sql, new { BugId = id });
            }
        }

        public async Task<List<EnumModel>> GetEnumTurnosAsync()
        {
            string sql = @"SELECT [NUM_RUNID] AS id, [NAME] as descripcion FROM [NUM_RUN]";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<EnumModel> result = await db.QueryAsync<EnumModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<bool> ExisteTurnoAsync(int id)
        {
            string sql = SELECT_TURNOS + " WHERE NUM_RUNId = @BugId;";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                TurnoModel result = null;
                db.Open();               
                result = await db.QueryFirstOrDefaultAsync<TurnoModel>(sql, new { BugId = id});                
                return result != null;                
            }
        }

        #region Logs
        public async Task LogInsertTurno(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega Turno";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select NAME from NUM_RUN 
                                    where NUM_RUNId = (select IDENT_CURRENT('NUM_RUN')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateTurno(SystemLogModel log)
        {
            log.LogDescr = "Modifica Turno";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select NAME from NUM_RUN 
                                    where NUM_RUNID = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteTurno(SystemLogModel log)
        {
            log.LogDescr = "Borra Turno";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select NAME from NUM_RUN 
                                    where NUM_RUNID = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }
        #endregion

    }
}

