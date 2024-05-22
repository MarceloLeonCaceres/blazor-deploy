using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using DataAccessLibrary.DbAccess;

namespace DataAccessLibrary.DataAccessLayer
{
    public class DepartamentoDAL : IDepartamentoDAL
    {

        public readonly IConfiguration Configuration;
        public DepartamentoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string SELECT_DEPTOS = @"With ConHijos As (
            select distinct deptId as IdConHijos, 'true' as ConHijos from departments 
	            where deptId in 
		            (select distinct SUPDEPTID as IdPadre from DEPARTMENTS where SUPDEPTID > 0)
            )
            select DEPTID, DEPTNAME, SUPDEPTID, ConHijos.ConHijos
            from DEPARTMENTS D left join ConHijos on D.DEPTID = ConHijos.IdConHijos
            order by SUPDEPTID, DEPTID";

        public const string SELECT_DEPTO_LOCAL = @"SELECT DEPTID, DEPTNAME, 0, NULL as ConHijos
            from DEPARTMENTS WHERE DeptId = @IdDepto;";

        public const string SELECT_SUB_DEPTOS = @"WITH SubDepartamentos (deptid, deptName, supDeptId, ConHijos)
            AS
            (
                SELECT deptid, deptName, 0, NULL FROM departments WHERE deptid =  @IdDepto
                UNION ALL                
	            SELECT D.deptid, D.deptName, D.supDeptId, NULL FROM departments D inner join SubDepartamentos Sub on Sub.deptid = D.SUPDEPTID
            )
            SELECT *
            FROM  SubDepartamentos order by supDeptId, deptid";

        public List<DepartamentoModel> GetDeptos()
        {
            string sql = SELECT_DEPTOS;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<DepartamentoModel> result = db.Query<DepartamentoModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<DepartamentoModel> GetDeptos(int otAdmin, int idDepto)
        {
            DynamicParameters parametro = new DynamicParameters();
            parametro.Add("IdDepto", idDepto, DbType.Int32);
            string sql = otAdmin == 1 ? SELECT_DEPTO_LOCAL : SELECT_SUB_DEPTOS;

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<DepartamentoModel> result = db.Query<DepartamentoModel>(sql, parametro, commandType: CommandType.Text);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<List<DepartamentoModel>> GetDeptosAsync()
        {
            string sql = SELECT_DEPTOS;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    IEnumerable<DepartamentoModel> result = await db.QueryAsync<DepartamentoModel>(sql);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }
        public async Task AddDeptoAsync(DepartamentoModel depto)
        {
            DynamicParameters parametros = new DynamicParameters();
            string sql = "Insert Into DEPARTMENTS (DEPTNAME, SUPDEPTID) " +
                    "Values (@DeptName, @SupDeptId);";
            parametros.Add("DeptName", depto.DeptName, DbType.String);
            parametros.Add("SupDeptId", depto.SupDeptId, DbType.String);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task UpdateDeptoAsync(DepartamentoModel depto)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("DeptName", depto.DeptName, DbType.String);
            parametros.Add("DeptId", depto.DeptId, DbType.Int32);
            string sql = "Update DEPARTMENTS Set DeptName=@DeptName " +
                    "where DeptId=@DeptId";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task<int> PermiteRemoverDeptoAsync(int bugid)
        {
            string sqlTieneSubDeptos_Hijos = @"	declare @sub int;
            SET @sub = (SELECT count(*) from departments where SUPDEPTID = @BugId);
            SET @sub = @sub + (SELECT count(*) from USERINFO where DEFAULTDEPTID = @BugId)
            SELECT @sub";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                int tieneSubDeptos_Hijos = await db.ExecuteScalarAsync<int>(sqlTieneSubDeptos_Hijos, new { BugId = bugid });
                return tieneSubDeptos_Hijos;
            }
        }

        public async Task<bool> RemoveDeptoAsync(int bugid)
        {
            string sqlDelete = "Delete from [DEPARTMENTS] Where DeptId = @BugId;\n";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sqlDelete, new { BugId = bugid });
                return true;

            }
        }

        public async Task LogInsertDepto(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega Departamento";

            string sqlAux = @"select DeptName from DEPARTMENTS 
                                    where DeptId = (select IDENT_CURRENT('DEPARTMENTS')); ";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateDepto(SystemLogModel log)
        {
            log.LogDescr = "Modifica Departamento";

            string sqlAux = @"select DeptName from DEPARTMENTS 
                                    where DeptId = " + log.LogTag.ToString();

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteDepto(SystemLogModel log)
        {
            log.LogDescr = "Borra Departamento";
            string sqlAux = @"select DeptName from DEPARTMENTS 
                                    where DeptId = " + log.LogTag.ToString();
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }
    }
}
