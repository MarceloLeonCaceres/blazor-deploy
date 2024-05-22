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

namespace DataAccessLibrary.DataAccessLayer.Empleado
{
    public class EmpleadoHoraOcasionalDAL
    {
        public IConfiguration Configuration;

        public EmpleadoHoraOcasionalDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string SELECT_EMP_HOR_OCASIONAL = @"SELECT U.USERID, U.Badgenumber as Badge, U.[Name] as NombreEmp, EmpOca.TYPE, EmpOca.Flag, EmpOca.Overtime,
            H.SCHCLASSID as idHorario, H.SCHNAME as nomHorario, EmpOca.[COMETIME] as FechaIni, EmpOca.LEAVETIME as FechaFin
            FROM (USER_TEMP_SCH EmpOca INNER JOIN USERINFO U ON EmpOca.UserId = U.USERID)
            INNER JOIN SCHCLASS H ON EmpOca.SCHCLASSID = H.SCHCLASSID ";


        public List<EmpleadoHoraOcasionalModel> GetHorariosOcasionales1Emp(string Usuario)
        {
            string sql = SELECT_EMP_HOR_OCASIONAL + " WHERE U.USERID IN ( " + Usuario + " )";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();

                    IEnumerable<EmpleadoHoraOcasionalModel> resultado = db.Query<EmpleadoHoraOcasionalModel>(sql, null, commandType: CommandType.Text);
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

        public List<EmpleadoHoraOcasionalModel> GetHorariosOcasionales1Emp(string Usuario, DateTime f1, DateTime f2)
        {
            string rangoFechas = "'" + f1.ToString("dd/MM/yyy") + "' AND '" + f2.ToString("dd/MM/yyyy") + "'";
            string sql = SELECT_EMP_HOR_OCASIONAL + " WHERE U.USERID IN ( " + Usuario + " ) AND ( FechaIni between " + rangoFechas + " );";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();

                    IEnumerable<EmpleadoHoraOcasionalModel> resultado = db.Query<EmpleadoHoraOcasionalModel>(sql, null, commandType: CommandType.Text);
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

        public async Task EliminaHorarioOcasionalEmps(DateTime fIni, DateTime fFin, string csListUsers)
        {
            var preList = csListUsers.Split(',').Select(int.Parse);
            List<int> Ids = preList.ToList();
            string fD = fIni.ToString("dd/MM/yyyy");
            string fH = fFin.AddDays(1).ToString("dd/MM/yyyy");
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"DELETE FROM [USER_TEMP_SCH]
                    WHERE UserId IN ( " + csListUsers.Replace("/", ",") + " ) AND cast(COMETIME AS DATE) BETWEEN '" + fD + "' AND '" + fH + "';\n");            

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

        public async Task InsertaHorarioOcasionalEmps(Dictionary<DateTime, List<HorarioModel>> dicFecHorario, DateTime fIni, DateTime fFin, string csListUsers)
        {
            var preList = csListUsers.Split(',').Select(int.Parse);
            List<int> Ids = preList.ToList();
            string fD =  fIni.ToString("dd/MM/yyyy");
            string fH =  fFin.AddDays(1).ToString("dd/MM/yyyy");            
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"DELETE FROM [USER_TEMP_SCH]
                    WHERE UserId IN ( " + csListUsers.Replace("/", ",") + " ) AND cast(COMETIME AS DATE) BETWEEN '" + fD + "' AND '" + fH + "';\n");
            foreach (int userId in Ids)
            {
                foreach(DateTime fecha in dicFecHorario.Keys)
                {
                    foreach(HorarioModel horario in dicFecHorario[fecha])
                    {
                        sbSql.Append(@"INSERT INTO USER_TEMP_SCH (USERID, SchclassId, ComeTime, LeaveTime, [Type], Flag, Overtime) VALUES (" + 
                            userId.ToString() + ", " + horario.Id + ", '" +
                            fecha.Add(horario.Entrada.TimeOfDay) + "', '" + fecha.Add(horario.Salida.TimeOfDay) + 
                            "', 0, 1, " + horario.OverTime + ");\n");                
                    }                    
                }                
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

        public async Task SaveEmpleadosHorariosOcasionalesAsync(string csListUsers, string idHorario, DateTime fDesde, DateTime fHasta, int tipo, int flag, int overtime)
        {
            List<int> Ids = csListUsers.Split(',').Select(int.Parse).ToList();
            string fD = fDesde == null ? "NULL" : fDesde.ToString("dd/MM/yyyy");
            string fH = fHasta == null ? "NULL" : fHasta.ToString("dd/MM/yyyy");
            StringBuilder sbSql = new StringBuilder();
            foreach (int userId in Ids)
            {
                sbSql.Append(@"INSERT INTO USER_TEMP_SCH (USERID, SchclassId, ComeTime, LeaveTime, Type, Flag, Overtime)
                VALUES (" + userId.ToString() + ", " + idHorario + ", '" + fD + "', '" + fH + "', " + tipo.ToString() + ", " + 
                        flag.ToString() + ", " + overtime.ToString() + " )");
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

        public async Task DeleteEmpleadosHorariosOcasionalesAsync(string csListUsers, string idHorario, DateTime fDesde)
        {

            string fD = fDesde == null ? "NULL" : fDesde.ToString("dd/MM/yyyy");

            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdHorario", idHorario, DbType.String);
            parametros.Add("Fini", fDesde, DbType.DateTime);
            string sql = "DELETE FROM USER_TEMP_SCH WHERE USERID IN ( " + csListUsers + @") AND SchClassId = @IdHorario AND Cometime = @FIni";


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

        public async Task DeleteEmpleadosHorariosOcasionalesAsync(string csListUsers, DateTime fDesde, DateTime fHasta)
        {

            string fD = fDesde == null ? "NULL" : fDesde.ToString("dd/MM/yyyy");
            string fH = fHasta == null ? "NULL" : fHasta.ToString("dd/MM/yyyy");

            DynamicParameters parametros = new DynamicParameters();            
            parametros.Add("Fini", fDesde, DbType.DateTime);
            parametros.Add("Ffin", fHasta, DbType.DateTime);
            string sql = "DELETE FROM USER_TEMP_SCH WHERE USERID IN ( " + csListUsers + @") AND Cometime between @FIni and @Ffin";


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

        public async Task LogInsertHorarioOcasional(SystemLogModel log)
        {
            log.LogDescr = "Asigna Horario Ocasional";
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

        public async Task LogUpdateHorarioOcasional(SystemLogModel log)
        {
            log.LogDescr = "Modifica Horario Ocasional";
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

        public async Task LogDeleteHorarioOcasional(SystemLogModel log)
        {
            log.LogDescr = "Quita Horario Ocasional";
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
