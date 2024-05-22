using Dapper;
using DataAccessLibrary.Models;
using DataAccessLibrary.Models.Empleado;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.Empleado
{
    public class AsignacionSupervisoresDAL
    {

        public IConfiguration Configuration;

        public AsignacionSupervisoresDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<AsignacionSupervisoresModel>> GetEmpleadosSupervisoresAsync(string csLstUsers)
        {

            string sql = "SELECT U.userid, U.badgenumber as Badge, U.Name as NombreEmp, U.DefaultDeptId as DeptId, " +
                "U.Supervisor1 as idSupervisor1, U.Supervisor2 as idSupervisor2, U.Supervisor3 as idSupervisor3, " + 
                    " SUP1.NAME AS Supervisor1, SUP2.NAME AS Supervisor2, SUP3.NAME AS Supervisor3 " +
                    " FROM (((USERINFO AS U LEFT JOIN TipoEmpleado ON U.idTipoEmpleado = TipoEmpleado.idTipo) " +
                    "LEFT JOIN USERINFO AS SUP1 ON U.Supervisor1 = SUP1.USERID) " +
                    "LEFT JOIN USERINFO AS SUP2 ON U.Supervisor2 = SUP2.USERID) " +
                    "LEFT JOIN USERINFO AS SUP3 ON U.Supervisor3 = SUP3.USERID " +
                    "WHERE U.UserId in (" + csLstUsers + ") " +
                    "ORDER BY U.Name, U.Badgenumber;";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {

                try
                {
                    
                    db.Open();

                    IEnumerable<AsignacionSupervisoresModel> resultado = await db.QueryAsync<AsignacionSupervisoresModel>(sql, null, commandType: CommandType.Text);
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

        public List<AsignacionSupervisoresModel> GetEmpleadosSupervisores(string csLstUsers)
        {

            string sql = @"SELECT U.userid, U.badgenumber as Badge, U.Name as NombreEmp, U.DefaultDeptId as DeptId, Deptname as Departamento, 
                U.Supervisor1 as idSupervisor1, U.Supervisor2 as idSupervisor2, U.Supervisor3 as idSupervisor3,  
                    SUP1.NAME AS Supervisor1, SUP2.NAME AS Supervisor2, SUP3.NAME AS Supervisor3
                    FROM (((USERINFO AS U LEFT JOIN DEPARTMENTS ON U.DEFAULTDEPTID = DEPARTMENTS.DEPTID)  
                    LEFT JOIN USERINFO AS SUP1 ON U.Supervisor1 = SUP1.USERID)  
                    LEFT JOIN USERINFO AS SUP2 ON U.Supervisor2 = SUP2.USERID)  
                    LEFT JOIN USERINFO AS SUP3 ON U.Supervisor3 = SUP3.USERID " + "\n" +
                    "WHERE U.UserId in (" + csLstUsers + ") \n" +
                    "ORDER BY Departamento, U.Name, U.Badgenumber;";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();

                    IEnumerable<AsignacionSupervisoresModel> resultado = db.Query<AsignacionSupervisoresModel>(sql, null, commandType: CommandType.Text);
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


        public async Task SaveEmpleadosSupervisoresAsync(string csLstUsers, int idSup1, int idSup2, int idSup3)
        {
            string sql = @"UPDATE Userinfo set Supervisor1 = " + idSup1.ToString() + 
                @", Supervisor2 = " + idSup2.ToString() + 
                @", Supervisor3 = " + idSup3.ToString() + " WHERE UserId in (" + csLstUsers + ")";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {                    
                    
                    db.Open();
                    await db.ExecuteAsync(sql, null, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task RemoveEmpleadosSupervisoresAsync(string csLstUsers)
        {
            string sql = @"UPDATE Userinfo set Supervisor1 = null, Supervisor2 = null, Supervisor3 = null WHERE UserId in (" + csLstUsers + ")";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.ExecuteAsync(sql, null, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}