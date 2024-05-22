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
    public class RpRsmAsistenciaEmpleadoDAL
    {
        public IConfiguration Configuration;

        public RpRsmAsistenciaEmpleadoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private const string SELECT_Rsm_Asistencia = @"SET DateFormat dmy;
With Temporal as
(
SELECT 
[UserId]
	  
,round(SUM(CAST([TiempoAlmuerzo] As float)) * 24, 4) As [Almuerzo]
,round(SUM(CAST([ExcesoAlmuerzo] As float)) * 24, 4) As	[ExcesoAlmuerzo]
,round(SUM(CAST([Atrasos] As float)) * 24, 4) As	[Atrasos]
,round(SUM(CAST([SalidasTemprano] As float)) * 24, 4) As	[SalidasTemprano]

,COUNT([Atrasos] ) As	[NumAtrasos]
,COUNT([SalidasTemprano] ) As	[NumSalidasTemprano]
      
,SUM([DiaNormal]) As	[DiasLaborables]
,SUM([DiaTrabajado]) As	[DiasTrabajados]
,SUM([DiaAusente]) As	[DiasAusente]
      
,round(SUM(CAST([HNormal] As float)) * 24, 4) As	[HNormal]
,round(SUM(CAST([Suplem 25%] As float)) * 24, 4) As	[H_25]
,round(SUM(CAST([ExtraOrd] As float)) * 24, 4) As	[H_50]
,round(SUM(CAST([Extra 100%] As float)) * 24, 4) As	[H_100]
,round(SUM(CAST([DiaLibre] As float)) * 24, 4) As	[DiaLibre]
      
,round(SUM([TotalHE100]), 4) As	[TotalHE100]
,COUNT([Permiso]) As NumPermisos

,round(SUM(CAST([TPermisoTrab] As float)) * 24, 4) As	[TPermisoTrab]
,round(SUM(CAST([TPermisoNoTrab] As float)) * 24, 4) As	[TPermisoNoTrab]
,round(SUM(CAST([TTrabajado] As float)) * 24, 4) As	[TTrabajado]
,round(SUM(CAST([TAsistido] As float)) * 24, 4) As	[TAsistido]
,round(SUM(CAST([TNoCumplido] As float)) * 24, 4) As	[TNoCumplido]

,SUM([USD Suplem]) As	[USD_25]
,SUM([USD ExOrd]) As	[USD_50]
,SUM([USD 100%]) As	[USD_100]
,SUM([USD Total]) As	[USD_Total]
,SUM([MultaAtrasos]) As	[MultaAtrasos]
,SUM([MultaAusencias]) As	[MultaAusencias]
      
,round(SUM(CAST([T Aprobado HE50] As float)) * 24, 4) As	[TAprobadoHE50]
,round(SUM(CAST([T Aprobado HE100] As float)) * 24, 4) As	[TAprobadoHE100]
,round(SUM(CAST([T Aprobado DiaLibre] As float)) * 24, 4) As	[TAprobadoDiaLibre]
      
,SUM([A USD 50%]) As	[AUSD_50]
,SUM([A USD 100%]) As	[AUSD_100]
,SUM([A USD TOTAL]) As [AUSD_Total]
      
FROM [dbo].[rptAsistencia]
";

        private string SELECT_Rsm_Asistencia_PARCIAL(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            string query = SELECT_Rsm_Asistencia + " WHERE Userid in (" + csLstUsers + ") \n" +
                    "AND Fecha >= '" + fDesde.ToString("dd/MM/yyyy") + "' AND Fecha < '" + fHasta.ToString("dd/MM/yyyy") + "' \n" +
                    @"GROUP BY [UserId]
)
select U.Badgenumber as  [NumCA], U.Name as [Empleado], U.SSN as [Cedula], u.title as [Cargo], Temporal.*
from Temporal inner join userinfo U on 
Temporal.UserId = U.USERID 
ORDER BY Empleado;";
            return query;
        }

        public async Task<List<RpRsmAsistenciaEmpledoModel>> GetRsmAsistenciaAsync(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_Rsm_Asistencia_PARCIAL(csLstUsers, fDesde, fHasta);
                db.Open();
                try
                {
                    IEnumerable<RpRsmAsistenciaEmpledoModel> result = await db.QueryAsync<RpRsmAsistenciaEmpledoModel>(sql);
                    return result.ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<RpRsmAsistenciaEmpledoModel> GetRsmAsistencia(string csLstUsers, DateTime fDesde, DateTime fHasta)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = SELECT_Rsm_Asistencia_PARCIAL(csLstUsers, fDesde, fHasta);
                db.Open();
                try
                {
                    IEnumerable<RpRsmAsistenciaEmpledoModel> result = db.Query<RpRsmAsistenciaEmpledoModel>(sql);
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
