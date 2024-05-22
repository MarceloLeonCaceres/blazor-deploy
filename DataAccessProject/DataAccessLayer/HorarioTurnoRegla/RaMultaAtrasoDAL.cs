using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.HorarioTurnoRegla;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using DataAccessLibrary.DbAccess;

namespace DataAccessLibrary.DataAccessLayer.HorarioTurnoRegla
{
    public class RaMultaAtrasoDAL : IRaMultaAtrasoDAL
    {
        private readonly ISqlDataAccess _dataAccess;

        public RaMultaAtrasoDAL(IConfiguration config, ISqlDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        const string SELECT_RA_MultaAtraso = @"SELECT [Id], [Name], Menos15, Entre15_30, Mas30
  FROM [dbo].[RegMultaAtraso];";

        public async Task<List<RaMultaAtrasoModel>> GetRaMultaAtrasoAsync()
        {
            var result = await _dataAccess.ReadDataAsync<RaMultaAtrasoModel, dynamic>(SELECT_RA_MultaAtraso, null, VariablesGlobales.PROPERTIME_DATABASE);
            return result.ToList();
        }

        public async Task AddRaMultaAtrasoAsync(RaMultaAtrasoModel multa)
        {

            string sql = @"INSERT INTO [dbo].[RegMultaAtraso]
                        ( [Name], [Menos15], [Entre15_30], [Mas30] )
                        VALUES ( @Name, @Menos15, @Entre15_30, @Mas30 );";
            await _dataAccess.SaveDataAsync(sql, multa, VariablesGlobales.PROPERTIME_DATABASE);


            DynamicParameters parametros = SetParametros(multa);
            //using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            //{
            //    db.Open();
            //    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            //}
        }

        DynamicParameters SetParametros(RaMultaAtrasoModel regla)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Name", regla.Name, DbType.String);
            parametros.Add("Menos15", regla.Menos15, DbType.Decimal);
            parametros.Add("Entre15_30", regla.Entre15_30, DbType.Decimal);
            parametros.Add("Mas30", regla.Mas30, DbType.Decimal);
            return parametros;
        }

        public async Task UpdateRaMultaAtrasoAsync(RaMultaAtrasoModel multa)
        {
            string sql = @"UPDATE [dbo].[RegMultaAtraso]
   SET [Name] = @Name, 
      [Menos15] = @Menos15, 
      [Entre15_30] = @Entre15_30, 
      [Mas30] = @Mas30
 WHERE Id = @Id";
            await _dataAccess.SaveDataAsync(sql, multa, VariablesGlobales.PROPERTIME_DATABASE);

            //DynamicParameters parametros = SetParametros(multa);
            //parametros.Add("Id", multa.Id, DbType.Int16);
            //using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            //{
            //    db.Open();
            //    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            //}
        }

        public async Task RemoveRaMultaAtrasoAsync(int id)
        {
            string sql = "Delete from [dbo].[RegMultaAtraso] WHERE Id = @Id;\n";
            sql += "Update USERINFO set RegMultaAtraso = 0 Where RegMultaAtraso = @Id;";
            await _dataAccess.DeleteDataAsync(sql, id, VariablesGlobales.PROPERTIME_DATABASE);

            //using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            //{
            //    db.Open();
            //    await db.ExecuteAsync(sql, new { Id = id }, commandType: CommandType.Text);
            //}
        }

        public async Task LogInsertRaMultaAtraso(SystemLogModel log)
        {
            string sqlAux = @"select Name from [RegMultaAtraso] 
                                    where Id = (select IDENT_CURRENT('RegMultaAtraso')); ";
            log.LogTag = 0;
            log.LogDescr = "Agrega regla Multa Atraso";
            log.LogDetailed = (string)await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);

            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);
            //using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            //{
            //    db.Open();
            //    log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
            //    InsertLog insertLog = new InsertLog(log);
            //    await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            //}
        }

        public async Task LogUpdateRaMultaAtraso(SystemLogModel log)
        {
            string sqlAux = @"select [Name] from [RegMultaAtraso] 
                                    where Id = " + log.LogTag.ToString();
            log.LogDescr = "Modifica regla Multa Atraso";
            log.LogDetailed += " -> " + (string)await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);

            //using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            //{
            //    db.Open();
            //    log.LogDetailed += " -> " + (string)await db.ExecuteScalarAsync(sqlAux);
            //    InsertLog insertLog = new InsertLog(log);
            //    await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            //}
        }

        public async Task LogDeleteRaMultaAtraso(SystemLogModel log)
        {
            log.LogDescr = "Borra regla Multa Atrasos";
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros,VariablesGlobales.PROPERTIME_DATABASE);

            //using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            //{
            //    db.Open();
            //    await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            //}
        }

    }
}
