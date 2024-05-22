using Dapper;
using DataAccessLibrary.Models;
using DataAccessLibrary.Models.Reports;

using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.ReportsDAL
{
    public  class RpAsistenciaDAL
    {
        public IConfiguration Configuration;

        public RpAsistenciaDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private const string SELECT_ASISTENCIA = @"SET DateFormat dmy;
SELECT [NumCA]
,USERINFO.[NAME] AS [Empleado]
,[Dia]
,[Fecha]
,[Horario]
,Cast([HoraEntrada] AS varchar(5)) AS [HoraEntrada]
,Cast([HoraSalida]  AS varchar(5)) AS [HoraSalida]
,Cast([RegEntrada]  AS varchar(5)) AS [RegEntrada]
,Cast([RegSalida]   AS varchar(5)) AS [RegSalida]
,Cast([SalidaAlmuerzo]  AS varchar(5)) AS [SalidaAlmuerzo]
,Cast([RegresoAlmuerzo] AS varchar(5)) AS [RegresoAlmuerzo]
,[TiempoAlmuerzo]
,[ExcesoAlmuerzo]
,[Atrasos]
,[SalidasTemprano]
,[DiaAusente]  As Ausente
,[Permiso]
,[MotivoPermiso]
,[HNormal]
,[Suplem 25%]  As H_25
,[ExtraOrd]	   As H_50
,[Extra 100%]  As H_100
,[DiaLibre]    
,[TPermisoTrab] 
,[TPermisoNoTrab] 
,[TotalHE100]
,[TTrabajado]  
,[TAsistido]   
,[TNoCumplido] 
,[USD Suplem]	as USD_25
,[USD ExOrd]	as USD_50
,[USD 100%]	    as USD_100
,[USD Total]	as USD_Total
,[MultaAtrasos]
,[MultaAusencias]
,[DiaTrabajado]	as DiasTrabajados
,rptAsistencia.[UserId]
,[codh]
,[USERINFO].ssn as Cedula
,[Departamento]
,[Cargo]
,[AprobadoHE50]
,[AprobadoHE100]
,[T Aprobado HE50]		As AprobadoHE50
,[T Aprobado HE100]		As AprobadoHE100
,[T Aprobado DiaLibre]	As AprobadoDiaLibre
,[A USD 50%]	as AUSD_50
,[A USD 100%]	as AUSD_100
,[A USD TOTAL]	as AUSD_Total
,[Motivo]
,[Autorizado]
        FROM [dbo].[rptAsistencia] INNER JOIN USERINFO ON rptAsistencia.UserId = Userinfo.Userid
";

        private const string SELECT_ASISTENCIA_EXCEL = @"SET DateFormat dmy;
SELECT [NumCA]
,USERINFO.[NAME] AS [Empleado]
,[USERINFO].ssn as Cedula
,[Dia]
,[Fecha]
,[Horario]
,[HoraEntrada]
,[HoraSalida]
,[RegEntrada]
,[RegSalida]
,[SalidaAlmuerzo]
,[RegresoAlmuerzo]
,[TiempoAlmuerzo]
,[ExcesoAlmuerzo]
,[Atrasos]
,[SalidasTemprano]
,[DiaAusente]
,[Permiso]
,[MotivoPermiso]
,[HNormal]
,[Suplem 25%] as H_25
,[ExtraOrd]	as H_50
,[Extra 100%] as H_100
,[TPermisoTrab]
,[TPermisoNoTrab]
,[DiaLibre]
,[TotalHE100]
,[TTrabajado]
,[TAsistido]
,[TNoCumplido]
,[DiaTrabajado]	as DiasTrabajados
,rptAsistencia.[UserId]
,[codh]
        FROM [dbo].[rptAsistencia]
";

        private string SELECT_ASISTENCIA_EXCEL_PARCIAL(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            string query = SELECT_ASISTENCIA_EXCEL + " WHERE rptAsistencia.UserId in (" + csLstUsers + ") " +
                    "AND Fecha >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND Fecha < '" + fHasta.ToString("dd/MM/yyyy") + "' \n" +
                    "ORDER BY Empleado, Fecha;";
            return query;
        }

        private string SELECT_ASISTENCIA_PARCIAL(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            string query = SELECT_ASISTENCIA + " WHERE rptAsistencia.UserId in (" + csLstUsers + ") " +
                    "AND Fecha >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND Fecha < '" + fHasta.ToString("dd/MM/yyyy") + "' \n" +
                    "ORDER BY Empleado, Fecha;";
            return query;
        }


        public async Task<List<RpAsistenciaModel>> GetAsistenciaAsync(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            using(IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_ASISTENCIA_PARCIAL(csLstUsers, fDesde, fHasta);
                db.Open();
                try
                {
                    IEnumerable<RpAsistenciaModel> result = await db.QueryAsync<RpAsistenciaModel>(sql);
                    return result.ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<List<RpAsistenciaModel>> GetAsistenciaExcelAsync(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_ASISTENCIA_EXCEL_PARCIAL(csLstUsers, fDesde, fHasta);
                db.Open();
                try
                {
                    IEnumerable<RpAsistenciaModel> result = await db.QueryAsync<RpAsistenciaModel>(sql);
                    return result.ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<RpAsistenciaModel> GetAsistencia(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_ASISTENCIA_PARCIAL(csLstUsers, fDesde, fHasta);
                db.Open();
                try
                {
                    IEnumerable<RpAsistenciaModel> result = db.Query<RpAsistenciaModel>(sql);
                    return result.ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}
