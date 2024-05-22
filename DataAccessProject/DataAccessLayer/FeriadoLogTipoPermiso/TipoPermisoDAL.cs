using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using DataAccessLibrary.DbAccess;

namespace DataAccessLibrary.DataAccessLayer.FeriadoLogTipoPermiso
{
    public class TipoPermisoDAL : ITipoPermisoDAL
    {
        private readonly ISqlDataAccess _dataAccess;

        public TipoPermisoDAL(ISqlDataAccess dataAccess)
        {
            this._dataAccess = dataAccess;
        }

        private const string SELECT_TIPOSPERMISO = @"SELECT [LeaveId], [LeaveName], [ReportSymbol], 
            [Classify] as idCategoria, 
            CASE Classify   when 0      then 'Justificación' 
                            when 1      then 'Vacaciones' 
                            when 2      then 'Cargo a Vacaciones' 
                            when 128    then  'Trabajado' end as categoria,
            [Seleccionable] as idSeleccion, case Seleccionable when 0 then 'false' else 'true' end as Seleccionable
            FROM [LeaveClass]";

        private const string SELECT_ENUM_TIPOSPERMISO = @"SELECT [LeaveId] as id, [LeaveName] as descripcion FROM [LeaveClass]";
        private const string SELECT_ENUM_TIPOSPERMISO_SELECCIONABLES = SELECT_ENUM_TIPOSPERMISO + " WHERE Seleccionable = 1";


        private const string GET_ENUM_TIPOSCATEGORIASPERMISO = @"SELECT [LeaveId] as id, [LeaveName] as descripcion, Classify FROM [LeaveClass]";
        private const string GET_ENUM_TIPOSCATEGORIASPERMISO_SELECCIONABLES = GET_ENUM_TIPOSCATEGORIASPERMISO + " WHERE Seleccionable = 1";

        public async Task<List<TipoPermisoModel>> GetTiposPermisoAsync()
        {
            string sql = SELECT_TIPOSPERMISO;
            var lista = await _dataAccess.ReadDataAsync<TipoPermisoModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);

            return lista.ToList();
        }

        public List<TipoPermisoModel> GetTiposPermiso()
        {
            string sql = SELECT_TIPOSPERMISO;
            var lista = _dataAccess.ReadData<TipoPermisoModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);

            return lista.ToList();
        }

        public List<EnumModel> GetEnumsTipos(int seleccionable = 0)
        {
            string sql = seleccionable == 1 ? SELECT_ENUM_TIPOSPERMISO_SELECCIONABLES : SELECT_ENUM_TIPOSPERMISO;
            var lista = _dataAccess.ReadData<EnumModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);

            return lista.ToList();
        }

        public List<TipoCategoriaPermisoModel> GetEnumsTiposCategorias(int seleccionable = 0)
        {
            string sql = seleccionable == 1 ? GET_ENUM_TIPOSCATEGORIASPERMISO_SELECCIONABLES : GET_ENUM_TIPOSCATEGORIASPERMISO;
            var lista = _dataAccess.ReadData<TipoCategoriaPermisoModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);

            return lista.ToList();
        }

        public async Task<int> GetTiposPermisoCountAsync()
        {
            string sql = "select count(*) from LeaveClass";
            int numero = await _dataAccess.ScalarDataAsync<int, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);

            return numero;
        }

        public async Task AddTipoPermisoAsync(TipoPermisoModel tipoPermiso)
        {
            string sql = @"Insert into [LeaveClass] ([LeaveName], [ReportSymbol], [Classify], [Seleccionable])
                    values (@LeaveName, @ReportSymbol, @idCategoria, @idSeleccionable);";
            tipoPermiso.idSeleccionable = tipoPermiso.seleccionable ? 1 : 0;

            await _dataAccess.SaveDataAsync(sql, tipoPermiso, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task UpdateTipoPermisoAsync(TipoPermisoModel tipoPermiso)
        {
            string sql = @"Update[LeaveClass] set LeaveName = @LeaveName,
                ReportSymbol=@ReportSymbol, Classify=@idCategoria, seleccionable=@idSeleccionable
                where LeaveId = @LeaveId";
            tipoPermiso.idSeleccionable = tipoPermiso.seleccionable ? 1 : 0;

            await _dataAccess.SaveDataAsync(sql, tipoPermiso, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task RemoveTipoPermisoAsync(int idTipoPermiso)
        {
            string sql = "Delete from [LeaveClass] Where LeaveId=@Id;\n";
            sql += "Delete from [USER_SPEDAY] Where DateId=@Id;\n";

            await _dataAccess.DeleteDataAsync(sql, idTipoPermiso, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task LogInsertTipoPermiso(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega Tipo de Permiso";
            string sqlAux = @"select LeaveName from [LeaveClass] 
                                    where LeaveId = (select IDENT_CURRENT('LeaveClass')); ";
            log.LogDetailed = await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task LogUpdateTipoPermiso(SystemLogModel log)
        {
            log.LogDescr = "Modifica Tipo de Permiso";
            string sqlAux = @"select [LeaveName] from [LeaveClass] 
                                    where LeaveId = " + log.LogTag.ToString();
            log.LogDetailed += " -> " + await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task LogDeleteTipoPermiso(SystemLogModel log)
        {
            log.LogDescr = "Borra Tipo de Permiso";
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

        }

    }
}
