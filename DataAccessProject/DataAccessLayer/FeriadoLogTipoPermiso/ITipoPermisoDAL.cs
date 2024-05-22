using DataAccessLibrary.Models;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.FeriadoLogTipoPermiso
{
    public interface ITipoPermisoDAL
    {
        Task AddTipoPermisoAsync(TipoPermisoModel tipoPermiso);
        List<EnumModel> GetEnumsTipos(int seleccionable = 0);
        List<TipoCategoriaPermisoModel> GetEnumsTiposCategorias(int seleccionable = 0);
        List<TipoPermisoModel> GetTiposPermiso();
        Task<List<TipoPermisoModel>> GetTiposPermisoAsync();
        Task<int> GetTiposPermisoCountAsync();
        Task LogDeleteTipoPermiso(SystemLogModel log);
        Task LogInsertTipoPermiso(SystemLogModel log);
        Task LogUpdateTipoPermiso(SystemLogModel log);
        Task RemoveTipoPermisoAsync(int idTipoPermiso);
        Task UpdateTipoPermisoAsync(TipoPermisoModel tipoPermiso);
    }
}