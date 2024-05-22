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
    public class DetalleSolicitudDAL
    {
        public IConfiguration Configuration;

        public DetalleSolicitudDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private string SELECT_SOLICITUD = @"SELECT S.idSolicitud, Estado, 
case Estado when '0' then 'Negado' when 1 then 'Aprobado' when -2 then 'Anulado' else 'Pendiente' end as sEstado, 
U.USERID, U.Badgenumber AS NumCA,  u.[NAME] as NomEmpleado, CASE WHEN U.CorreoOficina is null then '' else U.CorreoOficina end as CorreoEmpleado, 
TP.LeaveName,
cast(s.Desde as smalldatetime) as FIni, cast(s.Hasta as smalldatetime) as FFin, s.Motivo, cast(S.fIngresoSolicitud as smalldatetime) AS FIngreso, Adjunto,
s.idSupervisor1, s.respuesta1, Sup1.[NAME] as NombreS1, case respuesta1 when 1 then 'Aprobado' when 0 then 'Negado' when -2 then 'Anulado' else '' end as sRespuesta1, S.fRespuesta1 as FechaR1, MotivoR1, case when Sup1.CorreoOficina is null then '' else Sup1.CorreoOficina end as CorreoS1,
s.idSupervisor2, s.respuesta2, Sup2.[NAME] as NombreS2, case respuesta2 when 1 then 'Aprobado' when 0 then 'Negado' when -2 then 'Anulado' else '' end as sRespuesta2, S.fRespuesta2 as FechaR2, MotivoR2, case when Sup2.CorreoOficina is null then '' else Sup2.CorreoOficina end as CorreoS2
From (SolicitudPermiso S INNER JOIN USERINFO U on S.idUsuario = U.USERID)
left JOIN LeaveClass TP on s.idTipoPermiso = TP.LeaveId
left JOIN USERINFO Sup1 on Sup1.USERID = S.idSupervisor1
left JOIN USERINFO Sup2 on Sup2.USERID = S.idSupervisor2
WHERE s.idSolicitud = @IdSolicitud";

        public DetalleSolicitudModel GetDetalleSolicitud(string idSolicitud)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdSolicitud", idSolicitud, DbType.String);            

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    DetalleSolicitudModel result = db.QueryFirst<DetalleSolicitudModel>(SELECT_SOLICITUD, parametros, commandType: CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}
