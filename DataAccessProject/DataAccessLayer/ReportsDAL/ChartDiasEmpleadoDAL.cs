using DataAccessLibrary.DbAccess;
using DataAccessLibrary.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.ReportsDAL
{
    public class ChartDiasEmpleadoDAL : IChartDiasEmpleadoDAL
    {
        private readonly ISqlDataAccess _dataAccess;

        public ChartDiasEmpleadoDAL(ISqlDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        private const string SELECT_RESUMEN_DIAS_EMPLEADO = @"select userid, sum(DiaNormal) as Laborables, sum(DiaTrabajado) as Trabajados, 
sum(DiaAusente) as Ausencias, 
count(Atrasos) as Atrasos, count(SalidasTemprano) as SalidasTemprano, count(Permiso) as Permisos, count(ExcesoAlmuerzo) as ExcesoLunch
from rptAsistencia
where Fecha between @Desde and @Hasta and userid = @UserId
group by userid";

        private static string SELECT_RESUMEN_EMPLEADOS(string listaUsersId)
        {
            return $@"select Empleado as NomEmpleado, cast(sum(DiaAusente) as decimal(5,1)) as NumeroDias, CAST( sum(DiaAusente) AS varchar) as DataLabelMappingName
from rptAsistencia
where Fecha between @Desde and @Hasta AND userid in ( {listaUsersId} )
group by userid, Empleado";
        }

        private static string SELECT_RESUMEN_VARIOS_EMPLEADOS(string listaUsersId)
        {
            return $@"select Empleado as NomEmpleado, 
Cast(sum(DiaNormal) as decimal(5,1) ) as Laborables, Cast(sum(DiaNormal) as varchar ) as LabelLaborables,
cast(sum(DiaTrabajado) as decimal(5,1)) as Trabajados, CAST( sum(DiaTrabajado) AS varchar) as LabelTrabajados,
cast(sum(DiaAusente) as decimal(5,1)) as Ausencias, 
cast(count(Atrasos) as decimal(5,1)) as Atrasos, 
cast(count(SalidasTemprano) as decimal(5,1)) as SalidasTemprano, 
cast(count(Permiso) as decimal(5,1)) as Permisos, 
cast(count(ExcesoAlmuerzo) as decimal(5,1)) as ExcesosLunch
from rptAsistencia
where Fecha between @Desde and @Hasta AND userid in ( {listaUsersId} )
group by Empleado
order by empleado";
        }

        public async Task<ChartDiasEmpleadoModel> GetResumenDiasEmpleado(DateTime fDesde, DateTime fHasta, int userId)
        {
            string sql = SELECT_RESUMEN_DIAS_EMPLEADO;
            var datosChart = await _dataAccess.ReadDataAsync<ChartDiasEmpleadoModel, dynamic>(sql, new { Desde = fDesde, Hasta = fHasta, UserId = userId }, VariablesGlobales.PROPERTIME_DATABASE);
            return datosChart.FirstOrDefault();
            
        }

        public async Task<List<DataChartEmpleado>> GetDatosGraficosAsync(DateTime fDesde, DateTime fHasta, string listaUsers)
        {
            string sql = SELECT_RESUMEN_EMPLEADOS(listaUsers);
            var datosChart = await _dataAccess.ReadDataAsync<DataChartEmpleado, dynamic>(sql, new { Desde = fDesde, Hasta = fHasta }, VariablesGlobales.PROPERTIME_DATABASE);
            return datosChart.ToList();
        }

        public async Task<List<ChartVariosEmpleadosModel>> GetDatosVariosGraficosAsync(DateTime fDesde, DateTime fHasta, string listaUsers)
        {
            string sql = SELECT_RESUMEN_VARIOS_EMPLEADOS(listaUsers);
            var datosChart = await _dataAccess.ReadDataAsync<ChartVariosEmpleadosModel, dynamic>(sql, new { Desde = fDesde, Hasta = fHasta }, VariablesGlobales.PROPERTIME_DATABASE);
            return datosChart.ToList();
        }

    }
}
