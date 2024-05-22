using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataAccessLayer
{
    public class EnumDAL
    {
        public IConfiguration Configuration;
        
        public const string SELECT_Parametro = @"SELECT id, Descripcion
            FROM [bpParam] Where Parametro = @TipoParametro Order By id";

        public EnumDAL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<List<EnumModel>> GetParametrosAsync(string tipoParametros)
        {

            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("TipoParametro", tipoParametros, DbType.String);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                
                db.Open();
                IEnumerable<EnumModel> result = await db.QueryAsync<EnumModel>(SELECT_Parametro, parametros, commandType: CommandType.Text);
                return result.ToList();
            }
        }

        public async Task<(List<EnumModel>, List<EnumModel>, List<EnumModel>)> GetParametrosAsync(string par1, string par2, string par3)
        {

            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("Par1", par1, DbType.String);
            parametros.Add("Par2", par2, DbType.String);
            parametros.Add("Par3", par3, DbType.String);
            string sql = @"SELECT id, Descripcion FROM [bpParam] Where Parametro = @Par1 Order By id;
                           SELECT id, Descripcion FROM [bpParam] Where Parametro = @Par2 Order By id;
                           SELECT id, Descripcion FROM [bpParam] Where Parametro = @Par3 Order By id;";
            List<EnumModel> ListParams1 = null;
            List<EnumModel> ListParams2 = null;
            List<EnumModel> ListParams3 = null;
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                using (var listas = await db.QueryMultipleAsync(sql, parametros, commandType: CommandType.Text))
                {
                    ListParams1 = listas.Read<EnumModel>().ToList();
                    ListParams2 = listas.Read<EnumModel>().ToList();
                    ListParams3 = listas.Read<EnumModel>().ToList();
                }                    
            }
            return (ListParams1, ListParams2, ListParams3);
        }

        public async Task<(List<eContratoModel>, List<CiudadModel>, List<CentroCostosModel>, 
            List<EnumModel>, List<EnumModel>, List<EnumModel>, List<EnumModel>, List<EnumModel>)> GetParamEmpleadosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select * from dbo.Contrato;
                               select * from dbo.Ciudad;                               
                               select * from dbo.Centro_Costos;     
                               select DeptId as id,         DeptName as descripcion      from dbo.Departments;
                               select idTipo as id,         TipoEmpleado as descripcion  from dbo.TipoEmpleado; 
                               select idGenero as id,       nomGenero as descripcion     from dbo.Genero; 
                               select idTipoSangre as id,   TipoSangre as descripcion    from dbo.TipoDeSangre; 
                               select idEstadoCivil as id, nomEstadoCivil as descripcion from dbo.Estado_Civil;"; 
                List<eContratoModel> eContratos = null;
                List<CiudadModel> ciudades = null;
                List<CentroCostosModel> centrosCosto = null;
                List<EnumModel> deptos = null;
                List<EnumModel> tiposEmpleado = null;                
                List<EnumModel> generos = null;
                List<EnumModel> tipoSangre = null;
                List<EnumModel> estadoCivil = null;

                db.Open();
                using (var lists = await db.QueryMultipleAsync(sql))
                {
                    eContratos = lists.Read<eContratoModel>().ToList();
                    ciudades = lists.Read<CiudadModel>().ToList();
                    centrosCosto = lists.Read<CentroCostosModel>().ToList();
                    deptos = lists.Read<EnumModel>().ToList();
                    tiposEmpleado = lists.Read<EnumModel>().ToList();
                    generos = lists.Read<EnumModel>().ToList();
                    tipoSangre = lists.Read<EnumModel>().ToList();
                    estadoCivil = lists.Read<EnumModel>().ToList();

                }
                return (eContratos, ciudades, centrosCosto, deptos, tiposEmpleado, generos, tipoSangre, estadoCivil);
            }
        }

        public async Task<List<EnumModel>> GetEstadosCivilesAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select idEstadoCivil as id, nomEstadoCivil as descripcion from dbo.Estado_Civil;";                                 

                db.Open();
                IEnumerable<EnumModel> estadoCivil = await db.QueryAsync<EnumModel>(sql);
                return estadoCivil.ToList();
            }
        }

        public async Task<List<EnumModel>> GetTiposSangreAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select [idTipoSangre] as id, [Tiposangre] as descripcion from dbo.[TipoDeSangre];";

                db.Open();
                IEnumerable<EnumModel> tiposDeSangre = await db.QueryAsync<EnumModel>(sql);
                return tiposDeSangre.ToList();
            }
        }

        public async Task<List<EnumModel>> GetGenerosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select idGenero as id,       nomGenero as descripcion     from dbo.Genero;";

                db.Open();
                IEnumerable<EnumModel> tiposDeSangre = await db.QueryAsync<EnumModel>(sql);
                return tiposDeSangre.ToList();
            }
        }

        public async Task<List<EnumModel>> GetCiudadesAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select idCiudad as id,       nomCiudad as descripcion     from dbo.Ciudad;";

                db.Open();
                IEnumerable<EnumModel> ciudades = await db.QueryAsync<EnumModel>(sql);
                return ciudades.ToList();
            }
        }

        public async Task<List<EnumModel>> GetReglasAsistenciaAsync(string tipo)
        {
            string sql = "";
            switch(tipo)
            {
                case "MarcacionesIncompletas":
                    sql = @"SELECT id_RegAsistencia as id,         des_RegAsistencia as descripcion     FROM RegAsistencia;";
                    break;
                case "Sobretiempo":
                    sql = @"SELECT id_RegSobretiempo as id,       des_RegSobretiempo as descripcion     FROM RegSobretiempo;";
                    break;
                case "HoraExtra":
                    sql = @"SELECT id_RegHoraExtra as id,           des_RegHoraExtra as descripcion     FROM RegHoraExtra;";
                    break;
                case "DiasHabiles":
                    sql = @"SELECT id_RegDiaHabil as id,             des_RegDiaHabil as descripcion     FROM RegDiaHabil;";
                    break;
                case "Feriados":
                    sql = @"SELECT id_RegOtros as id,                   des_RegOtros as descripcion     FROM RegOtros;";
                    break;
                case "MultaAtrasos":
                    sql = @"SELECT Id as id,                   [Name] as descripcion     FROM RegMultaAtraso;";
                    break;
            }
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {                
                db.Open();
                IEnumerable<EnumModel> reglas = await db.QueryAsync<EnumModel>(sql);
                return reglas.ToList();
            }
        }

        public async Task<List<EnumModel>> GetCentroCostosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select idCentroCostos as id,       nomCentroCostos as descripcion     from dbo.Centro_Costos;";

                db.Open();
                IEnumerable<EnumModel> centros = await db.QueryAsync<EnumModel>(sql);
                return centros.ToList();
            }
        }
        public async Task<List<EnumModel>> GetContratosAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select idContrato as id,       nomContrato as descripcion     from dbo.Contrato;";

                db.Open();
                IEnumerable<EnumModel> contratos = await db.QueryAsync<EnumModel>(sql);
                return contratos.ToList();
            }
        }
        public async Task<List<EnumModel>> GetDepartamentosAsync(int idRaiz, int otAdmin)
        {
            DynamicParameters parametros = new DynamicParameters();
            parametros.Add("idDepto", idRaiz, DbType.Int32);
            string sql = "";

            if(otAdmin == 0 || otAdmin == 1)
            {
                sql = @"SELECT deptid as id, deptName as descripcion FROM Departments 
                    WHERE deptid = @idDepto;";
            }
            else if (otAdmin == 2 )            
            {
                sql = @"WITH SubDepartamentos (deptid, deptName, supDeptId)
    AS
    (
        SELECT deptid, deptName, 0 FROM departments WHERE deptid = @idDepto
UNION ALL                
	    SELECT D.deptid, D.deptName, D.supDeptId FROM Departments D inner join SubDepartamentos Sub on Sub.deptid = D.SUPDEPTID
    )
SELECT deptid as id, deptName as descripcion FROM SubDepartamentos;";
            }
            else
            {
                sql = @"WITH SubDepartamentos (deptid, deptName, supDeptId)
    AS
    (
        SELECT deptid, deptName, 0 FROM departments WHERE SupDeptId = 0
UNION ALL                
	    SELECT D.deptid, D.deptName, D.supDeptId FROM Departments D inner join SubDepartamentos Sub on Sub.deptid = D.SUPDEPTID
    )
SELECT deptid as id, deptName as descripcion FROM SubDepartamentos;";
            }
            
            

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                db.Open();
                IEnumerable<EnumModel> contratos = await db.QueryAsync<EnumModel>(sql, parametros, commandType:CommandType.Text);
                return contratos.ToList();
            }
        }
        public async Task<List<EnumModel>> GetGruposSalarialesAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select idGrupo as id,       nomGrupo as descripcion     from dbo.GrupoSalarial;";

                db.Open();
                IEnumerable<EnumModel> contratos = await db.QueryAsync<EnumModel>(sql);
                return contratos.ToList();
            }
        }
        public async Task<List<EnumModel>> GetTiposEmpleadoAsync()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString(VariablesGlobales.PROPERTIME_DATABASE)))
            {
                string sql = @"select idTipo as id,       TipoEmpleado as descripcion     from dbo.TipoEmpleado;";

                db.Open();
                IEnumerable<EnumModel> tiposEmpleados = await db.QueryAsync<EnumModel>(sql);
                return tiposEmpleados.ToList();
            }
        }        


    }
}
