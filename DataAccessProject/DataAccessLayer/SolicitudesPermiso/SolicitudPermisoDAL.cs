using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.SolicitudesPermiso;
using System;
using System.Text;

namespace DataAccessLibrary.DataAccessLayer.SolicitudesPermiso
{
    public class SolicitudPermisoDAL
    {
        public IConfiguration Configuration;

        public SolicitudPermisoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private const string INSERT_SOLICITUD = @"INSERT INTO [dbo].[SolicitudPermiso]
([idUsuario], [idTipoPermiso], [Desde], [Hasta], [Estado], [Motivo], [fIngresoSolicitud]
,[idSupervisor1], [idSupervisor2], [Adjunto])
     VALUES
(@IdUsuario, @IdTipoPermiso, @Desde, @Hasta, @Estado, @Motivo, @FIngresoSolicitud
,@IdSupervisor1, @IdSupervisor2, @Adjunto);
SELECT CAST(SCOPE_IDENTITY() as int);";

        private const string GET_SOLICITUD = @"SELECT [idSolicitud],[idUsuario],[idTipoPermiso],[Desde],[Hasta],[Estado],[Motivo],[fIngresoSolicitud]
,[idSupervisor1],[respuesta1],[fRespuesta1],[MotivoR1]
,[idSupervisor2],[respuesta2],[fRespuesta2],[MotivoR2],[Adjunto]
  FROM [dbo].[SolicitudPermiso]
where idSolicitud = @IdSolicitud";

        private const string GET_SOLICITUD_PENDIENTE = @"SELECT	S.idSolicitud, U.Badgenumber, U.[Name], S.Desde, S.Hasta, S.Motivo, S.fIngresoSolicitud, S.fRespuesta1, fRespuesta2, Adjunto
FROM	SolicitudPermiso S INNER JOIN USERINFO U ON S.idUsuario = U.USERID
WHERE	U.Badgenumber = 1 AND Estado = -1 
ORDER BY U.[Name], Desde, Hasta;";

        public int InsertSolicitudPermiso(SolicitudPermisoModel solicitud)
        {
            DynamicParameters parametros = Parametros(solicitud);              
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();                
                try
                {                    
                    var result = db.Query<int>(INSERT_SOLICITUD, parametros, commandType: CommandType.Text);                    
                    return result.Single();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public SolicitudPermisoModel GetSolicitud(int idSolicitud)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdSolicitud", idSolicitud, DbType.Int32);

            SolicitudPermisoModel solicitud;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    solicitud = db.QueryFirst<SolicitudPermisoModel>(GET_SOLICITUD, parametros, commandType: CommandType.Text);
                    return solicitud;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        private DynamicParameters Parametros(SolicitudPermisoModel solicitud)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdUsuario", solicitud.UserId, DbType.Int32);
            parametros.Add("IdTipoPermiso", solicitud.IdTipo, DbType.Int32);            
            parametros.Add("IdSupervisor1", solicitud.IdSupervisor1, DbType.Int32);
            parametros.Add("IdSupervisor2", solicitud.IdSupervisor2, DbType.Int32);
            parametros.Add("Estado", solicitud.Estado, DbType.Int16);
            parametros.Add("Respuesta1", solicitud.Respuesta1, DbType.Int16);
            parametros.Add("Respuesta2", solicitud.Respuesta2, DbType.Int16);            
            parametros.Add("Desde", solicitud.FIni, DbType.DateTime);
            parametros.Add("Hasta", solicitud.FFin, DbType.DateTime);
            parametros.Add("FIngresoSolicitud", solicitud.FIngreso, DbType.DateTime);
            parametros.Add("FR1", solicitud.FechaR1, DbType.DateTime);
            parametros.Add("FR2", solicitud.FechaR2, DbType.DateTime);
            parametros.Add("Motivo", solicitud.Motivo, DbType.String);
            parametros.Add("MotivoR1", solicitud.MotivoR1, DbType.String);
            parametros.Add("MotivoR1", solicitud.MotivoR1, DbType.String);
            parametros.Add("Adjunto", solicitud.Adjunto, DbType.String);
            
            return parametros;
        }

        public bool ValidaTraslapeSolicitudes(SolicitudPermisoModel solicitud)
        {
            bool resultado = false;
            string cuentaSolicitudesPendientes = @"SET DATEFORMAT DMY;
SELECT COUNT(*)
FROM SolicitudPermiso S INNER JOIN USERINFO U ON S.IdUsuario = U.UserId
WHERE Estado <> 0 AND (
   (S.Desde between '" + solicitud.FIni.Value.ToString("dd/MM/yyyy HH:mm") + "' and '" + solicitud.FFin.Value.ToString("dd/MM/yyyy HH:mm") + @"')
or (S.Hasta between '" + solicitud.FIni.Value.ToString("dd/MM/yyyy HH:mm") + "' and '" + solicitud.FFin.Value.ToString("dd/MM/yyyy HH:mm") + @"')
or ('" + solicitud.FIni.Value.ToString("dd/MM/yyyy HH:mm") + @"' between S.Desde AND S.Hasta )
or ('" + solicitud.FFin.Value.ToString("dd/MM/yyyy HH:mm") + @"' between S.Desde AND S.Hasta )
) AND U.UserId = " + solicitud.UserId;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {

                db.Open();
                try
                {
                    var result =  db.ExecuteScalar<int>(cuentaSolicitudesPendientes, null, commandType: CommandType.Text);
                    return (uint)result == 0;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            return resultado;
        }

        public bool ValidaTraslapePermisos(SolicitudPermisoModel solicitud)
        {
            bool resultado = false;
            string cuentaPermisosAprobados = @"SET DATEFORMAT DMY;
SELECT COUNT(*)
FROM USER_SPEDAY P INNER JOIN USERINFO U ON P.USERID = U.USERID
WHERE (
   (P.STARTSPECDAY between '" + solicitud.FIni.Value.ToString("dd/MM/yyyy HH:mm") + "' and '" + solicitud.FFin.Value.ToString("dd/MM/yyyy HH:mm") + @"')
or (P.ENDSPECDAY   between '" + solicitud.FIni.Value.ToString("dd/MM/yyyy HH:mm") + "' and '" + solicitud.FFin.Value.ToString("dd/MM/yyyy HH:mm") + @"')
or ('" + solicitud.FIni.Value.ToString("dd/MM/yyyy HH:mm") + @"' between P.STARTSPECDAY AND P.ENDSPECDAY )
or ('" + solicitud.FFin.Value.ToString("dd/MM/yyyy HH:mm") + @"' between P.STARTSPECDAY AND P.ENDSPECDAY )
) AND U.UserId = " + solicitud.UserId;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {

                db.Open();
                try
                {
                    var result = db.ExecuteScalar<int>(cuentaPermisosAprobados, null, commandType: CommandType.Text);
                    return (uint)result == 0;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            return resultado;

        }

        public bool UpdateAdjunto(string idSolicitud, string nombreArchivo)
        {            
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdSolicitud", idSolicitud, DbType.String);
            parametros.Add("NombrePermiso", nombreArchivo, DbType.String);
            
            string sql = @"UPDATE SolicitudPermiso SET Adjunto = @NombrePermiso
WHERE idSolicitud = @IdSolicitud";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    var result = db.ExecuteScalar<int>(sql, parametros, commandType: CommandType.Text);
                    return (uint)result == 1;
                }
                catch (System.Exception ex)
                {
                    return false;
                    throw ex;
                }
            }            
        }

    }
}
