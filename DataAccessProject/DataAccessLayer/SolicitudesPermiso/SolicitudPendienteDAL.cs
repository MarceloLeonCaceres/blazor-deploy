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
using DataAccessLibrary.Models.Reports;
using DataAccessLibrary.Models.Auxiliares;

namespace DataAccessLibrary.DataAccessLayer.SolicitudesPermiso
{
    public class SolicitudPendienteDAL
    {
        public IConfiguration Configuration;

        public SolicitudPendienteDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        private string sqlSolicitudes(string sListUsers, string estado, int idConsultor)
        {            
            // 01   Revisar   negadas
            // 00   Consultar negadas
            // 11   Revisar   aprobadas
            // 10   Consultar aprobadas
            //-11   Revisar   pendientes
            //-20   Consultar pendientes

            // tipoConsulta 0: usuario
            // tipoConsulta 1: admin


            string sql = @"SET DateFormat dmy;
SELECT u.USERID, u.BADGENUMBER as NumCA, u.NAME as NomEmpleado, 
S.idSolicitud, S.idTipoPermiso as idTipo, s.Desde as FIni, s.Hasta as FFin, s.Motivo, S.fIngresoSolicitud AS FIngreso, 
TP.LeaveName as TipoPermiso,
Sup1.NAME as NomSupervisor1, S.fRespuesta1 as FechaR1, Sup2.NAME as NomSupervisor2, S.fRespuesta2 as FechaR2, S.Adjunto
From (SolicitudPermiso S INNER JOIN USERINFO U on S.idUsuario = U.USERID)
left JOIN LeaveClass TP on s.idTipoPermiso = TP.LeaveId
left JOIN USERINFO Sup1 on Sup1.USERID = S.idSupervisor1
left JOIN USERINFO Sup2 on Sup2.USERID = S.idSupervisor2
WHERE ( (S.Desde between @fDesde and @fHasta) or (S.Hasta between @fDesde and @fHasta) ) 
AND U.userid in ( " + sListUsers + ")\n";

            switch(estado)
            {
                case "01":
                    sql += "AND ( Estado = 0 or Estado = -2 ) \n";
                    sql += @"AND (  ( idSupervisor1 = @idAprobador AND S.respuesta1 = 0) or  
                                    ( idSupervisor2 = @idAprobador AND S.respuesta2 = 0 )       )";
                    break;
                case "00":
                    sql += "AND ( Estado = 0 or Estado = -2 )\n";
                    break;
                case "11":
                    sql += "AND Estado = 1\n";
                    sql += @"AND (  ( idSupervisor1 = @idAprobador AND S.respuesta1 = 1) or  
                                    ( idSupervisor2 = @idAprobador AND S.respuesta2 = 1 )       )";
                    break;
                case "10":
                    sql += "AND Estado = 1\n";
                    break;
                case "-11":
                    sql += "AND Estado = -1\n";
                    sql += @"AND (  ( idSupervisor1 = @idAprobador AND S.respuesta1 is Null) or  
                                    ( idSupervisor2 = @idAprobador AND S.respuesta1 = 1 )       )";
                    break;
                case "-20":
                    sql += "AND Estado = -1\n";
                    break;
            }
          

            return sql;          
        }
        
        private string sqlCompara_Permisos_Solicitudes(string sListUsers)
        {
            string sql = @"WITH ResumenPermisos AS (
SELECT idSolicitud as idPermiso_Sol, USERID, DATEID, YUANYING, cast([DATE] as date) as fechaIngreso, COUNT(*) as numPermisos, MIN(startspecday) as PermisoDesde, MAX(endSPECDAY) AS PermisoHasta
FROM USER_SPEDAY
WHERE ( (startspecday between @fDesde and @fHasta) or (endSPECDAY between @fDesde and @fHasta) ) 
    and USERID in ( " + sListUsers + @")
    GROUP by idSolicitud, USERID, DATEID, YUANYING, cast([DATE] as date)
)
SELECT
    P.*,
    CASE WHEN P.numPermisos = datediff(day, cast(Desde as date), cast(Hasta as date)) + 1  THEN '=' 

            WHEN P.numPermisos<datediff(day, cast(Desde as date), cast(Hasta as date)) + 1  THEN '<' ELSE '>' END AS Comparacion,
	S.idSolicitud, datediff(day, cast(Desde as date), cast(Hasta as date)) + 1 as diasSolicitados,[Desde],[Hasta]
	, [idUsuario], U.[Name] as Empleado, [idTipoPermiso],case [Estado] when 1 then 'Aprobado' when 0 then 'Negado' else 'Pendiente' END as Estado
	,[Motivo],[fIngresoSolicitud],[idSupervisor1],[respuesta1],[fRespuesta1],[MotivoR1],[idSupervisor2],[respuesta2],[fRespuesta2],[MotivoR2],[Adjunto]
FROM (SolicitudPermiso S inner join USERINFO U on S.idUsuario = U.USERID)

    FULL OUTER JOIN ResumenPermisos P on S.idSolicitud = P.idPermiso_Sol
WHERE S.Estado = 1 AND(([Desde] between @fDesde and @fHasta) or([Hasta] between @fDesde and @fHasta) ) 
    and [idUsuario] in ( " + sListUsers + @")
ORDER BY S.idSolicitud";

            return sql;
            
        }        

        public async Task<List<SolicitudPendienteModel>> GetSolicitudesPendientes(string listUsers, DateTime fDesde, DateTime fHasta, string estado, int idConsultor)
        {
            DynamicParameters parametros = new DynamicParameters();            
            parametros.Add("Estado", estado, DbType.String);
            parametros.Add("fDesde", fDesde, DbType.DateTime);
            parametros.Add("fHasta", fHasta, DbType.DateTime);
            parametros.Add("idAprobador", idConsultor, DbType.Int32);
            List<SolicitudPendienteModel> solicitudes;

            string SELECT_SOLICITUDES = sqlSolicitudes(listUsers, estado, idConsultor);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();                
                try
                {
                    IEnumerable<SolicitudPendienteModel> result = 
                        await db.QueryAsync<SolicitudPendienteModel>(SELECT_SOLICITUDES, parametros, commandType: CommandType.Text);
                    solicitudes = result.ToList();                    
                    return solicitudes;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task NiegaSolicitudes(string sListaSolicitudes, int idAprobador, string motivoNegacion)
        {
            List<string> queries = lsNiegaSolicitudes(sListaSolicitudes);
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("idAprobador", idAprobador, DbType.Int32);
            parametros.Add("motivo", motivoNegacion, DbType.String);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    foreach(string query in queries)
                    {
                        await db.ExecuteAsync(query, parametros, commandType: CommandType.Text);
                    }                    
                    return;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<bool> InsertPermisosAprobados(string sListaSolicitudes, int idAprobador)
        {

            //            string sql = @"UPDATE SolicitudPermiso
            //SET Respuesta2 = 1, fRespuesta2 = GetDate(), Estado = 1,
            //    Respuesta1 = 1, fRespuesta1 = CASE WHEN idSupervisor1 = idSupervisor2 THEN GetDate() ELSE fRespuesta1 END
            //WHERE idSolicitud in ( " + sListaSolicitudes + ");";

            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("idAprobador", idAprobador, DbType.Int32);

            string GET_SOLICITUDES_x_APROBAR = @"SELECT [idUsuario] as UserId, [idSolicitud], [idTipoPermiso] as IdTipo, [Desde] as FIni, [Hasta] as FFin, [Motivo]
  FROM [dbo].[SolicitudPermiso]
where Estado = 1 AND idSupervisor2 = @idAprobador
AND idSolicitud in ( " + sListaSolicitudes + @")";


            string sInsertaPermiso = "";           

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    // await db.ExecuteAsync(sql, null, commandType: CommandType.Text);
                    IEnumerable<PermisoModel> solicitudesAprobadas = 
                        await db.QueryAsync<PermisoModel>(GET_SOLICITUDES_x_APROBAR, parametros, commandType: CommandType.Text);
                    
                    foreach( PermisoModel PporIngresar in solicitudesAprobadas.ToList())
                    {
                        PermisoComplejo pComplejo = new PermisoComplejo(PporIngresar);
                        foreach(RangoHorario fechas in pComplejo.LstRangoHorarios){
                            sInsertaPermiso = @"Insert INTO USER_SPEDAY
(USERID,STARTSPECDAY,ENDSPECDAY,DATEID,YUANYING,[DATE], idSolicitud)
VALUES( " + pComplejo.UserId.ToString() + ", '" + fechas.Ini.ToString("dd/MM/yyyy HH:mm") + "', '" + fechas.Fin.ToString("dd/MM/yyyy HH:mm") + "', " +
pComplejo.IdTipo.ToString() + ", '" + pComplejo.Motivo + "', '" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "', " + pComplejo.IdSolicitud.ToString() + ");";
                            await db.ExecuteAsync(sInsertaPermiso, null, commandType: CommandType.Text);
                        }
                        
                    };
                    return true;
                }
                catch (System.Exception ex)
                {                                        
                    throw ex;
                }
                
            }
        }

        public async Task<bool> ApruebaSolicitudes(string sListaSolicitudes, int idAprobador, string motivoAprobacion)
        {

            List<string> queries = lsApruebaSolicitudes(sListaSolicitudes);
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("idAprobador", idAprobador, DbType.Int32);
            parametros.Add("motivo", motivoAprobacion, DbType.String);

            bool finalResult;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    foreach(string query in queries)
                    {
                        await db.ExecuteAsync(query, parametros, commandType: CommandType.Text);
                    }

                    finalResult = await InsertPermisosAprobados(sListaSolicitudes, idAprobador);
                    return finalResult;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        private List<string> lsNiegaSolicitudes(string sListaSolicitudes)
        {
            List<string> lstQueries = new List<string>();
            string update = "UPDATE SolicitudPermiso SET Estado = 0, \n";
            string where = "\n WHERE idSolicitud in ( " + sListaSolicitudes + ") \n";

            string negadorUnico = update +
                "respuesta1 = 0, fRespuesta1 = getdate(), MotivoR1 = @motivo, respuesta2 = 0, fRespuesta2 = GETDATE(), MotivoR2 = @motivo"
                + where +
                "AND (   idSupervisor1 = @idAprobador and idSupervisor2 = @idAprobador and respuesta1 is null	)";

            string negador1 = update +
                "respuesta1 = 0, fRespuesta1 = getdate(), MotivoR1 = @motivo"
                + where +
                "AND  idSupervisor1 = @idAprobador and idSupervisor2 <> @idAprobador and respuesta1 is null";

            string negador2 = update +
                "respuesta2 = 0, fRespuesta2 = getdate(), MotivoR2= @motivo"
                + where +
                "AND  idSupervisor1 <> @idAprobador and idSupervisor2 = @idAprobador and respuesta1 = 1";

            lstQueries.Add(negadorUnico);
            lstQueries.Add(negador1);
            lstQueries.Add(negador2);

            return lstQueries;

        }

        private List<string> lsApruebaSolicitudes(string sListaSolicitudes)
        {
            List<string> lstQueries = new List<string>();
            string update = "UPDATE SolicitudPermiso SET \n";
            string where = "\n WHERE idSolicitud in ( " + sListaSolicitudes + ") \n";

            string aprobadorUnico = update +
                "Estado = 1, respuesta1 = 1, fRespuesta1 = getdate(), MotivoR1 = @motivo, respuesta2 = 1, fRespuesta2 = GETDATE(), MotivoR2 = @motivo"
                + where +
                "AND (   idSupervisor1 = @idAprobador and idSupervisor2 = @idAprobador and respuesta1 is null	)";

            string aprobador1 = update +
                "respuesta1 = 1, fRespuesta1 = getdate(), MotivoR1 = @motivo"
                + where +
                "AND  idSupervisor1 = @idAprobador and idSupervisor2 <> @idAprobador and respuesta1 is null";

            string aprobador2 = update +
                "Estado = 1, respuesta2 = 1, fRespuesta2 = getdate(), MotivoR2 = @motivo"
                + where +
                "AND  idSupervisor1 <> @idAprobador and idSupervisor2 = @idAprobador and respuesta1 = 1";

            lstQueries.Add(aprobadorUnico);
            lstQueries.Add(aprobador1);
            lstQueries.Add(aprobador2);

            return lstQueries;

        }
    }
}
