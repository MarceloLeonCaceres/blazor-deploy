using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer
{
    public class SelectUserModelDAL
    {

        public IConfiguration Configuration;

        public const string SELECT_EMPLEADOS = @"SELECT U.USERID, U.Badgenumber AS Badge, U.[Name] AS NombreEmp, D.DEPTID, D.DEPTNAME as Departamento
            FROM USERINFO U INNER JOIN DEPARTMENTS D ON U.DEFAULTDEPTID = D.DEPTID ";

        private string SELECT_EMPLEADOS_ADMINDEPTO(int idDepto)
        {
            return @"WITH SubDepartamentos (deptid, deptName, supDeptId)
    AS
    (
        SELECT deptid, deptName, 0 FROM departments WHERE deptid = " + idDepto.ToString() + @"
UNION ALL                
	    SELECT D.deptid, D.deptName, D.supDeptId FROM departments D inner join SubDepartamentos Sub on Sub.deptid = D.SUPDEPTID
    )
SELECT U.USERID, U.Badgenumber AS Badge, U.[Name] AS NombreEmp, D.DEPTID, D.DEPTNAME as Departamento
            FROM SubDepartamentos D INNER JOIN USERINFO U ON D.DEPTID = U.DEFAULTDEPTID";
        }

        public SelectUserModelDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<EmpleadoBaseModel>> GetUsuariosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = SELECT_EMPLEADOS + "ORDER BY NombreEmp";
                try
                {
                    IEnumerable<EmpleadoBaseModel> result = await db.QueryAsync<EmpleadoBaseModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<EmpleadoBaseModel> GetUsuarios(int otAdmin, int idDepto, int userId)
        {
            string sql = SELECT_EMPLEADOS;
            switch (otAdmin)
            {
                case 3:
                    sql += " WHERE DefaultDeptId > 0 ";
                    break;
                case 2:
                    sql = SELECT_EMPLEADOS_ADMINDEPTO(idDepto);
                    break;
                case 1:
                    sql += " WHERE DefaultDeptId = " + idDepto.ToString();
                    break;
                case 0:
                    sql += " WHERE UserId = " + userId.ToString();
                    break;
            }
            sql += "\n ORDER BY NombreEmp";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<EmpleadoBaseModel> result = db.Query<EmpleadoBaseModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}
