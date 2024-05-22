using Dapper;
using DataAccessLibrary.Models;
using DataAccessLibrary.Models.Empleado;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text;

namespace DataAccessLibrary.DataAccessLayer.Empleado
{
    public class EmpleadoTurnoRotaOcaDAL
    {

        public IConfiguration Configuration;

        public EmpleadoTurnoRotaOcaDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string SELECT_FILA_EMP_TURNO = @"SELECT USERID, Badge, NombreEmp, idTurno, NomTurno, FechaIni, FechaFin
    FROM ( " + SELECT_EMPLEADO_TURNO;
        

        public const string SELECT_EMPLEADO_TURNO = @"SELECT USERINFO.USERID, USERINFO.BADGENUMBER as Badge, USERINFO.[NAME] as NombreEmp, 
        USER_OF_RUN.NUM_OF_RUN_ID AS IdTurno, NUM_RUN.[NAME] AS[NomTurno], USER_OF_RUN.STARTDATE AS FechaIni, USER_OF_RUN.ENDDATE AS FechaFin,
        ROW_NUMBER() over(PARTITION by USERINFO.userid order by USER_OF_RUN.startdate) as fila
        from USERINFO left JOIN USER_OF_RUN ON USER_OF_RUN.USERID = USERINFO.USERID
            left JOIN NUM_RUN ON NUM_RUN.NUM_RUNID = USER_OF_RUN.NUM_OF_RUN_ID ";        

        public async Task<List<EmpleadoTurnoRotaOcaModel>> GetEmpleadosTurnosAsync(string csLstUsers)
        {            
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();

                    IEnumerable<EmpleadoTurnoRotaOcaModel> resultado = await db.QueryAsync<EmpleadoTurnoRotaOcaModel>(SELECT_EMPLEADO_TURNO, null, commandType: CommandType.Text);
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

        public List<EmpleadoTurnoRotaOcaModel> GetEmpleadosTurnos(string csLstUsers)
        {
            
            string sql = SELECT_FILA_EMP_TURNO + " WHERE USERINFO.USERID IN ( " + csLstUsers + @" )
                ) fila
                WHERE fila = 1 ORDER BY NombreEmp;";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    IEnumerable<EmpleadoTurnoRotaOcaModel> resultado = db.Query<EmpleadoTurnoRotaOcaModel>(sql, null, commandType: CommandType.Text);
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

        public List<EmpleadoTurnoRotaOcaModel> GetTurnos1Emp(string userId)
        {
            string sql = SELECT_EMPLEADO_TURNO + " WHERE USERINFO.USERID IN ( " + userId + @" );";                

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    IEnumerable<EmpleadoTurnoRotaOcaModel> resultado = db.Query<EmpleadoTurnoRotaOcaModel>(sql, null, commandType: CommandType.Text);
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
        

        

        public async Task RemoveEmpleadosTurnosAsync(string csListUsers)
        {
            
            string sql = @"DELETE FROM USER_OF_RUN 
                WHERE USERID IN (" + csListUsers + ");";                           

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.QueryAsync(sql.ToString(), null, commandType: CommandType.Text);
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
    }
}
