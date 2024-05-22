using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer
{
    public class EmpresaDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_EMPRESA = @"SELECT TOP 1 
            Id, nombre, ruc, logo, idCiudad, direccion, dia1mes FROM EMPRESA";

        public EmpresaDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public EmpresaModel GetEmpresa()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    EmpresaModel result = new EmpresaModel();
                    result = db.QueryFirstOrDefault<EmpresaModel>(SELECT_EMPRESA, null, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task AddEmpresaAsync(EmpresaModel empresa)
        {
            DynamicParameters parametros = new DynamicParameters();
            string sql = "Insert Into EMPRESA (Nombre, Ruc) " +
                    "Values (@Nombre, @Ruc);";
            parametros.Add("Nombre", empresa.Nombre, DbType.String);
            parametros.Add("Ruc", empresa.Ruc, DbType.String);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task UpdateEmpresaAsync(EmpresaModel empresa)
        {
            DynamicParameters parametros = new DynamicParameters();            
            parametros.Add("Id", empresa.Id, DbType.Int16);
            parametros.Add("Nombre", empresa.Nombre, DbType.String);
            parametros.Add("RUC", empresa.Ruc, DbType.String);
            parametros.Add("IdCiudad", empresa.IdCiudad, DbType.Int16);
            parametros.Add("Direccion", empresa.Direccion, DbType.String);
            parametros.Add("Dia1Mes", empresa.Dia1Mes, DbType.Int16);

            string sql = @"Update EMPRESA Set Nombre = @Nombre,
                RUC = @Ruc,
                idCiudad = @IdCiudad,
                direccion = @Direccion,
                dia1mes = @Dia1Mes
                WHERE Id = @Id";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task UpdateLogoAsync(EmpresaModel empresa)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Logo", empresa.Logo, DbType.Binary);

            string sql = @"Update EMPRESA Set Logo = @Logo";

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

        public async Task BorraLogoAsync()
        {            
            string sql = "Update EMPRESA Set Logo=NULL ";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, null, commandType: CommandType.Text);
            }
        }
        public async Task RemoveEmpresaAsync(int bugid)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                string sql = "Delete from [EMPRESA] Where Id=@BugId;\n";
                await db.ExecuteAsync(sql, new { BugId = bugid });
            }
        }

        public async Task LogInsertEmpresa(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega Empresa";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select Nombre from EMPRESA 
                                    where Id = (select IDENT_CURRENT('EMPRESA')); ";
                db.Open();

                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateEmpresa(SystemLogModel log)
        {
            log.LogDescr = "Modifica datos Empresa";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select Nombre from EMPRESA 
                                    where Id = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed = "Cambia datos empresa";
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateLogo(SystemLogModel log)
        {
            log.LogDescr = "Modifica datos Empresa";
            log.LogDetailed = "Cambia Logo";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();                
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogDeleteEmpresa(SystemLogModel log)
        {
            log.LogDescr = "Borra Empresa";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select Nombre from EMPRESA 
                                    where Id = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }
    }

}
