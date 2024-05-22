using Dapper;
using System.Data;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer
{
    public class InsertLog
    {
        public DynamicParameters parametros;
        public string sql = @"INSERT INTO [dbo].[SystemLog]
                    ([Operator], [Alias], [LogTag], [LogDescr], [LogDetailed])
                    VALUES(@Operator, @Alias, @LogTag, @LogDescr, @LogDetailed)";
        public InsertLog(SystemLogModel log)
        {
            parametros = new DynamicParameters();
            parametros.Add("Operator", log.Operator, DbType.String);
            parametros.Add("Alias", log.Alias, DbType.String);
            parametros.Add("LogTag", log.LogTag, DbType.Int16);
            parametros.Add("LogDescr", log.LogDescr, DbType.String);
            parametros.Add("LogDetailed", log.LogDetailed, DbType.String);            
        }        
                
    }
}
