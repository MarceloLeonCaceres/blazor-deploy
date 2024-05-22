using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using DataAccessLibrary.Models.HorarioTurnoRegla;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.HorarioTurnoRegla
{
    public interface IRaMultaAtrasoDAL
    {
        Task<List<RaMultaAtrasoModel>> GetRaMultaAtrasoAsync();
        Task AddRaMultaAtrasoAsync(RaMultaAtrasoModel multa);
        Task UpdateRaMultaAtrasoAsync(RaMultaAtrasoModel multa);
        Task RemoveRaMultaAtrasoAsync(int id);

        Task LogInsertRaMultaAtraso(SystemLogModel log);
        Task LogUpdateRaMultaAtraso(SystemLogModel log);
        Task LogDeleteRaMultaAtraso(SystemLogModel log);
    }
}