using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.DbAccess;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer
{
    public class CentroCostosDAL : ICentroCostosDAL
    {
        private readonly ISqlDataAccess _dataAccess;

        public CentroCostosDAL(ISqlDataAccess dataAccess)
        {
            this._dataAccess = dataAccess;
        }

        public const string SELECT_CENTROCOSTOS = "Select IdCentroCostos, NomCentroCostos From Centro_Costos";

        public async Task<List<CentroCostosModel>> GetCentrosCostoAsync()
        {
            string sql = SELECT_CENTROCOSTOS + " Where IdCentroCostos > 0";
            var result = await _dataAccess.ReadDataAsync<CentroCostosModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);

            return result.ToList();
            
        }

        public async Task<List<CentroCostosModel>> TraeTodosCentrosCostoAsync()
        {
            string sql = SELECT_CENTROCOSTOS;
            var result = await _dataAccess.ReadDataAsync<CentroCostosModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);

            return result.ToList();
            
        }

        public async Task<int> GetCentroCostosCountAsync()
        {
            string sql = "select count(*) from Centro_Costos";
            int numeroCentrosCosto = await _dataAccess.ScalarDataAsync<int, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);
            return numeroCentrosCosto;
            
        }

        public async Task AddCentroCostosAsync(CentroCostosModel cc)
        {
            string sql = "Insert into Centro_Costos (NomCentroCostos) values (@NomCentroCostos);";
            await _dataAccess.SaveDataAsync(sql, cc, VariablesGlobales.PROPERTIME_DATABASE);
            
        }

        public async Task UpdateCentroCostosAsync(CentroCostosModel cc)
        {
            string sql = @"Update Centro_Costos set NomCentroCostos=@NomCentroCostos where idCentroCostos=@IdCentroCostos";
            await _dataAccess.SaveDataAsync(sql, cc, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task RemoveCentroCostosAsync(int ccId)
        {
            string sql = "Delete From Centro_Costos Where idCentroCostos=@Id;\n";
            sql += "Update USERINFO Set idCentroCostos = 0 Where idCentroCostos=@Id;\n";

            await _dataAccess.DeleteDataAsync(sql, ccId, VariablesGlobales.PROPERTIME_DATABASE);
        }

        public async Task LogInsertCentroCostos(SystemLogModel log)
        {

            log.LogTag = 0;
            log.LogDescr = "Agrega Centro de Costos";
            string sqlAux = @"select nomCentroCostos From Centro_Costos 
                where idCentroCostos = (select IDENT_CURRENT('Centro_Costos')); ";
            log.LogDetailed = (string)await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task LogUpdateCentroCostos(SystemLogModel log)
        {
            log.LogDescr = "Modifica Centro de Costos";
            string sqlAux = @"select nomCentroCostos From Centro_Costos 
                                    where idCentroCostos = " + log.LogTag.ToString();
            log.LogDetailed += " -> " + (string)await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task LogDeleteCentroCostos(SystemLogModel log)
        {
            log.LogDescr = "Borra Centro de Costos";
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

        }
    }
}
