using DataAccessLibrary.Models;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer
{
    public interface ICentroCostosDAL
    {
        Task AddCentroCostosAsync(CentroCostosModel cc);
        Task<int> GetCentroCostosCountAsync();
        Task<List<CentroCostosModel>> GetCentrosCostoAsync();
        Task LogDeleteCentroCostos(SystemLogModel log);
        Task LogInsertCentroCostos(SystemLogModel log);
        Task LogUpdateCentroCostos(SystemLogModel log);
        Task RemoveCentroCostosAsync(int ccId);
        Task<List<CentroCostosModel>> TraeTodosCentrosCostoAsync();
        Task UpdateCentroCostosAsync(CentroCostosModel cc);
    }
}