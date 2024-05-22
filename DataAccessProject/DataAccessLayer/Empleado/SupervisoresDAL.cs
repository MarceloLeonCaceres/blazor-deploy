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
    public class SupervisoresDAL
    {
        public IConfiguration Configuration;


        public const string SELECT_SUPERVISORES = @"SELECT userid, badgenumber as Badge, Name as NombreEmp, DefaultDeptId as DeptId, 
esAdministrador, esAprobador, esSupervisor3, esSupervisorXL
";

        public SupervisoresDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private string querySupervisores(int depto, int otAdmin, int tipo)
        {
            string deptos = @"WITH temporal (deptId, deptName, supDeptId)
AS
(
	SELECT deptId, deptName, supDeptId
	FROM DEPARTMENTS
	WHERE DEPTID = " + depto.ToString() + @"
	UNION ALL
	SELECT D.deptId, D.deptName, D.supDeptId
	FROM DEPARTMENTS D inner join temporal ON D.SUPDEPTID = temporal.deptId	
)
";
            string sTipoAprobacion = tipo == 3 ? "esSupervisor3" : "esAprobador";
            string sql = "";
            
            if (otAdmin == 2)
            {
                sql = deptos + SELECT_SUPERVISORES + @"FROM USERINFO INNER JOIN temporal ON USERINFO.DEFAULTDEPTID = temporal.deptId
WHERE " + sTipoAprobacion + " = 1";
            }
            else if (otAdmin == 3)
            {
                sql = SELECT_SUPERVISORES + "FROM USERINFO WHERE " + sTipoAprobacion + " = 1 and DefaultDeptId > 0";
            }
            else if (otAdmin == 1)
            {
                sql = SELECT_SUPERVISORES + "FROM USERINFO WHERE " + sTipoAprobacion + " = 1 and DefaultDeptId = " + depto.ToString();
            }
                        
            return sql;
        }

        public List<SupervisorModel> GetSupervisores(int depto, int otAdmin, int tipo)
        {
            string sql = querySupervisores(depto, otAdmin, tipo);
            IEnumerable<SupervisorModel> result;

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    result = db.Query<SupervisorModel>(sql);
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
