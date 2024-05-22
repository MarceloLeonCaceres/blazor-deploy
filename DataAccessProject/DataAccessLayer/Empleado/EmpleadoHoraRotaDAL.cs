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
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.Empleado
{
    public class EmpleadoHoraRotaDAL
    {
        public IConfiguration Configuration;

        public EmpleadoHoraRotaDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string SELECT_EMPLEADO_HORAROTA = @"SELECT U.USERID, U.Badgenumber as Badge, U.[Name] as NombreEmp, 
            H.SCHCLASSID as idHorario, H.SCHNAME as nomHorario, EmpRot.[Start] as FechaIni, EmpRot.[End] as FechaFin
            FROM (UserUsedSClasses EmpRot INNER JOIN USERINFO U ON EmpRot.UserId = U.USERID)
            INNER JOIN SCHCLASS H ON EmpRot.SchId = SCHCLASSID ";
        // WHERE Tipo < 24";


        public List<EmpleadoHoraRotaModel> GetHorarios1Emp(string Usuario)
        {
            string sql = SELECT_EMPLEADO_HORAROTA + " WHERE U.USERID IN ( " + Usuario + " )";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();

                    IEnumerable<EmpleadoHoraRotaModel> resultado =  db.Query<EmpleadoHoraRotaModel>(sql, null, commandType: CommandType.Text);
                    return resultado.ToList();
                }
                catch (System.Data.SyntaxErrorException error)
                {
                    throw error;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task SaveEmpleadosHorariosAsync(string csListUsers, string idHorario, DateTime fDesde, DateTime fHasta)
        {
            List<int> Ids = csListUsers.Split(',').Select(int.Parse).ToList();
            string fD = fDesde == null ? "NULL" : fDesde.ToString("dd/MM/yyyy");
            string fH = fHasta == null ? "NULL" : fHasta.ToString("dd/MM/yyyy");
            StringBuilder sbSql = new StringBuilder();
            foreach (int userId in Ids)
            {                
                sbSql.Append(@"INSERT INTO UserUsedSClasses (USERID, SchId, START, [END])
                VALUES (" + userId.ToString() + ", " + idHorario + ", '" + fD + "', '" + fH + "')");
            }

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.QueryAsync(sbSql.ToString(), null, commandType: CommandType.Text);
                }
                catch (System.Data.SyntaxErrorException error)
                {
                    throw error;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task DeleteEmpleadosHorariosAsync(string csListUsers, string idHorario, DateTime fDesde)
        {
            
            string fD = fDesde == null ? "NULL" : fDesde.ToString("dd/MM/yyyy");            

            DynamicParameters parametros = new DynamicParameters();            
            parametros.Add("IdHorario", idHorario, DbType.String);
            parametros.Add("Fini", fDesde, DbType.DateTime);
            string sql = "DELETE FROM UserUsedSClasses WHERE USERID IN ( " + csListUsers + @") AND SchId = @IdHorario";
            

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.QueryAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (System.Data.SyntaxErrorException error)
                {
                    throw error;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }


        public async Task LogInsertHorarioAsignado(SystemLogModel log)
        {
            log.LogDescr = "Asigna Horario Rotativo";
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

        public async Task LogUpdateHorarioAsignado(SystemLogModel log)
        {
            log.LogDescr = "Modifica Horario Asignado";
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

        public async Task LogDeleteHorarioAsignado(SystemLogModel log)
        {
            log.LogDescr = "Quita asignación de Horario";
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
