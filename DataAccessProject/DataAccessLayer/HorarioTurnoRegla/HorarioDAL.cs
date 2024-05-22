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
    public class HorarioDAL
    {

        public IConfiguration Configuration;

        public const string SELECT_HORARIOS = @"SELECT [SCHCLASSID] AS Id
          ,[SCHNAME] as Nombre
          ,[STARTTIME] as Entrada
          ,[ENDTIME] as Salida
          ,[LATEMINUTES] as GraciaEntrada
          ,[EARLYMINUTES] as GraciaSalida
          ,[CHECKIN]
          ,[CHECKOUT]
          ,[CHECKINTIME1] as IniEntrada
          ,[CHECKINTIME2] as FinEntrada
          ,[CHECKOUTTIME1] as IniSalida
          ,[CHECKOUTTIME2] as FinSalida
          ,[COLOR]
          ,[WorkDay] as DiaTrabajo
          ,[WorkMins] as MinutosBreak
          ,DescuentaBreak as DescuentaAlmuerzo
          ,Tipo
          ,Duracion
          ,[OverTime]
          ,(select Descripcion from BpParam where Parametro = 'H_DescuentaBreak' AND id = H.[DescuentaBreak]) as SDescuentaAlmuerzo
          ,(select Descripcion from BpParam where Parametro = 'H_tipo' AND id = H.[Tipo]) as STipo          
          ,(select Descripcion from BpParam where Parametro = 'H_duracion' AND id = H.[Duracion]) as SDuracion
            FROM SCHCLASS H";
        // La columna Autobind ya no se va a ocupar, en su lugar se ocupa DescuentaBreak

        public const string SELECT_HORARIOS_SIMPLE = @"SELECT [SCHCLASSID] AS Id
          ,[SCHNAME] as Nombre         
            FROM SCHCLASS H";


        public HorarioDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public List<HorarioModel> GetHorarios()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<HorarioModel> result = db.Query<HorarioModel>(SELECT_HORARIOS);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<List<HorarioModel>> GetHorariosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<HorarioModel> result = await db.QueryAsync<HorarioModel>(SELECT_HORARIOS);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task AddHorarioAsync(HorarioModel horario)
        {
            DynamicParameters parametros = ParametrosHorario(horario);

            string sql = @"INSERT INTO [dbo].[SCHCLASS] ([SCHNAME], [STARTTIME], [ENDTIME]
                ,[LATEMINUTES], [EARLYMINUTES], [CHECKIN], [CHECKOUT]
                ,[CHECKINTIME1], [CHECKINTIME2], [CHECKOUTTIME1], [CHECKOUTTIME2]
                ,[WorkDay], [WorkMins], Autobind, Duracion, DescuentaBreak, [Tipo], [OverTime]) " +
                      @"Values (@Nombre, @Entrada, @Salida
                    , @GraciaEntrada, @GraciaSalida, @CheckIn, @CheckOut
                    , @IniEntrada, @FinEntrada, @IniSalida, @FinSalida
                    , @DiaTrabajo, @MinutosBreak, @DescuentaAlmuerzo
                    , @Duracion, @DescuentaAlmuerzo, @Tipo, @OverTime
                    );";
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

        public async Task CreaHorarioAsync()
        {
            HorarioModel Horario = new HorarioModel();
            DynamicParameters parametros = ParametrosHorario(Horario);

            string sql = @"INSERT INTO [dbo].[SCHCLASS] ([SCHNAME], [STARTTIME], [ENDTIME]
                ,[LATEMINUTES], [EARLYMINUTES], [CHECKIN], [CHECKOUT]
                ,[CHECKINTIME1], [CHECKINTIME2], [CHECKOUTTIME1], [CHECKOUTTIME2]
                ,[WorkDay], [WorkMins], Autobind, Duracion, DescuentaBreak, [Tipo], [OverTime]) " +
                      @"Values (@Nombre, @Entrada, @Salida
                    , @GraciaEntrada, @GraciaSalida, @CheckIn, @CheckOut
                    , @IniEntrada, @FinEntrada, @IniSalida, @FinSalida
                    , @DiaTrabajo, @MinutosBreak, @DescuentaAlmuerzo
                    , @Duracion, @DescuentaAlmuerzo, @Tipo, @OverTime
                    );";
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
        private DynamicParameters ParametrosHorario(HorarioModel horario)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Nombre", horario.Nombre, DbType.String);
            parametros.Add("Entrada", horario.Entrada, DbType.DateTime);
            parametros.Add("Salida", horario.Salida, DbType.DateTime);
            parametros.Add("GraciaEntrada", horario.GraciaEntrada, DbType.Int16);
            parametros.Add("GraciaSalida", horario.GraciaSalida, DbType.Int16);
            parametros.Add("CheckIn", horario.CheckIn, DbType.Int16);
            parametros.Add("CheckOut", horario.CheckOut, DbType.Int16);
            parametros.Add("IniEntrada", horario.IniEntrada, DbType.DateTime);
            parametros.Add("FinEntrada", horario.FinEntrada, DbType.DateTime);
            parametros.Add("IniSalida", horario.IniSalida, DbType.DateTime);
            parametros.Add("FinSalida", horario.FinSalida, DbType.DateTime);
            parametros.Add("DiaTrabajo", horario.DiaTrabajo, DbType.Single);
            parametros.Add("MinutosBreak", horario.MinutosBreak, DbType.Single);
            parametros.Add("DescuentaAlmuerzo", horario.DescuentaAlmuerzo, DbType.Int16);
            parametros.Add("Tipo", horario.Tipo, DbType.Int16);
            parametros.Add("Duracion", horario.Duracion, DbType.Int16);
            parametros.Add("OverTime", horario.OverTime, DbType.Int16);
            return parametros;
        }

        public async Task UpdateHorarioAsync(HorarioModel horario)
        {
            DynamicParameters parametros = ParametrosHorario(horario);
            parametros.Add("Id", horario.Id, DbType.Int16);
            string sql = "Update SCHCLASS SET " +
                @"[SCHNAME] =   @Nombre
                  ,[STARTTIME] =    @Entrada
                  ,[ENDTIME] =      @Salida
                  ,[LATEMINUTES] =  @GraciaEntrada
                  ,[EARLYMINUTES] = @GraciaSalida
                  ,[CHECKIN] =      @CheckIn
                  ,[CHECKOUT] =     @CheckOut
                  ,[CHECKINTIME1] = @IniEntrada
                  ,[CHECKINTIME2] = @FinEntrada
                  ,[CHECKOUTTIME1]= @IniSalida
                  ,[CHECKOUTTIME2]= @FinSalida
                  ,[WorkDay] =      @DiaTrabajo
                  ,[WorkMins] =     @MinutosBreak
                  ,Autobind   =     @DescuentaAlmuerzo
                  ,[Tipo] =         @Tipo
                  ,[Duracion] =     @Duracion
                  ,[OverTime] =     @OverTime " +
                    "where SCHCLASSId=@Id";

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

        public async Task RemoveHorarioAsync(int bugid)
        {
            string sql = @"Delete from [SCHCLASS] Where SCHCLASSId = @BugId;
Delete from  NUM_RUN_DEIL where SchClassId = @BugId;
Delete from  USER_TEMP_SCH where SchClassId = @BugId;
Delete from  UserUsedSClasses where SchId = @BugId;";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, new { BugId = bugid });
            }
        }

        public int CountHorariosDependents(int idHorario)
        {
            string sql = @"select 
            (select count(*) from NUM_RUN_DEIL where SchClassId = @IdHorario ) +
            (select count(*) from USER_TEMP_SCH where SchClassId = @IdHorario ) +
            (select count(*) from UserUsedSClasses where SchId = @IdHorario )";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                int result = (int)db.ExecuteScalar(sql, new { IdHorario = idHorario });
                return result;
            }
        }

        public async Task<List<EnumModel>> GetEnumHorariosAsync()
        {
            string sql = @"SELECT [SchClassID] AS id, [SchName] as descripcion FROM [SCHCLASS]";
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

        public async Task LogInsertHorario(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega Horario";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select SCHNAME from SCHCLASS 
                                    where SCHCLASSId = (select IDENT_CURRENT('SCHCLASS')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateHorario(SystemLogModel log)
        {
            log.LogDescr = "Modifica Horario";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select SCHNAME from SCHCLASS 
                                    where SCHCLASSID = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteHorario(SystemLogModel log)
        {
            log.LogDescr = "Borra Horario";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                // string sqlAux = @"select SCHNAME from SCHCLASS 
                //                    where SCHCLASSID = " + log.LogTag.ToString();
                db.Open();
                // log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
