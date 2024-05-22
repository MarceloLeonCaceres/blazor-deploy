using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.FeriadoLogTipoPermiso
{
    public interface ISystemLogDAL
    {
        Task<List<SystemLogModel>> GetSystemLogsAsync(DateTime f1, DateTime f2);
    }
}