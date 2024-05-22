using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models.HorarioTurnoRegla;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.HorarioTurnoRegla
{
    public class DetalleTurnoDAL
    {
        public IConfiguration Configuration;

        public DetalleTurnoDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private DynamicParameters Parametros(DetalleTurnoModel detalle)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("IdTurno", detalle.IdTurno, DbType.Int16);
            parametros.Add("hDesde", detalle.hDesde, DbType.DateTime);
            parametros.Add("hHasta", detalle.hHasta, DbType.DateTime);
            parametros.Add("sDia", detalle.sDia, DbType.Int16);
            parametros.Add("eDay", detalle.eDay, DbType.Int16);
            parametros.Add("IdHorario", detalle.IdHorario, DbType.Int16);
            return parametros;
        }

        public async Task InsertaDetalleTurnoAsync(DetalleTurnoModel detalle)
        {
            DynamicParameters parametros = Parametros(detalle);
            string sql = @"INSERT INTO num_run_deil 
                (NUM_RUNID,STARTTIME,ENDTIME,SDAYS,EDAYS,SCHCLASSID) 
                VALUES
                (@IdTurno, @hDesde, @hHasta, @sDia, @eDay, @IdHorario)";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql, parametros, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task EliminaHorariosDeTurno(int idTurno)
        {
            DynamicParameters parametro = new DynamicParameters();
            parametro.Add("idTurno", idTurno, DbType.Int16);
            string sql = @"DELETE FROM num_run_deil WHERE num_runid = @idTurno";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                try
                {
                    await db.ExecuteAsync(sql, parametro, commandType: CommandType.Text);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task LogBorraDetalleTurno(SystemLogModel log)
        {
            log.LogDescr = "Borra Horarios Turno";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

        public async Task LogInsertDetalleTurno(SystemLogModel log)
        {
            log.LogDescr = "Inserta Horarios Turno";
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sqlAux = @"select NAME from NUM_RUN 
                                    where NUM_RUNID = " + log.LogTag.ToString();
                db.Open();
                log.LogDetailed = (string)await db.ExecuteScalarAsync(sqlAux);
                InsertLog insertLog = new InsertLog(log);
                await db.ExecuteAsync(insertLog.sql, insertLog.parametros, commandType: CommandType.Text);
            }
        }

    }


}
