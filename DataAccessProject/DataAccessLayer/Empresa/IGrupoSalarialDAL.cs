using DataAccessLibrary.Models.Empresa;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.Empresa
{
    public interface IGrupoSalarialDAL
    {
        Task AddGrupoSalarialAsync(GrupoSalarialModel gs);
        Task<int> GetGrupoSalarialCountAsync();
        Task<List<GrupoSalarialModel>> GetGruposSalarialesAsync();
        Task LogDeleteGrupoSalarial(SystemLogModel log);
        Task LogInsertGrupoSalarial(SystemLogModel log);
        Task LogUpdateGrupoSalarial(SystemLogModel log);
        Task RemoveGrupoSalarialAsync(int ccId);
        Task<List<GrupoSalarialModel>> TraeTodosGruposSalarialesAsync();
        Task UpdateGrupoSalarialAsync(GrupoSalarialModel gs);
    }
}