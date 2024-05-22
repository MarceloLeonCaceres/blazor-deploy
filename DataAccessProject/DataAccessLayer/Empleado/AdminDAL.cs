using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessProject;

namespace DataAccessLibrary.DataAccessLayer
{
    public class AdminDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_ADMIN = @"SELECT userid, badgenumber as Badge, Name as NombreEmp, DefaultDeptId as DeptId, 
OTAdmin, OTPrivAdmin, OTPassword, username, 
esAdministrador, esAprobador, esSupervisor3, esSupervisorXl, att as activo
FROM userinfo
";

        public AdminDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public AdminModel GetAdmin(string user)
        {
            AdminModel resultado = new AdminModel();
            string sql = SELECT_ADMIN +
                        "WHERE (badgenumber = @User OR username = @User) AND DefaultDeptId > 0 ";
            var parametros = new DynamicParameters();
            parametros.Add("User", user, DbType.String);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                try
                {                                
                    db.Open();
                    resultado = db.QueryFirstOrDefault<AdminModel>(sql, parametros, commandType: CommandType.Text);
                    return resultado;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<AdminModel> GetAllAdmins()
        {
            IEnumerable<AdminModel> resultado;
            string sql = SELECT_ADMIN +
                        "WHERE OTAdmin > 0";            

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    resultado = db.Query<AdminModel>(sql, null, commandType: CommandType.Text);
                    return resultado.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<int> GetAdminCountAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                int result = await db.ExecuteScalarAsync<int>("select count(*) from Admin");
                return result;
            }
        }

        public async Task AddAdminAsync(AdminModel bug)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "insert into Admin (NomAdmin) values (@NomAdmin);";
                sql += @"INSERT INTO [dbo].[SystemLog]
                    ([Operator],[LogTime],[MachineAlias],[LogTag],[LogDescr],[LogDetailed])
                         VALUES('-1', (select getdate()), 'Proper Blazor Admin', 0, 'Admin insertada', @NomAdmin)";
                await db.ExecuteAsync(sql, bug);
            }
        }

        public async Task UpdateAdminAsync(AdminModel bug)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync("update Admin set NomAdmin=@NomAdmin where idAdmin=@IdAdmin", bug);
            }
        }

        public async Task UpdatePrivAdminAsync(string userid, string privAdmin)
        {
            var parametros = new DynamicParameters();
            parametros.Add("UserId", userid, DbType.String);
            parametros.Add("PrivAdmin", privAdmin, DbType.String);
            string sql = "Update Userinfo SET OTPrivAdmin = @PrivAdmin where Userid = @UserId";
                        
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task QuitaAdministradores()
        {
            string sql = "UPDATE USERINFO SET OTPrivAdmin = null, OTPassword = null, OTAdmin = 0;";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, null, commandType: CommandType.Text);
            }
        }



        public async Task<int> ConfirmaNombreUsuario(string nombreUsuario)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "SELECT count(*) FROM userinfo WHERE username = @nombreUsuario;";
                int result = await db.ExecuteScalarAsync<int>(sql);
                return result;
            }
        }

    }
}

