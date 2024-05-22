using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.MarcacionTeletrabajo;
using System;

namespace DataAccessLibrary.DataAccessLayer.MarcacionTeletrabajo
{
    public class WebCheckDAL
    {
        private readonly IConfiguration Config;

        public WebCheckDAL(IConfiguration configuration)
        {
            this.Config = configuration;
        }

        public async Task<bool> InsertWebCheckAsync(WebCheckModel remota)
        {
            string sql = @"INSERT INTO WEB_CHECK
(UserId, Estado, Checktime, Latitude, Longitude, Comentario, Fotografia, idSupervisor3)
SELECT UserId, -1, GetDate(), @Lat, @Long, @Coment, @Imagen, supervisor3
FROM Userinfo where USERID = @userid;";
            // VALUES (@userid, -1, GETDATE(), @Lat, @Long, @Coment, @Imagen);";

            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("userid", remota.UserId, DbType.Int16);
            parametros.Add("Lat", remota.Latitude, DbType.Double);
            parametros.Add("Long", remota.Longitude, DbType.Double);
            parametros.Add("Imagen", remota.Fotografia, DbType.Binary);
            parametros.Add("Coment", remota.Comentario, DbType.String);

            using (IDbConnection db = new SqlConnection(Config.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                try
                {
                    db.Open();
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
            }
        }

        private string sqlSelectMarcaciones(string listUsers, string estado)
        {
            // 01   Revisar   negadas
            // 00   Consultar negadas
            // 11   Revisar   aprobadas
            // 10   Consultar aprobadas
            //-11   Revisar   pendientes
            //-20   Consultar pendientes

            // tipoConsulta 0: usuario
            // tipoConsulta 1: admin

            string sql = @"SELECT [LOGID], [WEB_CHECK].[USERID], [CHECKTIME], [Estado], [Latitude], [Longitude], [Comentario], [Fotografia],
            [Name] as NomEmpleado, Badgenumber as Badge
            FROM [dbo].[WEB_CHECK] INNER JOIN USERINFO ON [WEB_CHECK].UserId = USERINFO.UserId
            where checktime between @desde and @hasta
            AND USERINFO.userid in ( " + listUsers + ")\n";

            switch(estado)
            {
                case "01":
                    sql += " and idSupervisor3 = @IdSupervisor AND Estado = 0;";
                    break;
                case "00":
                    sql += " AND Estado = 0;";
                    break;
                case "11":
                    sql += " and idSupervisor3 = @IdSupervisor AND Estado = 1;";
                    break;
                case "10":
                    sql += " AND Estado = 1;";
                    break;
                case "-11":
                    sql += " and idSupervisor3 = @IdSupervisor AND Estado = -1;";
                    break;
                case "-20":
                    sql += " AND Estado = -1;";
                    break;
            }

            return sql;
        }

        public async Task<List<WebCheckDisplayModel>> GetWebChecks(
            string listUsers, string estado, int idSupervisor, DateTime fDesde, DateTime fHasta)
        {
            string sql = sqlSelectMarcaciones(listUsers, estado);

            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdSupervisor", idSupervisor, DbType.Int32);
            parametros.Add("desde", fDesde, DbType.DateTime);
            parametros.Add("hasta", fHasta, DbType.DateTime);
            parametros.Add("Estado", estado, DbType.String);

            IEnumerable<WebCheckDisplayModel> result;
            using (IDbConnection db = new SqlConnection(Config.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    result = await db.QueryAsync<WebCheckDisplayModel>(sql, parametros, commandType: CommandType.Text);
                    return result.ToList();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<bool> AprobarNegarMarcacion(int logId, int estado)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("LogId", logId, DbType.Int32);
            parametros.Add("Estado", estado, DbType.Int32);
            
            string sql = "UPDATE Web_Check Set Estado = @Estado WHERE LOGID = @LogId;";
            bool result;

            using (IDbConnection db = new SqlConnection(Config.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    int iResult = await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                    result = iResult == 1;
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            
        }

        public byte[] RetornaFotoWebCheck(int id)
        {
            using (IDbConnection db = new SqlConnection(Config.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = "SELECT Fotografia FROM WEB_CHECK WHERE LOGID = @LogId";
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("LogId", id, DbType.Int32);
                db.Open();
                try
                {
                    byte[] foto = db.QueryFirstOrDefault<byte[]>(sql, parametros, commandType: CommandType.Text);
                    return foto;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<WebCheckCorreoModel> GetWebCheckCorreo(int id)
        {
            using (IDbConnection db = new SqlConnection(Config.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select U.userid, U.[name] as NomEmpleado, U.CorreoOficina as CorreoEmpleado, 
U.Supervisor3 as idSupervisor, sup3.[name] as NomSupervisor, sup3.CorreoOficina as CorreoSupervisor
from userinfo U inner join userinfo sup3 on u.Supervisor3 = sup3.USERID
where U.USERID = @Id;";
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("Id", id, DbType.Int32);
                db.Open();
                try
                {
                    WebCheckCorreoModel datosCorreo = await db.QueryFirstOrDefaultAsync<WebCheckCorreoModel>(sql, parametros, commandType: CommandType.Text);
                    return datosCorreo;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}
