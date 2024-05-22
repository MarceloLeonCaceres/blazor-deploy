using DataAccessLibrary.Models;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer
{
    public interface IDepartamentoDAL
    {
        Task AddDeptoAsync(DepartamentoModel depto);
        List<DepartamentoModel> GetDeptos();
        List<DepartamentoModel> GetDeptos(int otAdmin, int idDepto);
        Task<List<DepartamentoModel>> GetDeptosAsync();
        Task LogDeleteDepto(SystemLogModel log);
        Task LogInsertDepto(SystemLogModel log);
        Task LogUpdateDepto(SystemLogModel log);
        Task<int> PermiteRemoverDeptoAsync(int bugid);
        Task<bool> RemoveDeptoAsync(int bugid);
        Task UpdateDeptoAsync(DepartamentoModel depto);
    }
}