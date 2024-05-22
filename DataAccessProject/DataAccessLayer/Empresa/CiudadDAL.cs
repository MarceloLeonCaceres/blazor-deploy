using DataAccessLibrary.DbAccess;
using DataAccessLibrary.Models;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer
{
    public class CiudadDAL : ICiudadDAL
    {
        
        private readonly ISqlDataAccess _dataAccess;
        public CiudadDAL(ISqlDataAccess dataAccess)
        {
            this._dataAccess = dataAccess;
        }

        public const string SELECT_CIUDADES = "Select IdCiudad, NomCiudad From Ciudad";

        public async Task<List<CiudadModel>> GetCiudadesAsync()
        {
            string sql = SELECT_CIUDADES + " Where IdCiudad > 0";
            var result = await _dataAccess.ReadDataAsync<CiudadModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);
            return result.ToList();
            
        }

        public async Task<List<CiudadModel>> TraeTodasCiudadesAsync()
        {
            string sql = SELECT_CIUDADES;
            var result = await _dataAccess.ReadDataAsync<CiudadModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);
            return result.ToList();

        }

        public async Task<int> GetCiudadCountAsync()
        {
            string sql = "select count(*) from Ciudad";
            int numero = await _dataAccess.ScalarDataAsync<int, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);
            return numero;

        }

        public async Task AddCiudadAsync(CiudadModel city)
        {
            string sql = "Insert into Ciudad (NomCiudad) values (@NomCiudad);";
            await _dataAccess.SaveDataAsync(sql, city, VariablesGlobales.PROPERTIME_DATABASE);
            
        }

        public async Task UpdateCiudadAsync(CiudadModel city)
        {
            string sql = @"Update Ciudad set NomCiudad=@NomCiudad where idCiudad=@IdCiudad";
            await _dataAccess.SaveDataAsync(sql, city, VariablesGlobales.PROPERTIME_DATABASE);

        }
        
        public async Task RemoveCiudadAsync(int id)
        {
            string sql = "Delete from Ciudad Where idCiudad=@Id;\n";
            sql += "Delete from [HOLIDAYS] Where idCiudad=@Id;\n";
            sql += "Update USERINFO Set idCiudad = 0 Where idCiudad=@Id;\n";
            await _dataAccess.DeleteDataAsync(sql, id, VariablesGlobales.PROPERTIME_DATABASE);
            
        }

        public async Task LogInsertCiudad(SystemLogModel log)
        {
            string sqlAux = @"select nomCiudad from CIUDAD 
                                    where idCiudad = (select IDENT_CURRENT('ciudad')); ";
            log.LogTag = 0;
            log.LogDescr = "Agrega ciudad";
            log.LogDetailed = (string)await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task LogUpdateCiudad(SystemLogModel log)
        {
            string sqlAux = @"select nomCiudad from CIUDAD 
                                    where idCiudad = " + log.LogTag.ToString();
            log.LogDescr = "Modifica ciudad";
            log.LogDetailed += " -> " + (string)await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task LogDeleteCiudad(SystemLogModel log)
        {
            log.LogDescr = "Borra ciudad";
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);
            
        }

    }
}
