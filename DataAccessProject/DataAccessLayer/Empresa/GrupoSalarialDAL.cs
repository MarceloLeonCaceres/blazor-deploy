using Dapper;
using DataAccessLibrary.Models.Empresa;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.DbAccess;
using DataAccessLibrary.Models.FeriadoLogTipoPermiso;

namespace DataAccessLibrary.DataAccessLayer.Empresa
{
    public class GrupoSalarialDAL : IGrupoSalarialDAL
    {

        public readonly ISqlDataAccess _dataAccess;

        public GrupoSalarialDAL(ISqlDataAccess dataAccess)
        {
            this._dataAccess = dataAccess;
        }

        public const string SELECT_GRUPOSSALARIALES = @"Select idGrupo, nomGrupo, automatico, Sueldo, Bono, HE50, HE100, JN25, DescuentoHora, DescuentoDia
            FROM GrupoSalarial";

        public async Task<List<GrupoSalarialModel>> GetGruposSalarialesAsync()
        {
            string sql = SELECT_GRUPOSSALARIALES + " Where idGrupo > 0";
            var result = await _dataAccess.ReadDataAsync<GrupoSalarialModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);
            return result.ToList();

        }

        public async Task<List<GrupoSalarialModel>> TraeTodosGruposSalarialesAsync()
        {
            string sql = SELECT_GRUPOSSALARIALES;
            var result = await _dataAccess.ReadDataAsync<GrupoSalarialModel, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);
            return result.ToList();
        }

        public async Task<int> GetGrupoSalarialCountAsync()
        {
            string sql = "select count(*) from GrupoSalarial";
            int numero = await _dataAccess.ScalarDataAsync<int, dynamic>(sql, null, VariablesGlobales.PROPERTIME_DATABASE);
            return numero;
        }

        public async Task AddGrupoSalarialAsync(GrupoSalarialModel gs)
        {
            //DynamicParameters parametros = ParametrosHorario(gs);

            string sql = @"Insert into GrupoSalarial (NomGrupo, Sueldo, Bono, HE50, HE100, JN25, DescuentoHora, DescuentoDia) 
                    values (@NomGrupo, @Sueldo, @Bono, @HE50, @HE100, @JN25, @DescuentoHora, @DescuentoDia);";
            await _dataAccess.SaveDataAsync(sql, gs, VariablesGlobales.PROPERTIME_DATABASE);

        }

        public async Task UpdateGrupoSalarialAsync(GrupoSalarialModel gs)
        {
            //DynamicParameters parametros = ParametrosHorario(gs);
            //parametros.Add("IdGrupo", gs.IdGrupo, DbType.Int16);

            string sql = @"Update GrupoSalarial set NomGrupo=@NomGrupo, 
                Sueldo = @Sueldo,  
                Bono = @Bono,  
                HE50 = @HE50,
                HE100 = @HE100,
                JN25 = @JN25,
                DescuentoHora = @DescuentoHora,
                DescuentoDia = @DescuentoDia
                where idGrupo=@IdGrupo";

            await _dataAccess.SaveDataAsync(sql, gs, VariablesGlobales.PROPERTIME_DATABASE);

        }

        //private DynamicParameters ParametrosHorario(GrupoSalarialModel grupoSalarial)
        //{
        //    DynamicParameters parametros = new DynamicParameters();
        //    parametros.Add("NomGrupo", grupoSalarial.NomGrupo, DbType.String);
        //    parametros.Add("automatico", grupoSalarial.Automatico, DbType.Boolean);
        //    parametros.Add("Sueldo", grupoSalarial.Sueldo, DbType.Double);
        //    parametros.Add("Bono", grupoSalarial.Bono, DbType.Double);
        //    parametros.Add("HE50", grupoSalarial.HE50, DbType.Double);
        //    parametros.Add("HE100", grupoSalarial.HE100, DbType.Double);
        //    parametros.Add("JN25", grupoSalarial.JN25, DbType.Double);
        //    parametros.Add("DescuentoHora", grupoSalarial.DescuentoHora, DbType.Double);
        //    parametros.Add("DescuentoDia", grupoSalarial.DescuentoDia, DbType.Double);

        //    return parametros;
        //}
        public async Task RemoveGrupoSalarialAsync(int ccId)
        {
            string sql = "Delete From GrupoSalarial Where idGrupo=@Id;\n";
            sql += "Update USERINFO Set idGrupoSalarial = 0 Where idGrupoSalarial=@Id;\n";
            await _dataAccess.DeleteDataAsync(sql, ccId, VariablesGlobales.PROPERTIME_DATABASE);
        }

        public async Task LogInsertGrupoSalarial(SystemLogModel log)
        {
            log.LogTag = 0;
            log.LogDescr = "Agrega Grupo Salarial";
            string sqlAux = @"select NomGrupo From GrupoSalarial 
                                    where idGrupo = (select IDENT_CURRENT('GrupoSalarial')); ";
            log.LogDetailed = (string)await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);
        }

        public async Task LogUpdateGrupoSalarial(SystemLogModel log)
        {
            log.LogDescr = "Modifica Grupo Salarial";
            string sqlAux = @"select NomGrupo From GrupoSalarial 
                                    where idGrupo = " + log.LogTag.ToString();
            log.LogDetailed += " -> " + (string)await _dataAccess.ScalarDataAsync<string, dynamic>(sqlAux, null, VariablesGlobales.PROPERTIME_DATABASE);
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);
        }

        public async Task LogDeleteGrupoSalarial(SystemLogModel log)
        {
            log.LogDescr = "Borra Grupo Salarial";
            InsertLog insertLog = new InsertLog(log);
            await _dataAccess.SaveDataAsync(insertLog.sql, insertLog.parametros, VariablesGlobales.PROPERTIME_DATABASE);
        }

    }
}
