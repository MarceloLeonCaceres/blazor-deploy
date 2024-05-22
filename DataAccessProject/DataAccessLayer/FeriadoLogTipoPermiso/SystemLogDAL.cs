using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using DataAccessLibrary.DbAccess;

namespace DataAccessLibrary.DataAccessLayer.FeriadoLogTipoPermiso
{
    public class SystemLogDAL : ISystemLogDAL
    {

        private readonly ISqlDataAccess _dataAccess;
        public SystemLogDAL(ISqlDataAccess dataAccess)
        {
            this._dataAccess = dataAccess;
        }

        public const string SELECT_LOGs = @"SELECT [ID],[Operator],[LogTime],[Alias],[LogTag],[LogDescr],[LogDetailed]
FROM [SystemLog]
WHERE Logtime between @fDesde and @fHasta
ORDER BY [LogTime] DESC;";

        public async Task<List<SystemLogModel>> GetSystemLogsAsync(DateTime f1, DateTime f2)
        {

            var lista = await _dataAccess.ReadDataAsync<SystemLogModel, dynamic>(SELECT_LOGs, new { fDesde = f1, fHasta = f2 }, VariablesGlobales.PROPERTIME_DATABASE);
            return lista;

        }
    }

}
