using DataAccessLibrary.Models.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.ReportsDAL
{
    public interface IChartDiasEmpleadoDAL
    {
        Task<List<DataChartEmpleado>> GetDatosGraficosAsync(DateTime fDesde, DateTime fHasta, string listaUsers);
        Task<List<ChartVariosEmpleadosModel>> GetDatosVariosGraficosAsync(DateTime fDesde, DateTime fHasta, string listaUsers);
        Task<ChartDiasEmpleadoModel> GetResumenDiasEmpleado(DateTime fDesde, DateTime fHasta, int userId);
    }
}