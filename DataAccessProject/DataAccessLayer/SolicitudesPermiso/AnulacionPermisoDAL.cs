using Dapper;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using DataAccessLibrary.Models.SolicitudesPermiso;
using DataAccessProject;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.SolicitudesPermiso
{
    public class AnulacionPermisoDAL
    {
        private readonly IConfiguration Configuration;

        public AnulacionPermisoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private const string ANULA_SOLICITUD_S1 = @"UPDATE SolicitudPermiso
SET Estado = -2, respuesta1 = -2,
fAnulacion = getdate(), MotivoAnulacion = @motivo
where idSolicitud = @idSolicitud AND idSupervisor1 = @idAprobador;";

        private const string ANULA_SOLICITUD_S2 = @"UPDATE SolicitudPermiso
SET Estado = -2, respuesta2 = -2,
fAnulacion = getdate(), MotivoAnulacion = @motivo
where idSolicitud = @idSolicitud AND idSupervisor2 = @idAprobador;";

        private const string ELIMINA_PERMISOS_DEPENDIENTES = @"DELETE FROM USER_SPEDAY 
WHERE idSolicitud = @idSolicitud;";
        public async Task AnulaSolicitud(int idAprobador, int idSolicitud, string motivo, SystemLogModel log)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdAprobador", idAprobador, DbType.Int32);
            parametros.Add("IdSolicitud", idSolicitud, DbType.Int32);
            parametros.Add("motivo", motivo, DbType.String);

            log.LogDescr = "Anula Permiso Aprobado";
            InsertLog insertLog = new InsertLog(log);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(ANULA_SOLICITUD_S1, parametros, commandType: CommandType.Text);
                    await db.ExecuteAsync(ANULA_SOLICITUD_S2, parametros, commandType: CommandType.Text);
                    await db.ExecuteAsync(ELIMINA_PERMISOS_DEPENDIENTES, parametros, commandType: CommandType.Text);
                    await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }

        }
    }
}
