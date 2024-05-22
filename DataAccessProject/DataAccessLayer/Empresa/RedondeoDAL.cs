using Dapper;
using DataAccessLibrary.Models.Empresa;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer.Empresa
{
    public class RedondeoDAL
    {
        public IConfiguration Configuration;

        public const string SELECT_PARAM = @"SELECT * FROM
(
	SELECT [PARANAME], [PARAVALUE]
	  FROM [ProperParam]
) t
PIVOT (
	MIN(ParaValue)
	for paraname in ([RedondeoDir], [RedondeoA], [RedondeoHoraMinSeg])
) as pivot_table;";

        public RedondeoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public RedondeoModel GetRedondeoModel()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    RedondeoModel OpcRedondeo = new RedondeoModel();
                    OpcRedondeo = db.QueryFirstOrDefault<RedondeoModel>(SELECT_PARAM, null, commandType: CommandType.Text);
                    return OpcRedondeo;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task UpdateOpcRedondeoAsync(RedondeoModel OpcRedondeo)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("UpDown", OpcRedondeo.RedondeoDir, DbType.Int16);
            parametros.Add("CompleteHours", OpcRedondeo.RedondeoA, DbType.Int16);
            parametros.Add("FMinSeg", OpcRedondeo.RedondeoHoraMinSeg, DbType.Int16);

            string sql = @"DELETE from ProperParam where PARANAME in ( 'RedondeoDir', 'RedondeoA', 'RedondeoHoraMinSeg');
            INSERT into ProperParam (PARANAME, PARATYPE, PARAVALUE) VALUES ('RedondeoDir', 'Integer', @UpDown);
            INSERT into ProperParam(PARANAME, PARATYPE, PARAVALUE) VALUES('RedondeoA', 'Integer', @CompleteHours);
            INSERT into ProperParam(PARANAME, PARATYPE, PARAVALUE) VALUES('RedondeoHoraMinSeg', 'Integer', @FMinSeg);";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogUpdateRedondeo(SystemLogModel log)
        {
            log.LogDescr = "Modifica opciones Redondeo";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {

                db.Open();
                log.LogDetailed = "Modifica opciones de redondeo";
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }
}
