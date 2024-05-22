using DataAccessLibrary.Models;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer
{
    public interface ICiudadDAL
    {
        Task AddCiudadAsync(CiudadModel city);
        Task<int> GetCiudadCountAsync();
        Task<List<CiudadModel>> GetCiudadesAsync();
        Task LogDeleteCiudad(SystemLogModel log);
        Task LogInsertCiudad(SystemLogModel log);
        Task LogUpdateCiudad(SystemLogModel log);
        Task RemoveCiudadAsync(int id);
        Task<List<CiudadModel>> TraeTodasCiudadesAsync();
        Task UpdateCiudadAsync(CiudadModel city);
    }
}