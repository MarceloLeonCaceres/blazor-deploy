using Dapper;
using DataAccessLibrary.Models.Empresa;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.Empresa
{
    public class MailSettingsDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_CORREO = "Select Remitente as FromEmail, Password, Smtp as Host, Port, Ssl, TimeOut From MailParam";

        public MailSettingsDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public MailSettingsDAL()
        {
        }

        public async Task<MailSettingsModel> GetCorreoAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                db.Open();
                MailSettingsModel result = new MailSettingsModel();
                result = await db.QueryFirstOrDefaultAsync<MailSettingsModel>(SELECT_CORREO, null, commandType: CommandType.Text);
                return result;
            }
        }
        public MailSettingsModel GetCorreo()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                MailSettingsModel result = new MailSettingsModel();
                result = db.QueryFirstOrDefault<MailSettingsModel>(SELECT_CORREO, null, commandType: CommandType.Text);
                return result;
            }
        }

        private DynamicParameters ParametrosCorreo(IMailSettingsModel correo)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Remitente", correo.FromEmail, DbType.String);
            parametros.Add("Password", correo.Password, DbType.String);
            parametros.Add("Ssl", correo.Ssl, DbType.Boolean);
            parametros.Add("Smtp", correo.Host, DbType.String);
            parametros.Add("Port", correo.Port, DbType.Int16);
            parametros.Add("TimeOut", correo.TimeOut, DbType.Int32);
            return parametros;
        }

        public async Task<bool> UpdateCorreoAsync(IMailSettingsModel correo, SystemLogModel log)
        {
            string sql = @"Update MailParam set Remitente=@Remitente, 
                Password=@Password,
                Ssl=@Ssl,
                Smtp=@Smtp,
                Port=@Port,
                TimeOut=@TimeOut;";
            DynamicParameters parameters = ParametrosCorreo(correo);

            InsertLog updateLog = LogUpdateCorreo(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.ExecuteAsync(sql, parameters, commandType: CommandType.Text);
                    await db.ExecuteAsync(updateLog.sql, updateLog.parametros, commandType: CommandType.Text);
                    return true;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }                
            }
        }

        public async Task LogEnviaCorreoAsync(SystemLogModel log)
        {

            InsertLog logEnvioCorreo = new InsertLog(log);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();                    
                    await db.ExecuteAsync(logEnvioCorreo.sql, logEnvioCorreo.parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        private InsertLog LogUpdateCorreo(SystemLogModel log)
        {
            log.LogDescr = "Modifica Correo";
            log.LogDetailed = "Modifica Correo";
            InsertLog updateLog = new InsertLog(log);
            return updateLog;
        }

    }
}
