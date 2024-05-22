using Dapper;
using DataAccessLibrary.Models.Empleado;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text;
using DataAccessLibrary.Models.HorarioTurnoRegla;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.HorarioTurnoRegla
{
    public class TurnoRangoAsignadoDAL
    {
        public IConfiguration Configuration;

        public TurnoRangoAsignadoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string SELECT_TURNOS_ASIGNADOS_EMPLEADO = @"SELECT NUM_OF_RUN_ID AS IdTurno, [Name] as NomTurno, UR.STARTDATE as FechaIni, UR.ENDDATE as FechaFin
            FROM USER_OF_RUN UR INNER JOIN NUM_RUN on UR.NUM_OF_RUN_ID = NUM_RUN.NUM_RUNID
            WHERE USERID = @UserId
            ORDER BY UR.STARTDATE";

        public async Task<List<TurnoRangoAsignadoModel>> GetTurnosAsignadosAsync(string sUserid)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    DynamicParameters parametros = new DynamicParameters();
                    parametros.Add("UserId", sUserid, DbType.String);
                    db.Open();

                    IEnumerable<TurnoRangoAsignadoModel> resultado = await db.QueryAsync<TurnoRangoAsignadoModel>(SELECT_TURNOS_ASIGNADOS_EMPLEADO, parametros, commandType: CommandType.Text);
                    return resultado.ToList();
                }
                catch (SyntaxErrorException error)
                {
                    throw error;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<TurnoRangoAsignadoModel> GetTurnosAsignados(string sUserid)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    DynamicParameters parametros = new DynamicParameters();
                    parametros.Add("UserId", sUserid, DbType.String);
                    db.Open();

                    IEnumerable<TurnoRangoAsignadoModel> resultado = db.Query<TurnoRangoAsignadoModel>(SELECT_TURNOS_ASIGNADOS_EMPLEADO, parametros, commandType: CommandType.Text);
                    return resultado.ToList();
                }
                catch (SyntaxErrorException error)
                {
                    throw error;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task SaveEmpleadosTurnosAsync(string csListUsers, string idTurno, DateTime fDesde, DateTime fHasta)
        {
            List<int> Ids = csListUsers.Split(',').Select(int.Parse).ToList();
            StringBuilder sbSql = new StringBuilder();
            foreach (int userId in Ids)
            {
                sbSql.Append(@"INSERT INTO USER_OF_RUN (USERID, NUM_OF_RUN_ID, STARTDATE, ENDDATE)
                VALUES (" + userId.ToString() + ", " + idTurno + ", '" + fDesde.ToString("dd/MM/yyyy") + "', '" + fHasta.ToString("dd/MM/yyyy") + "')");
            }

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.QueryAsync(sbSql.ToString(), null, commandType: CommandType.Text);
                }
                catch (SyntaxErrorException error)
                {
                    throw error;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task DeleteEmpleadosTurnosAsync(string csListUsers, string idTurno, DateTime fDesde)
        {

            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdTurno", idTurno, DbType.String);
            parametros.Add("Fini", fDesde, DbType.DateTime);


            string sql = "DELETE FROM USER_OF_RUN WHERE USERID IN ( " + csListUsers + @" ) AND 
                NUM_OF_RUN_ID = @IdTurno AND STARTDATE = @Fini;";


            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.QueryAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (SyntaxErrorException error)
                {
                    throw error;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        public async Task LogInsertTurnoAsignado(SystemLogModel log)
        {
            log.LogDescr = "Asigna Turno Fijo";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select [NAME] from USERINFO
                                    where UserId = " + log.LogTag.ToString();
                db.Open();

                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateTurnoAsignado(SystemLogModel log)
        {
            log.LogDescr = "Modifica Turno Asignado";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select [NAME] from USERINFO
                                    where UserId = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteTurnoAsignado(SystemLogModel log)
        {
            log.LogDescr = "Quita asignación de turno";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select [NAME] from USERINFO
                                    where UserId = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux) + " -> " + log.LogDetailed;
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }


    }
}
